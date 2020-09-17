using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Lyrica.Data;
using Lyrica.Data.Config;
using Lyrica.Services.AutoRemoveMessage;
using Lyrica.Services.CodePaste;
using Lyrica.Services.Core;
using Lyrica.Services.Core.Messages;
using Lyrica.Services.Help;
using Lyrica.Services.Image;
using Lyrica.Services.Quote;
using Lyrica.Services.WebHooks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Lyrica.Bot
{
    public class Bot
    {
        private const bool AttemptReset = true;
        private static CancellationTokenSource? _mediatorToken;

        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

        private CancellationTokenSource _reconnectCts = null!;

        private static ServiceProvider ConfigureServices() =>
            new ServiceCollection().AddHttpClient().AddMemoryCache()
                .AddDbContext<LyricaContext>(ContextOptions, ServiceLifetime.Transient)
                .AddMediatR(c => c.Using<LyricaMediator>(),
                    typeof(Bot), typeof(LyricaMediator))
                .AddLogging(l => l
                    .AddSerilog(dispose: true, logger: Log.Logger))
                .AddSingleton<InteractiveService>()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<TwitterService>()
                .AddSingleton<CodePasteService>()
                .AddSingleton<IQuoteService, QuoteService>()
                .AddCodePaste()
                .AddAutoRemoveMessage()
                .AddCommandHelp()
                .AddImages()
                .BuildServiceProvider();

        private static void ContextOptions(DbContextOptionsBuilder db)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<LyricaConfig>()
                .Build();

            db.UseNpgsql(configuration.GetConnectionString("PostgreSQL"));
        }

        private static Task LogAsync(LogMessage message)
        {
            var severity = message.Severity switch
            {
                LogSeverity.Critical => LogEventLevel.Fatal,
                LogSeverity.Error    => LogEventLevel.Error,
                LogSeverity.Warning  => LogEventLevel.Warning,
                LogSeverity.Info     => LogEventLevel.Information,
                LogSeverity.Verbose  => LogEventLevel.Verbose,
                LogSeverity.Debug    => LogEventLevel.Debug,
                _                    => LogEventLevel.Information
            };

            Log.Write(severity, message.Exception, message.Message);

            return Task.CompletedTask;
        }

        public static async Task Main() => await new Bot().StartAsync();

        public async Task StartAsync()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .WriteTo.Console().CreateLogger();

            await using var services = ConfigureServices();
            var client = services.GetRequiredService<DiscordSocketClient>();
            var commands = services.GetRequiredService<CommandService>();
            var mediator = services.GetRequiredService<IMediator>();

            var config = new ConfigurationBuilder()
                .AddUserSecrets<LyricaConfig>()
                .Build();

            _reconnectCts = new CancellationTokenSource();
            _mediatorToken = new CancellationTokenSource();

            await new DiscordSocketListener(client, mediator)
                .StartAsync(_mediatorToken.Token);

            client.Disconnected += _ => ClientOnDisconnected(client);
            client.Connected += ClientOnConnected;

            client.Log += LogAsync;
            commands.Log += LogAsync;

            await client.LoginAsync(TokenType.Bot, config.GetValue<string>(nameof(LyricaConfig.Token)));
            await client.StartAsync();

            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            await Task.Delay(-1);
        }

        private Task ClientOnConnected()
        {
            Log.Debug("Client reconnected, resetting cancel tokens...");

            _reconnectCts.Cancel();
            _reconnectCts = new CancellationTokenSource();

            Log.Debug("Client reconnected, cancel tokens reset.");
            return Task.CompletedTask;
        }

        private Task ClientOnDisconnected(DiscordSocketClient client)
        {
            // Check the state after <timeout> to see if we reconnected
            Log.Information("Client disconnected, starting timeout task...");
            _ = Task.Delay(Timeout, _reconnectCts.Token).ContinueWith(async _ =>
            {
                Log.Debug("Timeout expired, continuing to check client state...");
                await CheckStateAsync(client);
                Log.Debug("State came back okay");
            });

            return Task.CompletedTask;
        }

        private async Task CheckStateAsync(DiscordSocketClient client)
        {
            // Client reconnected, no need to reset
            if (client.ConnectionState == ConnectionState.Connected) return;
            if (AttemptReset)
            {
                Log.Information("Attempting to reset the client");

                var timeout = Task.Delay(Timeout);
                var connect = client.StartAsync();
                var task = await Task.WhenAny(timeout, connect);

                if (task == timeout)
                {
                    Log.Fatal("Client reset timed out (task deadlocked?), killing process");
                    FailFast();
                }
                else if (connect.IsFaulted)
                {
                    Log.Fatal("Client reset faulted, killing process", connect.Exception);
                    FailFast();
                }
                else if (connect.IsCompletedSuccessfully)
                {
                    Log.Information("Client reset succesfully!");
                }

                return;
            }

            Log.Fatal("Client did not reconnect in time, killing process");
            FailFast();
        }

        private void FailFast()
            => Environment.Exit(1);
    }
}
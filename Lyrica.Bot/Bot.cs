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
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lyrica.Bot
{
    public class Bot
    {
        private static DiscordSocketListener _listener;
        private static CancellationTokenSource _mediatorToken;

        static Bot()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .WriteTo.Console().CreateLogger();
        }

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

        public static async Task Main()
        {
            await using var services = ConfigureServices();
            var client = services.GetRequiredService<DiscordSocketClient>();
            var commands = services.GetRequiredService<CommandService>();
            var mediator = services.GetRequiredService<IMediator>();

            var config = new ConfigurationBuilder()
                .AddUserSecrets<LyricaConfig>()
                .Build();

            client.Disconnected += ClientOnDisconnected;
            client.Connected += () => ClientOnConnected(client, mediator);

            client.Log += LogAsync;
            commands.Log += LogAsync;

            await client.LoginAsync(TokenType.Bot, config.GetValue<string>(nameof(LyricaConfig.Token)));
            await client.StartAsync();

            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            await Task.Delay(-1);
        }

        private static async Task ClientOnConnected(DiscordSocketClient client, IMediator mediator)
        {
            _listener = new DiscordSocketListener(client, mediator);
            _mediatorToken = new CancellationTokenSource();
            await _listener.StartAsync(_mediatorToken.Token);
        }

        private static async Task ClientOnDisconnected(Exception arg)
        {
            _mediatorToken.Cancel();
            await _listener.StopAsync(_mediatorToken.Token);
        }
    }
}
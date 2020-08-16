using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Lyrica.Data;
using Lyrica.Data.Config;
using Lyrica.Services.AutoRemoveMessage;
using Lyrica.Services.Core;
using Lyrica.Services.Core.Messages;
using Lyrica.Services.Help;
using Lyrica.Services.Image;
using Lyrica.Services.WebHooks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Lyrica.Bot
{
    public class Bot
    {
        private static ServiceProvider ConfigureServices() =>
            new ServiceCollection().AddHttpClient().AddMemoryCache()
                .AddDbContext<LyricaContext>(ContextOptions, ServiceLifetime.Transient)
                .AddMediatR(c => c.Using<HaloHaloMediator>(),
                    typeof(Bot), typeof(HaloHaloMediator))
                .AddLogging(l => l
                    .AddSerilog(dispose: true)
                    .SetMinimumLevel(LogLevel.Critical)
                    .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning))
                .AddSingleton<InteractiveService>()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<TwitterService>()
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
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            await using var services = ConfigureServices();
            var client = services.GetRequiredService<DiscordSocketClient>();
            var commands = services.GetRequiredService<CommandService>();
            var mediator = services.GetRequiredService<IMediator>();

            var config = new ConfigurationBuilder()
                .AddUserSecrets<LyricaConfig>()
                .Build();

            // Events
            var listener = new DiscordSocketListener(client, mediator);
            await listener.StartAsync(new CancellationToken());

            client.Log += LogAsync;
            commands.Log += LogAsync;

            // Login
            await client.LoginAsync(TokenType.Bot, config.GetValue<string>(nameof(LyricaConfig.Token)));
            await client.StartAsync();

            // Here we initialize the logic required to register our commands.
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
    }
}
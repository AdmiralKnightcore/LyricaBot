using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lyrica.Data;
using Lyrica.Data.Users;
using Lyrica.Services.Core.Messages;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lyrica.Services.Core
{
    public class CommandHandlingService : INotificationHandler<MessageReceivedNotification>
    {
        private readonly CommandService _commands;
        private readonly LyricaContext _db;
        private readonly DiscordSocketClient _discord;
        private readonly ILogger<CommandHandlingService> _log;
        private readonly IServiceProvider _services;

        public CommandHandlingService(
            IServiceProvider services,
            ILogger<CommandHandlingService> log,
            CommandService commands,
            DiscordSocketClient discord,
            LyricaContext db)
        {
            _commands = commands;
            _services = services;
            _discord = discord;
            _db = db;
            _log = log;

            _commands.CommandExecuted += CommandExecutedAsync;
        }

        public async Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            var rawMessage = notification.Message;

            if (!(rawMessage is SocketUserMessage message))
                return;

            if (message.Source != MessageSource.User)
                return;

            var author = notification.Message.Author;
            var user = await _db.Users.FindAsync(author.Id);
            if (user is null)
            {
                user = new User((IGuildUser) author);
                await _db.Users.AddAsync(user, cancellationToken);
            }

            _log.LogTrace($"{author} [#{notification.Message.Channel.Name}]: {notification.Message.Content}");
            user.LastSeenAt = DateTimeOffset.Now;
            await _db.SaveChangesAsync(cancellationToken);

            var argPos = 0;
            var context = new SocketCommandContext(_discord, message);
            if (!(message.HasStringPrefix("l!", ref argPos, StringComparison.OrdinalIgnoreCase) ||
                  message.HasMentionPrefix(_discord.CurrentUser, ref argPos)))
                return;

            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (result is null)
            {
                _log.LogWarning("Command with context {0} ran by user {1} is null.", context, message.Author);
                return;
            }

            if (!result.IsSuccess)
                await CommandFailedAsync(context, result).ConfigureAwait(false);
        }

        public Task CommandFailedAsync(ICommandContext context, IResult result) =>
            context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");

        public Task CommandExecutedAsync(
            Optional<CommandInfo> command,
            ICommandContext context,
            IResult result)
        {
            if (!command.IsSpecified)
                return Task.CompletedTask;

            if (result.IsSuccess)
                return Task.CompletedTask;

            // return CommandFailedAsync(context, result);
            return Task.CompletedTask;
        }

        public Task InitializeAsync() => _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }
}
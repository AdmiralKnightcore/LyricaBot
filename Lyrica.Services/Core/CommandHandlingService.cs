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

namespace Lyrica.Services.Core
{
    public class CommandHandlingService : INotificationHandler<MessageReceivedNotification>
    {
        private readonly CommandService _commands;
        private readonly LyricaContext _db;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        public CommandHandlingService(
            IServiceProvider services,
            CommandService commands,
            DiscordSocketClient discord,
            LyricaContext db)
        {
            _commands = commands;
            _services = services;
            _discord = discord;
            _db = db;

            _commands.CommandExecuted += CommandExecutedAsync;
        }

        public async Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            var rawMessage = notification.Message;

            if (!(rawMessage is SocketUserMessage message))
                return;

            if (message.Source != MessageSource.User)
                return;

            var user = await _db.Users.FindAsync(notification.Message.Author.Id);
            if (user == null)
            {
                user = new User((IGuildUser) notification.Message.Author);
                await _db.Users.AddAsync(user, cancellationToken);
            }

            user.LastSeenAt = DateTimeOffset.Now;
            await _db.SaveChangesAsync(cancellationToken);

            var argPos = 0;
            var context = new SocketCommandContext(_discord, message);
            if (!(message.HasStringPrefix("l!", ref argPos, StringComparison.OrdinalIgnoreCase) ||
                  message.HasMentionPrefix(_discord.CurrentUser, ref argPos)))
                return;

            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (!result.IsSuccess && !(result.Error == CommandError.BadArgCount &&
                                       result.ErrorReason == "The input text has too few parameters."))
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
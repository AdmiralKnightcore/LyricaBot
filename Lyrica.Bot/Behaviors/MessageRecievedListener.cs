using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lyrica.Data;
using Lyrica.Data.Users;
using Lyrica.Services.Core.Messages;
using MediatR;

namespace Lyrica.Bot.Behaviors
{
    public class MessageRecievedListener : INotificationHandler<MessageReceivedNotification>
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly LyricaContext _db;

        public MessageRecievedListener(
            CommandService commands, DiscordSocketClient discord,
            IServiceProvider services, LyricaContext db)
        {
            _commands = commands;
            _discord = discord;
            _services = services;
            _db = db;
        }

        public Task CommandFailedAsync(ICommandContext context, IResult result) =>
            context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");

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
                _db.Users.Add(user);
            }

            user.LastSeenAt = DateTimeOffset.Now;
            await _db.SaveChangesAsync(cancellationToken);

            var argPos = 0;
            var context = new SocketCommandContext(_discord, message);
            if (!(message.HasStringPrefix("l!", ref argPos) ||
                  message.HasMentionPrefix(_discord.CurrentUser, ref argPos)))
                return;

            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (!result.IsSuccess)
                await CommandFailedAsync(context, result).ConfigureAwait(false);
        }
    }
}
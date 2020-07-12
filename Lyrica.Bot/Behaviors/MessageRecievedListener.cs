using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lyrica.Services.Core.Messages;
using Lyrica.Services.Utilities;
using MediatR;

namespace Lyrica.Bot.Behaviors
{
    public class MessageRecievedListener : INotificationHandler<MessageReceivedNotification>
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly Cache<ulong, IEnumerable<string>> _prefixCache;

        public MessageRecievedListener(
            CommandService commands, DiscordSocketClient discord, IServiceProvider services)
        {
            _commands = commands;
            _discord = discord;
            _services = services;
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

            var argPos = 0;
            var context = new SocketCommandContext(_discord, message);
            var prefixes = _prefixCache[context.Guild.Id];
            if (!(message.HasStringPrefix("h!", ref argPos)
                  || message.HasMentionPrefix(_discord.CurrentUser, ref argPos)))
                if (!prefixes.Any(p => message.HasStringPrefix(p, ref argPos)))
                    return;

            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (!result.IsSuccess)
                await CommandFailedAsync(context, result).ConfigureAwait(false);
        }
    }
}
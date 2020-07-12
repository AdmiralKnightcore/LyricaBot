using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Lyrica.Services.Core.Messages;
using MediatR;

namespace Lyrica.Bot.Behaviors
{
    public class OnReadyListener : INotificationHandler<ReadyNotification>
    {
        public OnReadyListener(DiscordSocketClient client)
        {
            _client = client;
        }

        private readonly DiscordSocketClient _client;

        public async Task Handle(ReadyNotification notification, CancellationToken cancellationToken) { }
    }
}
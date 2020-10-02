using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Lyrica.Data;
using Lyrica.Data.Guilds;
using Lyrica.Services.Core.Messages;
using MediatR;

namespace Lyrica.Bot.Behaviors
{
    public class MissingGuildBehavior : INotificationHandler<ReadyNotification>
    {
        private readonly DiscordSocketClient _client;
        private readonly LyricaContext _db;

        public MissingGuildBehavior(LyricaContext db, DiscordSocketClient client)
        {
            _db = db;
            _client = client;
        }

        public async Task Handle(ReadyNotification notification, CancellationToken cancellationToken)
        {
            var missingGuilds = _client.Guilds
                .Where(socketGuilds =>
                    !_db.Guilds.ToList().Select(g => g.Id).Contains(socketGuilds.Id));

            foreach (var guild in missingGuilds)
            {
                var newGuild = new Guild
                {
                    Id = guild.Id,
                    Owner = guild.Id
                };
                _db.Guilds.Add(newGuild);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
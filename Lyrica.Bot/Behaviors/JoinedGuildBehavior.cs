using System.Threading;
using System.Threading.Tasks;
using Lyrica.Data;
using Lyrica.Data.Guilds;
using Lyrica.Services.Core.Messages;
using MediatR;

namespace Lyrica.Bot.Behaviors
{
    public class JoinedGuildBehavior : INotificationHandler<JoinedGuildNotification>
    {
        private readonly LyricaContext _db;

        public JoinedGuildBehavior(LyricaContext db)
        {
            _db = db;
        }

        public async Task Handle(JoinedGuildNotification notification, CancellationToken cancellationToken)
        {
            var guild = await _db.Guilds.FindAsync(notification.Guild.Id);
            if (guild == null)
            {
                guild = new Guild
                {
                    Id = notification.Guild.Id,
                    Owner = notification.Guild.OwnerId
                };
                _db.Guilds.Add(guild);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
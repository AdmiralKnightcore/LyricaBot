using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Lyrica.Data;
using Lyrica.Data.Users;
using Lyrica.Services.Core.Messages;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lyrica.Bot.Behaviors
{
    public class NewUserBehavior :
        INotificationHandler<ReactionAddedNotification>,
        INotificationHandler<UserJoinedNotification>
    {
        private const ulong GuildId = 728459950468104284;
        private const ulong RulesChannelId = 756037831616495626;
        private const ulong ReactionRoleMessageId = 756616075646337165;
        private const long FlowerId = 731456070601408612;

        private readonly LyricaContext _db;

        private readonly ulong[] _joinRoles =
        {
            729608027883438091 // Lyrica Stream Pings
        };

        private readonly ILogger<NewUserBehavior> _logger;

        private readonly ulong[] _reactRoles =
        {
            728609118486528101 // SiniGang
        };

        public NewUserBehavior(ILogger<NewUserBehavior> logger, LyricaContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task Handle(ReactionAddedNotification notification, CancellationToken cancellationToken)
        {
            var isChannel = notification.Reaction.Channel.Id == RulesChannelId;
            var isMessage = notification.Message.Id == ReactionRoleMessageId;

            if (!isChannel || !isMessage)
                return;

            if (!(notification.Reaction.User.Value is IGuildUser user))
            {
                _logger.LogWarning("Received a reaction event {0} but could not cast user {1} to IGuildUser",
                    notification.Reaction, notification.Reaction.User);
                return;
            }

            if (notification.Reaction.Emote is Emote emote &&
                emote.Id == FlowerId)
            {
                if (!(notification.Channel is IGuildChannel guildChannel))
                {
                    _logger.LogWarning("Could not cast {0} to IGuildChannel", notification.Channel);
                    return;
                }

                var guild = guildChannel.Guild;
                await user.AddRolesAsync(_reactRoles.Select(r => guild.GetRole(r)));
                _logger.LogInformation("Added roles {0} to user {1}", _reactRoles, user);
            }
        }

        public async Task Handle(UserJoinedNotification notification, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FindAsync(notification.GuildUser.Id);
            if (user is null)
            {
                user = new User(notification.GuildUser);
                await _db.Users.AddAsync(user, cancellationToken);
            }

            var guild = notification.GuildUser.Guild;
            if (guild.Id != GuildId)
            {
                _logger.LogInformation("Received a join event for {0} but did not match Id {1}", notification.GuildUser, notification);
                return;
            }

            await notification.GuildUser.AddRolesAsync(_joinRoles.Select(r => guild.GetRole(r)));
            _logger.LogInformation("Added roles {0} to user {1}", _joinRoles, notification.GuildUser);
        }
    }
}
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
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

        private readonly ulong[] _joinRoles =
        {
            729608027883438091 // Lyrica Stream Pings
        };

        private readonly ILogger<NewUserBehavior> _logger;

        private readonly ulong[] _reactRoles =
        {
            728609118486528101, // SiniGang
            728903243035443251  // 4 hearts
        };

        public NewUserBehavior(ILogger<NewUserBehavior> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ReactionAddedNotification notification, CancellationToken cancellationToken)
        {
            if (!(notification.Reaction.User.Value is IGuildUser user))
            {
                _logger.LogWarning("Received a reaction event {0} but could not cast user {1} to IGuildUser",
                    notification.Reaction, notification.Reaction.User);
                return;
            }

            var isFlower = notification.Reaction.Emote.Name == "<:LyricaFlower:731456070601408612>";
            var isChannel = notification.Reaction.Channel.Id == RulesChannelId;
            var isMessage = (await notification.Message.GetOrDownloadAsync())?.Id == ReactionRoleMessageId;
            if (isFlower && isChannel && isMessage)
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
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Lyrica.Services.Core.Messages;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lyrica.Services.Quote
{
    public class MessageLinkBehavior :
        INotificationHandler<MessageReceivedNotification>,
        INotificationHandler<MessageUpdatedNotification>
    {
        private static readonly Regex Pattern = new Regex(
            @"(?<Prelink>\S+\s+\S*)?(?<OpenBrace><)?https?://(?:(?:ptb|canary)\.)?discord(app)?\.com/channels/(?<GuildId>\d+)/(?<ChannelId>\d+)/(?<MessageId>\d+)/?(?<CloseBrace>>)?(?<Postlink>\S*\s+\S+)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public MessageLinkBehavior(
            DiscordSocketClient discordClient,
            ILogger<MessageLinkBehavior> log,
            IQuoteService quoteService)
        {
            DiscordClient = discordClient;
            Log = log;
            QuoteService = quoteService;
        }

        private DiscordSocketClient DiscordClient { get; }

        private ILogger<MessageLinkBehavior> Log { get; }

        private IQuoteService QuoteService { get; }

        public async Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            await OnMessageReceivedAsync(notification.Message);
        }

        public async Task Handle(MessageUpdatedNotification notification, CancellationToken cancellationToken)
        {
            var cachedMessage = await notification.OldMessage.GetOrDownloadAsync();

            if (cachedMessage is null)
                return;

            if (Pattern.IsMatch(cachedMessage.Content))
                return;

            await OnMessageReceivedAsync(notification.NewMessage);
        }

        private async Task OnMessageReceivedAsync(IMessage message)
        {
            if (!(message is IUserMessage userMessage)
                || !(userMessage.Author is IGuildUser guildUser)
                || guildUser.IsBot
                || guildUser.IsWebhook)
                return;

            foreach (Match match in Pattern.Matches(message.Content))
            {
                // check if the link is surrounded with < and >. This was too annoying to do in regex
                if (match.Groups["OpenBrace"].Success && match.Groups["CloseBrace"].Success)
                    continue;

                if (ulong.TryParse(match.Groups["GuildId"].Value, out var guildId)
                    && ulong.TryParse(match.Groups["ChannelId"].Value, out var channelId)
                    && ulong.TryParse(match.Groups["MessageId"].Value, out var messageId))
                    try
                    {
                        var channel = DiscordClient.GetChannel(channelId);

                        if (channel is ITextChannel { IsNsfw: true }) return;

                        if (channel is IGuildChannel guildChannel &&
                            channel is ISocketMessageChannel messageChannel)
                        {
                            var currentUser = await guildChannel.Guild.GetCurrentUserAsync();
                            var channelPermissions = currentUser.GetPermissions(guildChannel);

                            if (!channelPermissions.ViewChannel) return;

                            var cacheMode = channelPermissions.ReadMessageHistory
                                ? CacheMode.AllowDownload
                                : CacheMode.CacheOnly;

                            var msg = await messageChannel.GetMessageAsync(messageId, cacheMode);

                            if (msg == null) return;

                            var success = await SendQuoteEmbedAsync(msg, guildUser, userMessage.Channel);
                            if (success
                                && string.IsNullOrEmpty(match.Groups["Prelink"].Value)
                                && string.IsNullOrEmpty(match.Groups["Postlink"].Value))
                                await userMessage.DeleteAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogError(ex, "An error occured while attempting to create a quote embed.");
                    }
            }
        }

        private async Task<bool> SendQuoteEmbedAsync(IMessage message, IGuildUser quoter, IMessageChannel targetChannel)
        {
            var success = false;
            await QuoteService.BuildRemovableEmbed(message, quoter,
                async embed => //If embed building is unsuccessful, this won't execute
                {
                    success = true;
                    return await targetChannel.SendMessageAsync(embed: embed.Build());
                });

            return success;
        }
    }
}
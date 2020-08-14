using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Discord;
using LinqToTwitter;
using Lyrica.Data.Config;
using Microsoft.Extensions.Configuration;
using Color = Discord.Color;

namespace Lyrica.Services.WebHooks
{
    public class TwitterService
    {
        private readonly TwitterContext _twitterClient;

        public TwitterService()
        {
            var secrets = new ConfigurationBuilder()
                .AddUserSecrets<LyricaConfig>()
                .Build().GetSection(nameof(LyricaConfig.TwitterSecrets));

            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = secrets.GetValue<string>(nameof(LyricaConfig.TwitterSecrets.ConsumerKey)),
                    ConsumerSecret = secrets.GetValue<string>(nameof(LyricaConfig.TwitterSecrets.ConsumerSecret)),
                    AccessToken = secrets.GetValue<string>(nameof(LyricaConfig.TwitterSecrets.AccessToken)),
                    AccessTokenSecret = secrets.GetValue<string>(nameof(LyricaConfig.TwitterSecrets.AccessTokenSecret))
                }
            };

            _twitterClient = new TwitterContext(auth);
        }

        public async Task<EmbedBuilder> AddTweetInfo(EmbedBuilder builder, ulong tweetId, int? photoNumber = null)
        {
            var status = await GetTweet(tweetId);

            if (status is null)
                throw new ArgumentOutOfRangeException(nameof(tweetId), "The tweet does not exist");

            var statusLink = $"https://twitter.com/{status.User.ScreenNameResponse}/status/{status.ID}";

            builder
                .WithDescription(HttpUtility.HtmlDecode(status.FullText))
                .WithColor((Color) ColorTranslator.FromHtml($"#{status.User.ProfileLinkColor}"))
                .WithAuthor(status.User.Name, status.User.ProfileImageUrl, statusLink);

            if (photoNumber > status.Entities.MediaEntities.Count)
                photoNumber = status.Entities.MediaEntities.Count;
            else if (photoNumber < 0)
                photoNumber = 0;

            if (status.Entities.MediaEntities.Any())
                builder.WithImageUrl(status.Entities.MediaEntities[photoNumber ?? 0].MediaUrl);

            return builder;
        }

        private async Task<Status> GetTweet(ulong tweetId)
        {
            return await _twitterClient.Status
                .Where(tweet =>
                    tweet.Type == StatusType.Show &&
                    tweet.ID == tweetId &&
                    tweet.TweetMode == TweetMode.Extended)
                .FirstOrDefaultAsync();
        }
    }
}
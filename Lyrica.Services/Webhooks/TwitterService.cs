using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using LinqToTwitter;
using Lyrica.Data.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

        public async Task<EmbedBuilder> AddInformation(EmbedBuilder builder, ulong tweetId)
        {
            var status = await _twitterClient.Status
                .Where(tweet =>
                    tweet.Type == StatusType.Show &&
                    tweet.ID == tweetId &&
                    tweet.TweetMode == TweetMode.Extended)
                .Include(x => x.Entities)
                .FirstOrDefaultAsync();

            if (status is null)
                throw new ArgumentNullException(nameof(status));

            builder
                .WithDescription(status.FullText)
                .WithImageUrl(status.Entities.MediaEntities.First().MediaUrl)
                .WithAuthor(status.User.ScreenName, status.User.ProfileImageUrl, status.Entities.UrlEntities[0].Url);

            return builder;
        }
    }
}
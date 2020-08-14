using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Lyrica.Services.Core.Attributes;
using Lyrica.Services.Interactive;
using Lyrica.Services.WebHooks;
using static Lyrica.Bot.Modules.AnnounceModule.StreamPingArguments;

namespace Lyrica.Bot.Modules
{
    [Group("announce")]
    [Summary("Group of commands of announcing various pre-formatted embeds")]
    public class AnnounceModule : InteractivePromptBase
    {
        public enum StreamPingArguments
        {
            Description,
            Author,
            ImageUrl,
            Footer
        }

        private readonly Regex _tweetRegex;

        private readonly TwitterService _twitterService;

        public AnnounceModule(TwitterService twitterService)
        {
            _twitterService = twitterService;
            _tweetRegex = new Regex(@"
                (https?://)?twitter\.com/(?<user>\w{1,15})/status/(?<tweetId>[0-9]+)
                (/photo/(?<photoNumber>([1-9][0-9]*)))?",
                RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        }

        public async Task StreamPingAsync(ulong tweetId, [Remainder] string title)
        {
            var prompts = new PromptCollection<StreamPingArguments>(this)
                .WithPrompt(Description, "Enter the message of the announcement");
        }

        [Command("stream")]
        public async Task StreamPingAsync(string tweetLink, [Remainder] string? message = null)
        {
            var embed = new EmbedBuilder()
                .WithFooter("Stream starts at", "https://abs.twimg.com/icons/apple-touch-icon-192x192.png");
            var tweet = _tweetRegex.Match(tweetLink);

            if (!tweet.Success)
            {
                await ReplyAsync("That's an invalid tweet!");
                return;
            }

            if (!ulong.TryParse(tweet.Groups["tweetId"].Value, out var tweetId))
            {
                await ReplyAsync("The tweet number is invalid!");
                return;
            }

            if (tweet.Groups["photoNumber"].Success)
            {
                var photoNumber = int.Parse(tweet.Groups["photoNumber"].Value);
                await _twitterService.AddTweetInfo(embed, tweetId, photoNumber - 1);
            }
            else
            {
                await _twitterService.AddTweetInfo(embed, tweetId);
            }

            await ReplyAsync(message, embed: embed.Build());
        }

        [Command("test")]
        public async Task TestAsync(ulong tweetId)
        {
            var embed = new EmbedBuilder()
                .WithFooter("Stream starts at", "https://abs.twimg.com/icons/apple-touch-icon-192x192.png");

            await _twitterService.AddTweetInfo(embed, tweetId);

            await ReplyAsync("Test", embed: embed.Build());
        }

        [NamedArgumentType]
        public class AnnouncementArguments
        {
            [Description("Text Channel to post the ping in")]
            public ITextChannel? Channel { get; set; } = null!;

            [Description("This is the overriden title")]
            public string? Title { get; set; } = null!;
        }
    }
}
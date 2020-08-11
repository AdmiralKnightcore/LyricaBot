using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Webhook;
using Lyrica.Services.Interactive;
using Lyrica.Services.WebHooks;
using static Lyrica.Bot.Modules.WebhookModule.StreamPingArguments;

namespace Lyrica.Bot.Modules
{
    [Group("announce")]
    [Summary("Group of commands of announcing various pre-formatted embeds")]
    public class WebhookModule : InteractivePromptBase
    {
        public enum StreamPingArguments
        {
            Description,
            Author,
            ImageUrl,
            Footer
        }

        private readonly DiscordWebhookClient _streamClient;
        private readonly DiscordWebhookClient _testClient;
        private readonly TwitterService _twitterService;

        public WebhookModule(TwitterService twitterService)
        {
            _twitterService = twitterService;

            _streamClient =
                new DiscordWebhookClient(
                    @"");

            _testClient =
                new DiscordWebhookClient(
                    @"");
        }

        public async Task StreamPingAsync([Remainder] string title)
        {
            var prompts = new PromptCollection<StreamPingArguments>(this)
                .WithPrompt(Description, "Enter the description of the announcement");
        }

        [Command("test")]
        public async Task TestAsync(ulong tweetId)
        {
            var embed = new EmbedBuilder()
                .WithFooter("Stream starts at", "https://abs.twimg.com/icons/apple-touch-icon-192x192.png");

            await _twitterService.AddInformation(embed, tweetId);

            await _testClient.SendMessageAsync("Test", embeds: new[] { embed.Build() });
        }
    }
}
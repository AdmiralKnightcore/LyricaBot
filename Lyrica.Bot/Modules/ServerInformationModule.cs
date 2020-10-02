using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Lyrica.Services.Utilities;

namespace Lyrica.Bot.Modules
{
    [Name("Server Rules")]
    public class ServerInformationModule : ModuleBase<SocketCommandContext>
    {
        private const string _serverAvatarUrl = "https://chito.ge/5cH9TCA.gif?_raw=true";

        public static List<(string title, string description)> MainRules { get; } = new List<(string, string)>
        {
            ("#1 - Please be respectful.",
                "Treat people with respect. Do not bring personal fights in the server, " +
                "that other people are not involved in. " +
                "If you want to fight, do it somewhere else, not here. " +
                "Listen to the moderators, and do not make other community members uncomfortable."),

            ("#2 - No offensive content.",
                "No offensive messages, nicknames, usernames, or avatars. " +
                "This includes but not limited to **racial slurs**, **insults**, and **mockery**. " +
                "Words like the **n word**, its variants, and others."),

            ("#3 No drama.",
                "Don't start drama in the server. If you were warned, and you feel its unfair, " +
                "please head onto <@!575252669443211264> to talk about it with other moderators. " +
                "Similarly, this means that discussing about bans and the reasons thereof fall under here. " +
                "This also includes talking about any sensitive topics, " +
                "such as religion topics, politics, extremely controversial things, etc."),

            ("#4 - No spamming.",
                "No spamming. This includes mention spams, " +
                "loud obnoxious noises in voice channels, character spams, emote spams and image spams. " +
                "Extremely off-topic topics that aren't warranted for also are included here."),

            ("#5 - Speak English!",
                "Primarily, we want to welcome the entire community so you should speak English as much as you can. " +
                "Occasionally, it is okay to speak Filipino, when the topic warrants it, like quoting someone. " +
                "If you want to converse in Filipino, then you may do so in <#728465080890556448>"),

            ("#6 - No NSFW or NSFL.",
                "Please do not post NSFW or NSFL content in any channel. " +
                "This includes **gore**, **screamer links**, **pornography**, " +
                "**hentai**, **nudity**, **animal** abuse and **death**."),

            ("#7 - Use appropriate channels.",
                "This rule isn't meant to break the flow of the conversation, but rather to make it continue. " +
                "Topics should generally be in their specific channel if it is too nuanced for <#728459951068151821>"),

            ("#8 - No harassment",
                "This includes sexual harassment, bullying, or encouraging of harassment directly to other members. " +
                "If someone is specifically targeting you because of this server, block them. " +
                "If they take it a step further, contact the ModMail."),

            ("#9 - Excessive Profanity",
                "Profanity or swearing is allowed, however **do not go overboard**, " +
                "or **do not direct it at someone to instigate drama**."),

            ("#10 - Other VTubers",
                "You may talk about other VTubers in the <#747277962419896363> channel. " +
                "Generally, you will have to move here if you want to talk about someone in detail. " +
                "The general channels can accept talking about VTubers as the concept itself, rather than a specific one. " +
                Environment.NewLine + Environment.NewLine +
                "Similarly, when there are collabs taking place on another channel, they will be held here. " +
                "If the collab is on the main channel, then the <#728459951068151821> channel will work."),

            ("#11 - Respect the moderators.",
                "If moderators tell you to stop, please comply. " +
                "The mods have authority to stop a topic based on their discretion, " +
                "and the rules are general guidelines for you and the community to follow. " +
                "They are not made for it to be meticulously skirted around, " +
                "or the words be scrutinized because of nuances or strict definitions."),

            ("#12 - Do not doxx people.",
                "This should be obvious, but do not come to this server looking for private information, " +
                "unreleased, or otherwise release information that the recipient is not comfortable being divulged into public. " +
                "This goes in hand with you as well, do not provide information that you might regret later, " +
                "as it might be hard to take care of it and clean up later."),

            ("#13 - Do not ping Lyrica directly.",
                "Please respect people's time, as they can get really busy. " +
                "Do not ping her when she does not appear in the chat, " +
                "and do not keep pinging her when she's around. If you need to contact her, " +
                "direct it to <@!575252669443211264> instead." +
                Environment.NewLine + Environment.NewLine +
                "Do not delete pings when you accidentally ping someone (aka a ghost ping), " +
                "so that they will be able to tell who pinged them."),

            ("#14 - Follow Individual Channel Rules.",
                "There are rules inside individual channels under their channel description, " +
                "make sure to read them. These rules apply specifically to the channel itself."),

            ("#15 - Follow the Discord's Terms of Service.",
                "Please follow Discord's Terms of Service. " +
                "Breaking of their Terms of Service is a **guaranteed ban**.")
        };

        [Command("information")]
        public async Task PostInformationAsync(ITextChannel channel)
        {
            var embed = new EmbedBuilder();
        }

        [Command("rule")]
        [Summary("Views that specific rule")]
        [Alias("rules")]
        public Task ViewRuleAsync([Summary("A number 1-15")] uint ruleNo)
        {
            if (ruleNo > MainRules.Count || ruleNo < 1)
                return ReplyAsync($"There are only 1-{MainRules.Count} rules!");

            var (title, description) = MainRules[(int) ruleNo - 1];
            var embed = new EmbedBuilder()
                .WithColor(Rgb(247, 209, 211))
                .WithTitle(title)
                .WithDescription(description)
                .WithUserAsAuthor(Context.User);

            return ReplyAsync(embed: embed.Build());
        }

        [Command("update rules")]
        public async Task UpdateRulesAsync()
        {
            var reactRole = new EmbedBuilder()
                .WithColor(Rgb(135, 203, 250))
                .WithTitle("I have read the rules")
                .WithThumbnailUrl("https://cdn.discordapp.com/emojis/731456070601408612.png?v=1")
                .WithImageUrl(
                    $"https://cdn.discordapp.com/banners/{Context.Guild.Id}/{Context.Guild.BannerId}.png?size=4096")
                .WithDescription(
                    "After reading the rules, please react here to be able to join <:LyricaApproves:731399021326893148>\r\n" +
                    "If this does not work, please contact the <@!575252669443211264> as seen below.");

            var reactMessage = (RestUserMessage) await Context.Guild
                .GetTextChannel(756037831616495626)
                .GetMessageAsync(756616075646337165);
            await reactMessage.ModifyAsync(x => x.Embed = reactRole.Build());
        }

        [RequireOwner]
        [Command("post rules")]
        public async Task PostRulesAsync(ITextChannel? channel = null)
        {
            channel ??= (ITextChannel) Context.Channel;
            var pink = Rgb(247, 209, 211);

            // Introduction
            var introduction = new EmbedBuilder()
                .WithColor(pink)
                .WithTitle($"Welcome to {Context.Guild.Name}")
                .WithDescription(
                    "We hope you enjoy your stay in here!" +
                    Environment.NewLine + Environment.NewLine +
                    "**This server is a hub for Lola's community, please enjoy your stay!**" +
                    Environment.NewLine + Environment.NewLine +
                    "Before you can enter, please read the rules carefully. Afterwards, you may accept the rules by reacting <:LyricaFlower:731456070601408612> at the bottom.")
                .WithThumbnailUrl(_serverAvatarUrl)
                .WithImageUrl("https://chito.ge/2L3DTUk.png?_raw=true");

            // Rules
            var rules = new EmbedBuilder()
                .WithColor(pink)
                .WithTitle("Server Rules & Regulations")
                .WithDescription(
                    "These are the general guidelines of the server. Take note that they are not meant to be skirted around. " +
                    "Mods can still warn you based on their discretion. They are also not made to be strict; they are in place to make sure that everyone feels welcome and has fun in the server.")
                .WithFooter("※ These rules were last updated")
                .WithCurrentTimestamp();

            foreach (var rule in MainRules) rules.AddField(rule.title, rule.description);

            // Voice Channel Rules
            var voice = new EmbedBuilder()
                .WithColor(pink)
                .WithTitle("Voice Channels' Rules & Regulations")
                .WithDescription(
                    "Here are the rules and regulations for voice channels. " +
                    "Read carefully. The rules in voice channels are more laid-back, " +
                    "and less stricter than text-channel rules.")
                .AddField("#1 - No ear-rape or bass boosted content.",
                    "No **ear-rape** or **bass boosted content** in music bots or with a soundboard.")
                .AddField("#2 - Don't start fights.", "Be respectful, **don't start a fight** in voice channels.")
                .AddField("#3 - Don't be annoying.", "Refrain from being **annoying** to users.")
                .WithFooter(
                    "※ There may be some situations not covered by these rules or times where the rule may not fit the situation. If this happens the moderators are trusted to handle the situation appropriately.");

            // Stream rules
            var chatEnglish = new[]
            {
                "　※ Be nice! No fighting!",
                "　※ Don't spam! Do not troll!",
                "　※ Be nice to the moderators and chat!",
                "　※ Please do not mention or give any comment about other Vtuber during the livestream if the topic isn't started first by the current streamer first because it would be considered impolite!",
                "　※ In the same way, please do not mention me in other streams!",
                "　※ If someone is misbehaving in chat, please ignore it! It makes cleaning up harder for the mods!",
                "　※ Do not have off topic discussions in the chat! Please respect the stream!"
            };

            var chatFilipino = new[]
            {
                "　※ Maging mabait! Walang away!",
                "　※ Huwag mag-spam! Huwag mag-troll!",
                "　※ Irespeto ang  mga moderator at chat!",
                "　※ Huwag banggitin ang ibang Vtuber sa panahon ng livestream kung hindi muna sinimulan ang paksa ng streamer!",
                "　※ Huwag mo rin banggitin ako sa iba pang mga streams!",
                "　※ Kung mayroong pasaway sa chat, huwag pansinin sila!",
                "　※ Huwag pag-usapan ang mga topics na hindi tungkol sa stream!"
            };

            var chatJapanese = new[]
            {
                "　※ 喧嘩しないで下さい。",
                "　※ スパムをしないで下さい。",
                "　　 荒らしをしないで下さい。",
                "　※ 上に記載されているように、",
                "　　 チャットの方々やライバーに対して礼儀正しく接して下さい。",
                "　※ 配信中に内容に関係ない他のＶチューバーについて言及したりコメントしないで下さい。",
                "　※ 同じように、他のストリームで私の事を言及しないで下さい。",
                "　※ もし誰かがチャットを荒らしていたら、無視してください。",
                "　　 そうしないとBAN対応が大変になります。",
                "　※ チャットでは配信内容に関係のない議論はしないで下さい。",
                "　　 流れを尊重いただくようお願いいたします。"
            };

            var stream = new EmbedBuilder()
                .WithColor(Rgb(100, 65, 165))
                .WithTitle("Stream Rules & Regulations")
                .AddField("If you want me to understand you, use the language stated in the title!",
                    string.Join(Environment.NewLine, chatEnglish))
                .AddField("Kung nais mong maintindihan kita, gamitin mo ang wika na nakasaad sa pamagat!",
                    string.Join(Environment.NewLine, chatFilipino))
                .AddField("私の配信をご覧いただく際は以下の事を守ってください。",
                    string.Join(Environment.NewLine, chatJapanese))
                .WithFooter("※ These rules were last updated")
                .WithCurrentTimestamp();

            // Rule acceptance
            var flower = Emote.Parse("<:LyricaFlower:731456070601408612>");
            var reactRole = new EmbedBuilder()
                .WithColor(Rgb(135, 203, 250))
                .WithTitle("I have read the rules")
                .WithThumbnailUrl("https://cdn.discordapp.com/emojis/731456070601408612.png?v=1")
                .WithDescription(
                    "After reading the rules, please react here to be able to join <:LyricaApproves:731399021326893148>\r\n" +
                    "If this does not work, please contact the ModMail as seen below.");

            // ModMail
            var modMail = new EmbedBuilder()
                .WithColor(Rgb(65, 105, 238))
                .WithTitle("Moderation Contacts and Inquiries")
                .WithDescription(
                    "If you have any questions or want to contact Lyrica, you must contact <@!575252669443211264> to relay your message to us. You can **directly message the bot** and **get a ticket** and we will reply to your message." +
                    Environment.NewLine + Environment.NewLine +
                    "As for anything more important, contact the head mods <@!189307395632005121> or <@!246903976937783296>--but it's advisable to use mod mail instead.")
                .WithThumbnailUrl(
                    "https://cdn.discordapp.com/avatars/575252669443211264/de604039e893f9ce7fa6870b7d3193c7.png?size=1024")
                .WithFooter("※ You can also contact an individual moderator if it is only involving them.");

            // Warning System
            var hearts = new[]
            {
                "<@&728903243035443251> Normal. This is the default amount.",
                "<@&728903246294548532> **5 minutes** mute.",
                "<@&728903249129635902> **12 hours** mute.",
                "<@&728903250916409425> **Kick + 24 hours** mute when rejoining.",
                "<@&728903408790011914> **Ban**"
            };

            var warning = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("Warning System")
                .WithDescription(
                    "The health system is a strike system to keep track of how many infractions the user has committed in the server. If you break the rules, you get damaged!")
                .WithThumbnailUrl("https://chito.ge/4sLzLhF.png?_raw=true")
                .AddField("How does it work?",
                    "Each member starts with **4 hearts**, if they break a rule, they lose 1 heart and get stunned (which means muted)--the duration of this stun depends on the amount of hearts you have left.")
                .AddField("Health Recovery",
                    "One of your warnings will be gone after 3 weeks of following the rules. So, do behave!")
                .AddField("Warning Reprimands",
                    string.Join(Environment.NewLine, hearts))
                .WithFooter(
                    "※ Some rules or situations may be too serious in which this system will be ignored and they will be dealt accordingly.");

            // Sending
            await channel.SendMessageAsync(embed: introduction.Build());

            await channel.SendMessageAsync(embed: rules.Build());
            await channel.SendMessageAsync(embed: voice.Build());
            await channel.SendMessageAsync(embed: stream.Build());
            var reactRoleMessage = await channel.SendMessageAsync(embed: reactRole.Build());
            await reactRoleMessage.AddReactionAsync(flower);

            await channel.SendMessageAsync(embed: modMail.Build());
            await channel.SendMessageAsync(embed: warning.Build());
        }

        private static Color Rgb(int red, int green, int blue) =>
            (Color) System.Drawing.Color.FromArgb(255, red, green, blue);
    }
}
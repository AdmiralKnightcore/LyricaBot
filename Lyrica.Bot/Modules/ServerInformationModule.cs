﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Lyrica.Services.Utilities;

namespace Lyrica.Bot.Modules
{
    [Name("Server Rules")]
    public class ServerInformationModule : ModuleBase<SocketCommandContext>
    {
        public enum RulePart
        {
            Introduction,
            Main,
            Voice,
            Stream,
            React,
            ModMail,
            Warning,
            Channel,
            Role,
            Social
        }

        private const string _serverAvatarUrl = "https://chito.ge/5cH9TCA.gif?_raw=true";

        private readonly Color _pink = new Color(247, 209, 211);
        private readonly Regex _roleNameRegex = new Regex(@"^— .+? —$", RegexOptions.Compiled);

        private IEnumerable<uint> MainRulesNumbers => MainRules
            .SelectMany(c =>
                c.rules.Select(r => r.ruleNo));

        private IEnumerable<uint> VoiceRulesNumbers => VoiceRules
            .Select(r => r.ruleNo);

        private static (string category, (uint ruleNo, string title, string description)[] rules)[] MainRules { get; } =
        {
            ("Respect & Courtesy", rules: new (uint ruleNo, string title, string description)[]
            {
                (1, "Please be Respectful to Everyone.",
                    "Do not make other members uncomfortable, " +
                    "listen to the moderators when they tell you to stop or you're going far. " +
                    "Do not bring personal problems to the server, do it somewhere else." +
                    Environment.NewLine + Environment.NewLine +
                    "Elitism, condescending, or insulting other people is dishonorable and insensitive. " +
                    "Respect others as you want others to respect you."),

                (2, "No Offensive Content.",
                    "No offensive messages, nicknames, usernames, or avatars. " +
                    "This includes but not limited to racial slurs, insults, and mockery." +
                    Environment.NewLine + Environment.NewLine +
                    "Please do not post NSFW or NSFL content in any channel. " +
                    "This includes but is not limited to: " +
                    "Gore, Screamer Links, Pornography, Hentai, " +
                    "Nudity, Animal Abuse, and Death."),

                (3, "No Harassment.",
                    "This includes sexual harassment, bullying, or encouraging of " +
                    "harassment directly to other members. " +
                    "If someone is specifically targeting you because of this server, block them. " +
                    "If they take it a step further, contact ModMail."),

                (4, "Avoid Drama.",
                    "Please do not bring up topics that incite drama, especially on the server. " +
                    "Warns, bans, issues unrelated to the server " +
                    "is not something that should be talked about in the public channels." +
                    "" +
                    "If you want to clarify your warning, direct it to @ModMail rather than on the server. " +
                    "Refrain from prying into issues that are done already, " +
                    "otherwise you can talk about it with the mods if it’s something really important.")
            }),
            ("Etiquette & Chat Discipline", new (uint ruleNo, string title, string description)[]
            {
                (5, "No Spamming.",
                    "No spamming. This includes mention spams, character spams, emote spams and image spams. " +
                    "Extremely off-topic topics that aren't warranted for are also included here. " +
                    "Minimize quoting messages especially long ones or when it's the channel’s topic."),

                (6, "This is an International Community.",
                    "Primarily, we want to welcome the entire community so " +
                    "you should speak English as much as you can. " +
                    "Occasionally, it is okay to write Filipino words, " +
                    "when the topic warrants it like quoting a song. " +
                    "If you want to converse in Filipino, then you may do so in <#761749895027228692>"),

                (7, "Use Appropriate Channels.",
                    "This rule isn't meant to break the flow of the conversation, " +
                    "but rather to make it continue after a nuanced topic starts from the main channels. " +
                    "Converse in a channel that best suits the topics so that you may be undisturbed about other conversations." +
                    Environment.NewLine + Environment.NewLine +
                    "There are rules inside individual channels under their channel description, " +
                    "make sure to read them. These rules apply specifically to the channel itself."),

                (8, "Listen to the Moderators.",
                    "If moderators tell you to stop, please comply. " +
                    "The mods have authority to stop a topic based on their discretion, " +
                    "and the rules are general guidelines for you and the community to follow.")
            }),
            ("Community", new (uint ruleNo, string title, string description)[]
            {
                (9, "Do not Unnecessarily Ping People.",
                    "Please respect people's time, as they can get really busy. " +
                    "Do not delete pings when you accidentally ping someone (ghost ping)." +
                    Environment.NewLine + Environment.NewLine +
                    "Do not ping Lyrica when she does not appear in the chat, " +
                    "and do not keep pinging her when she's around. " +
                    "If you need to contact her, direct it to <@!575252669443211264> instead."),

                (10, "Other VTubers.",
                    "You may talk about other VTubers in the <#747277962419896363> channel. " +
                    "Move in this channel when you talk about VTubers, rather than VTubing as a concept." +
                    Environment.NewLine + Environment.NewLine +
                    "When Lyrica has a collaboration with other VTubers " +
                    "then you may chat about it in the main channels."),

                (11, "Respect to Privacy.",
                    "This is Lyrica’s community server, do remember that it is public. " +
                    "Personal information is not appropriate for everyone to know. " +
                    "With that said, Full names, Addresses, and Confidential Information " +
                    "are not allowed even if it was yours. " +
                    Environment.NewLine + Environment.NewLine +
                    "Information about yourself is discouraged unless there is proper context. " +
                    "Please respect other people's rights to their privacy as well, " +
                    "just as you would want others to respect yours."),

                (12, "Follow the Discord's Terms of Service.",
                    "Because this community is in Discord, " +
                    "you are required to follow Discord's Terms of Service.")
            })
        };

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private static (uint ruleNo, string title, string description)[] VoiceRules =
        {
            (13, "No ear-rape or bass boosted content.",
                "Don't play music or sounds that are considered ear-rape " +
                "or harming to other's hearing."),

            (14, "Be Mindful of others.",
                "Be respectful, don't start a fight in voice channels. " +
                "Making inappropriate sounds or disturbing noises that may " +
                "disrupt discussions or make other people uncomfortable is not allowed." +
                Environment.NewLine + Environment.NewLine +
                "Refrain from being annoying to users."),

            (15, "No Video.",
                "Only streaming is allowed, " +
                "do not use video of your face for your own safety."),

            (16, "Use the Appropriate Voice Channel.",
                "Use the voice channel the topic you're doing fits in. " +
                "If you're going to be playing games, then use the gaming-vc channels.")
        };

        private static Dictionary<ulong, string> ChannelDescriptions { get; } = new Dictionary<ulong, string>
        {
            // Important
            [756037831616495626] = "The channel for the rules of the server, " +
                                   "please read them if you don’t want trouble on the server. ",
            [750764789063745627] = "The channel you are looking at right now.",
            [728465580331630604] = "The main channel for announcements. " +
                                   "This shows the main updates for the server and the community.",

            // Lyrica Ch.
            [728739356139061309] = "The main channel that notifies you on stream schedule and updates.",
            [728471208005337109] = "Post suggestions for streams here.",

            // Chat
            [761744127351586826] = "Please converse in English. You may speak other languages, " +
                                   "however always speak in English to prevent alienation. " +
                                   "Refrain from posting excessive image memes, post them on <#751118105480265789> instead.",
            [761749895027228692] = "Pwedeng makipag talakayan sa wikang tagalog at iba pang mga dayalekto dito. " +
                                   "Bawasan ang mga di nararapat na talakayan. Laging sundin ang batas sa #rules.",
            [728608245752659999] = "If you’re stuck or don’t know what to do, ask here! " +
                                   "This is also the main place for greeting like #general.",
            [728477659151204444] = "Introduce yourself here and let us know about you!",

            // Community
            [729620759906877451] = "Post suggestions for the server and community here.",
            [737796981019246592] = "Song Covers uploaded by Lyrica that is only exclusive for the discord server.",
            [735555361255194634] = "Channel for the official Lore of the 77th Mahou Shoujo Generation: " +
                                   "This is connected to lyrica’s character story and revolves around the world around her.",
            [728853984076496928] = "Updates you on Lyrica’s tweets.",
            [728471575187423304] = "Channel to post Lyrica fanart " +
                                   "(This is specifically a fanart channel for lyrica only, " +
                                   "they are also being monitored to be collected and featured for stream)" + Environment.NewLine +
                                   "[Please appreciate all kinds of art styles, " +
                                   "look into the efforts on what the artist has created, not its quality, " +
                                   "Traced art will only be accepted if it has proper credit " +
                                   "to the original artist to avoid trouble " +
                                   "(failure to credit the original artist from the traced art " +
                                   "will lead to the artist banned from fanart feature)",
            [728617392791355472] = "Post Lyrica-related/Lyrica’s stream related memes here. " +
                                   "Other memes go to <#751118105480265789>",
            [728575696099737630] = "Share Lyrica-related Youtube and Twitch clips here.",
            [735528170085679205] = "In relation to #lyrica-lore, it is the place where " +
                                   "members can talk about/give ideas to update the lore. " +
                                   "Parodies are entertained but please avoid being rude/toxic from your stories.",
            [739898192857923586] = "Post appreciative essays for Lyrica. " +
                                   "Refrain from posting memes. " +
                                   "This is also an appreciation channel, not a chat channel.",

            // Server History
            [728521445369708576] = "Starred moments in Lyrica’s SiniGANG Discord server. " +
                                   "(it needs 8 star reacts for that specific message for it to be logged at <#728521445369708576>)",
            [733311526424805467] = "History of the server which keeps track of what happened per day. ",

            // Topic Channels
            [730036135249838141] = "Channel where they can discuss gaming and tech stuff. " +
                                   "(Including gaming links, PC/Laptop specs, etc.)",
            [731519139167993916] = "A Channel to discuss anything Anime / Manga related content. " +
                                   "Be warned that potential Spoilers may be found here. " +
                                   "(Use of Spoiler Tag is recommended)",
            [751118105480265789] = "Found a funny picture/meme? You can post it here! " +
                                   "Just make sure it’s doesn’t break any rules " +
                                   "(In regarding of funny memes/pictures, " +
                                   "cursed things are fine as long as they do not condone/approve serious things " +
                                   "like life problems and suicide and inappropriate images)",
            [731217745211031633] = "Game with a 0.0001% or 1/10000 of a kiss. " +
                                   "Do `bless me, lola`, `bless me!`, `lola bless`, '<:LolaBless:731455738152747108>' " +
                                   "to try your chances! " +
                                   "(Those who won this gacha will earn the Kissed by Lola Role for reward!)",
            [728519103454642248] = "Channel to do bot commands, and a free place to play with the bots in the server.",
            [755125987485810859] = "Post horror stories in this channel. Part of Spooktober.",

            // Other Communities
            [729566704073441311] = "Channel to share your creative works like art, music, literature, editing, etc.",
            [728632419724427377] = "Channel to post shameless plugs/feature yourself or what you do to the public (Ex: Youtube, Commissions, etc)",
            [747277962419896363] = "Channel to talk about other vtubers and related topics about vtubers.",
            [728468134431948861] = "Channel to notify streams from other fellow vtubers.",

            // Voice Channels
            [731918978510880829] = "Main chat channel for those in vc. " +
                                   "Please only direct your topic on vc here not in general/fil-chat " +
                                   "to avoid cluttering of topics.",
            [752163296307707974] = "The place where you can talk about the song being played on the music vcs.",
            [728615656563671117] = "Channel for Groovy and Rythm bot commands. " +
                                   "For adding/removing songs played in Groovy and Rhythm VCs  " +
                                   "(Remember to disconnect them before leaving)"
        };

        public static Dictionary<ulong, string> RoleDescriptions { get; } = new Dictionary<ulong, string>
        {
            [728460901740707861] = "Lyrica's Role. As the owner of the server (Shannon Included). A" +
                                   "lso the name of their unit.",
            [728502621987667988] = "Wightwulf's role.  He’s the person closest to contact " +
                                   "about important things when it comes to her channels/content " +
                                   "(For Ex: Collabs, Scheduling, etc)",
            [759707412692992080] = string.Empty, // Manage Mod
            [762289957501927457] = string.Empty, // Lyrica Ch.
            [743442874779172937] = string.Empty, // ModMail
            [737760893235888199] = "The people behind overseeing everything that happens in the server and other platforms. " +
                                   "They are 2nd in the chain of command, when it comes to important regards, " +
                                   "feel free to message them if you have something serious to talk to about the server.",

            [748848865897349131] = string.Empty, // — Bots —
            [728600674362982451] = string.Empty, // blargbot
            [730026759843741746] = string.Empty, // Lola Bot
            [728516815726182421] = string.Empty, // Dyno
            [728516936899624971] = string.Empty, // carl-bot
            [728517204903198730] = string.Empty, // Tatsu
            [728881195298848818] = string.Empty, // Nadeko
            [729377067308941393] = string.Empty, // Rythm
            [729378331065319474] = string.Empty, // Groovy
            [748780146168954922] = string.Empty, // WulfBot

            [748849059812606062] = string.Empty, // — Mods —
            [728464070524534854] = "The people who help out implementing and enforcing out the rules, " +
                                   "as well as help regulate both the peace and chaos of the server.",
            [758353453687898174] = string.Empty, // Mods with Ahegao Jacket
            [729202576968515634] = "People who assist the Main Mods and the Mods in their duties. " +
                                   "Mainly moderators that are Discord only.",
            [747170160955359334] = string.Empty, // 1st Gen Mod
            [747170440077639700] = string.Empty, // 2nd Gen Mod
            [747170445413056645] = string.Empty, // 3rd Gen Mod
            [747170445643743382] = string.Empty, // 4th Gen Mod
            [728460369693245472] = string.Empty, // Twitch Mods
            [728463567887532083] = string.Empty, // YouTube Mods

            [748845428413431858] = string.Empty, // — Groupings —
            [729872365353435196] = "ULTIMATE SIMPS/Recognized Donators by Lyrica/End game. " +
                                   "This is rewarded to people who have donate at least 100USD to gain instantly, " +
                                   "or given by Lyrica herself (Honorary Title of Knighthood granted by Lyrica)",
            [728462786736422935] = "Automated Role. Tier 3 Subscribers on Twitch " +
                                   "(Your twitch must be linked on Discord " +
                                   "so that you can automatically get the role from the bot)",
            [730075825143021678] = "Reward Role. Won 5 tetris games on tetris streams.",
            [737917005369442404] = "Moderators who have now stepped down from their role.",
            [731807968940523560] = "People who are able to receive the super rare kiss from lola in the Blessing Game. " +
                                   "(Sometimes Lyrica gives away the role)" + Environment.NewLine +
                                   "(This is basically gacha channel)" + Environment.NewLine +
                                   "(Best Gacha channel)",
            [728610551378477137] = "Automated Role. People who boosted the server using nitro boost.",
            [728462786736422932] = "Automated Role. Tier 1 Subscribers on Twitch " +
                                   "(Similar to Bingo Winners. Your twitch account should be linked to discord " +
                                   "so the bot can give you the role)",
            [745594372644536480] = "It is a role rewarded to the major builders of " +
                                   "monumental landmarks in the Minecraft Server",
            [729248749968293920] = "A reward given to those who helped giving out ideas in the server, " +
                                   "contributed emotes, and aided the server generally.",
            [755409349039489033] = "A reward given to the people who helped out Poffi " +
                                   "with listing campers before the new camping system.",
            [741534040883986464] = "Liliana Vampaia’s role.",
            [728676190952620133] = "The Halohalolive and Kalbolive group of Vtubers. " +
                                   "Includes Kaheru, Arisa, Spica, " +
                                   "YanchaGogo, Yuma Yamano, and Himeko Sae.",
            [728679916169592923] = "The Hanamori group of Artists and Vtubers. " +
                                   "Includes Ryuusei Nova, Asagiri Yua, Ichika Maia, " +
                                   "Asahina Chie, Xiaochao, Akatora Chimon, and Seigi.",
            [729873227639160882] = "For people who made fanart of Lyrica and have been featured on stream. " +
                                   "(Formerly called Resident Drawers, the name Orocan is from a brand of a drawer)",
            [748851133308600333] = "People who are in charge of managing the <#733311526424805467> channel. " +
                                   "They are the chroniclers of the history of the server. ",
            [740511093968863232] = "Hosts of the Unscuffed Podcast back in July. " +
                                   "People who recapped on the contents of the <#733311526424805467> channel back in July",
            [728468630387294269] = "Fellow VTubers.  People who use a Virtual character to do YouTube/Twitch",
            [753414363569717348] = "Spuds.. Nagi-related role. (For context: Nagi is lola’s potato)",

            [748849348170874972] = string.Empty, // — Default Roles —
            [728467669409464341] = string.Empty, // Bots
            [728857949564436551] = "People who are muted through warnings or manual commands. They are temporarily not able to chat/connect to vcs.",
            [728609118486528101] = "This is the role given to the people who accepted the rules. Name of the fans of Lyrica.",
            [728903243035443251] = "Normal. This is the default amount.",
            [728903246294548532] = "5 minutes mute.",
            [728903249129635902] = "12 hours mute.",
            [728903250916409425] = "Kick + 24 hours mute when rejoining.",
            [728903408790011914] = "Ban",

            [748849597341761568] = string.Empty, // — Special Roles —
            [762228685905854475] = "Role awarded to the current #1 person on the leaderboard in Tatsumaki.",
            [741288494831239259] = "Members who participated in the 'Noli Me Tangere' reading stream. (Commemorative Role)",
            [749864996963483746] = "Members who joined the Heneral Luna Movie Night Last August 31st (Commemorative Role)",
            [728668857375522876] = string.Empty, // StreamVC Unlocked
            [728462786736422933] = string.Empty, // Twitch Subscriber: Tier 1
            [728462786736422934] = string.Empty, // Twitch Subscriber: Tier 2
            [738736886532800522] = string.Empty, // Movie Banned
            [761437714863882250] = string.Empty, // Streaming Banned

            [748849488579264512] = string.Empty, // — Self Assign —
            [729608027883438091] = "A role for everyone that gets pinged when the stream schedule is available, " +
                                   "or a stream is about to start. (Can be opted out of.)",
            [760710940727050270] = string.Empty, // Shannon Stream Pings
            [760711148391366656] = "Pings made when a new stream schedule is released.",

            [755409354177642536] = string.Empty, // — Auto Roles —
            [759257307648884746] = "Is given to the current singer in Karaoke.",
            [755409355968348232] = string.Empty, // VC Gen Access
            [759938688309395497] = string.Empty, // VC Karaoke Access
            [759938896929882113] = string.Empty, // VC Games Access
            [728659065542148126] = "This is the role that allows you to post media/files/links to chat channels. " +
                                   "To unlock this role, you need 10K Tatsu points " +
                                   "to be able to post images and GIFs and links " +
                                   "by simply chatting/being active on the server.",
            [761437576283422721] = "Given at 50k tatsu points. " +
                                   "Allows you to screenshare into the server.",
            [728459950468104284] = "A role that everyone has. Self Explanatory." // @everyone
        };

        [Command("rule")]
        [Summary("Views that specific rule")]
        [Alias("rules")]
        public Task ViewRuleAsync([Summary("A number corresponding to the rule")]
            uint ruleNo)
        {
            var rules = MainRulesNumbers.Concat(VoiceRulesNumbers)
                .OrderBy(c => c)
                .ToArray();

            if (!rules.Contains(ruleNo))
                return ReplyAsync($"There are only {rules.First()}-{rules.Last()} rules!");

            if (MainRulesNumbers.Contains(ruleNo))
            {
                var (category, (_, title, description)) = MainRules
                    .SelectMany(c => c.rules,
                        (c, rule) =>
                            (c.category, rule))
                    .Single(r => r.rule.ruleNo == ruleNo);

                return ReplyAsync(embed: GetRuleEmbed(ruleNo, category, title, description).Build());
            }

            if (VoiceRulesNumbers.Contains(ruleNo))
            {
                var (_, title, description) = VoiceRules.Single(r => r.ruleNo == ruleNo);
                return ReplyAsync(embed: GetRuleEmbed(ruleNo, "Voice Rules & Regulations", title, description).Build());
            }

            return Task.CompletedTask;
        }

        private static EmbedBuilder GetRuleEmbed(uint ruleNo, string category, string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(247, 209, 211)
                .WithAuthor(category, _serverAvatarUrl)
                .WithTitle($"#{ruleNo} 【 {title} 】")
                .WithDescription(description);
            return embed;
        }

        [RequireOwner]
        [Command("update part")]
        public async Task UpdatePartAsync(RulePart rulePart, ITextChannel? channel = null, ulong? messageId = null, string? content = null)
        {
            channel ??= (ITextChannel) Context.Channel;
            var embed = rulePart switch
            {
                RulePart.Introduction => GetIntroduction(),
                RulePart.Main         => GetMainRules(),
                RulePart.Voice        => GetVoiceRules(),
                RulePart.Stream       => GetStreamRules(),
                RulePart.React        => GetReactEmbed(out _),
                RulePart.ModMail      => GetModMailEmbed(),
                RulePart.Warning      => GetWarningSystemEmbed(),
                RulePart.Channel      => GetChannelInfo(),
                RulePart.Role         => GetRoleInfo(),
                RulePart.Social       => GetSocialInfo(),
                _                     => throw new ArgumentOutOfRangeException(nameof(rulePart), rulePart, null)
            };

            if (messageId is null)
            {
                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                var message = (IUserMessage) await channel.GetMessageAsync((ulong) messageId);
                await message.ModifyAsync(m =>
                {
                    m.Embed = embed.Build();
                    m.Content = content ?? m.Content;
                });
            }
        }

        [RequireOwner]
        [Command("post rules")]
        public async Task PostRulesAsync(ITextChannel? channel = null)
        {
            channel ??= (ITextChannel) Context.Channel;

            await channel.SendMessageAsync(embed: GetIntroduction().Build());

            await channel.SendMessageAsync(embed: GetMainRules().Build());
            await channel.SendMessageAsync(embed: GetVoiceRules().Build());
            await channel.SendMessageAsync(embed: GetStreamRules().Build());

            var reactRoleMessage = await channel.SendMessageAsync(embed: GetReactEmbed(out var flower).Build());
            await reactRoleMessage.AddReactionAsync(flower);

            await channel.SendMessageAsync(embed: GetModMailEmbed().Build());
            await channel.SendMessageAsync(embed: GetWarningSystemEmbed().Build());
        }

        private EmbedBuilder GetIntroduction()
        {
            var introduction = new EmbedBuilder()
                .WithColor(_pink)
                .WithTitle($"Welcome to {Context.Guild.Name}")
                .WithDescription(
                    "We hope you enjoy your stay in here!" +
                    Environment.NewLine + Environment.NewLine +
                    "**This server is a hub for Lola's community, please enjoy your stay!**" +
                    Environment.NewLine + Environment.NewLine +
                    "Before you can enter, please read the rules carefully. Afterwards, you may accept the rules by reacting <:LyricaFlower:731456070601408612> at the bottom.")
                .WithThumbnailUrl(_serverAvatarUrl)
                .WithImageUrl("https://chito.ge/2L3DTUk.png?_raw=true");
            return introduction;
        }

        private EmbedBuilder GetMainRules()
        {
            // Rules
            var embed = new EmbedBuilder()
                .WithColor(_pink)
                .WithTitle("Server Rules & Regulations")
                .WithDescription(
                    "These are the general guidelines of the server. Take note that they are not meant to be skirted around. " +
                    "Mods can still warn you based on their discretion. They are also not made to be strict; they are in place to make sure that everyone feels welcome and has fun in the server.")
                .WithFooter("※ These rules were last updated")
                .WithImageUrl("https://chito.ge/3eXuKaf.png?_raw=true")
                .WithCurrentTimestamp();

            foreach (var (category, rules) in MainRules)
            {
                embed.AddField("\x200b", $"```diff\n- ୨୧ {category} ୨୧ -```");
                foreach (var (ruleNo, title, description) in rules)
                {
                    embed.AddField($"#{ruleNo} 【 {title} 】", description);
                }
            }

            return embed;
        }

        private EmbedBuilder GetVoiceRules()
        {
            var voice = new EmbedBuilder()
                .WithColor(_pink)
                .WithTitle("Voice Channels' Rules & Regulations")
                .WithDescription(
                    "Here are the rules and regulations for voice channels. " +
                    "Read carefully. The rules in voice channels are more laid-back, " +
                    "and less stricter than text-channel rules.")
                .WithFooter(
                    "※ There may be some situations not covered by these rules " +
                    "or times where the rule may not fit the situation. " +
                    "If this happens the moderators are trusted to handle the situation appropriately.");

            foreach (var (ruleNo, title, description) in VoiceRules)
            {
                voice.AddField($"#{ruleNo} 【 {title} 】", description);
            }

            return voice;
        }

        private EmbedBuilder GetStreamRules()
        {
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
                .WithColor(100, 65, 165)
                .WithTitle("Stream Rules & Regulations")
                .AddField("If you want me to understand you, use the language stated in the title!",
                    string.Join(Environment.NewLine, chatEnglish))
                .AddField("Kung nais mong maintindihan kita, gamitin mo ang wika na nakasaad sa pamagat!",
                    string.Join(Environment.NewLine, chatFilipino))
                .AddField("私の配信をご覧いただく際は以下の事を守ってください。",
                    string.Join(Environment.NewLine, chatJapanese))
                .WithFooter("※ These rules were last updated")
                .WithCurrentTimestamp();
            return stream;
        }

        private EmbedBuilder GetReactEmbed(out Emote flower)
        {
            flower = Emote.Parse("<:LyricaFlower:731456070601408612>");
            var reactRole = new EmbedBuilder()
                .WithColor(135, 203, 250)
                .WithTitle("I have read the rules")
                .WithThumbnailUrl("https://cdn.discordapp.com/emojis/731456070601408612.png?v=1")
                .WithImageUrl(
                    $"https://cdn.discordapp.com/banners/{Context.Guild.Id}/{Context.Guild.BannerId}.png?size=4096")
                .WithDescription(
                    "After reading the rules, please react here to be able to join <:LyricaApproves:731399021326893148>\r\n" +
                    "If this does not work, please contact the <@!575252669443211264> as seen below.");
            return reactRole;
        }

        private EmbedBuilder GetModMailEmbed()
        {
            var modMail = new EmbedBuilder()
                .WithColor(65, 105, 238)
                .WithTitle("Moderation Contacts and Inquiries")
                .WithDescription(
                    "If you have any questions or want to contact Lyrica, you must contact <@!575252669443211264> to relay your message to us. You can **directly message the bot** and **get a ticket** and we will reply to your message." +
                    Environment.NewLine + Environment.NewLine +
                    "As for anything more important, contact the head mods <@!189307395632005121> or <@!246903976937783296>--but it's advisable to use mod mail instead.")
                .WithThumbnailUrl(
                    "https://cdn.discordapp.com/avatars/575252669443211264/de604039e893f9ce7fa6870b7d3193c7.png?size=1024")
                .WithFooter("※ You can also contact an individual moderator if it is only involving them.");
            return modMail;
        }

        private EmbedBuilder GetWarningSystemEmbed()
        {
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
            return warning;
        }

        [RequireOwner]
        [Command("post information")]
        public async Task PostInformationAsync(ITextChannel? channel = null)
        {
            channel ??= (ITextChannel) Context.Channel;

            await channel.SendMessageAsync(embed: GetChannelInfo().Build());
            await channel.SendMessageAsync(embed: GetRoleInfo().Build());
            await channel.SendMessageAsync("**Permanent Invite**: https://discord.gg/lyrica", embed: GetSocialInfo().Build());
        }

        private EmbedBuilder GetChannelInfo()
        {
            var categories = new ulong[]
            {
                728471729332158554,
                728617721318604830,
                728459951068151819,
                760712884933230612,
                735914773308244069,
                735555465580249189,
                761833372682027008,
                728459951068151820
            };
            var channels = new EmbedBuilder()
                .WithTitle("Channel Information")
                .WithColor(0x87CBFA)
                .WithDescription("Here are the summary of the channels and what they are for.")
                .WithImageUrl("https://chito.ge/96t1PmC.png?_raw=true")
                .WithFooter("※ Last Updated")
                .WithCurrentTimestamp();

            foreach (var category in categories)
            {
                var socketCategory = Context.Guild.GetCategoryChannel(category);
                AddCategoryInformation(channels, socketCategory);
            }

            return channels;
        }

        private EmbedBuilder AddCategoryInformation(EmbedBuilder builder, SocketCategoryChannel category, params IChannel[] hiddenChannels)
        {
            var channels = category.Channels
                .Except(hiddenChannels)
                .Where(c => ChannelDescriptions.ContainsKey(c.Id))
                .ToArray();

            if (!channels.Any())
                return builder;
            builder.AddLinesIntoFields($"【 {category.Name.Transform(To.LowerCase, To.TitleCase)} 】", channels, c => $"◈ **<#{c.Id}>** {ChannelDescriptions[c.Id]}");

            return builder;
        }

        private EmbedBuilder GetRoleInfo()
        {
            var roles = new EmbedBuilder()
                .WithTitle("Role Information")
                .WithColor(0x87CBFA)
                .WithImageUrl("https://chito.ge/3fMPbTh.png?_raw=true")
                .WithFooter("※ Last Updated")
                .WithCurrentTimestamp();

            var roleDescriptions = RoleDescriptions
                .Select((r, i) => new { Role = Context.Guild.GetRole(r.Key), Description = r.Value, Index = i })
                .Where(r => !string.IsNullOrWhiteSpace(r.Description))
                .Where(r => !_roleNameRegex.IsMatch(r.Role.Name))
                .GroupBy(r =>
                    RoleDescriptions
                        .Take(r.Index)
                        .Select(role => Context.Guild.GetRole(role.Key).Name)
                        .LastOrDefault(role => _roleNameRegex.IsMatch(role)))
                .ToArray();

            foreach (var category in roleDescriptions)
            {
                roles.AddLinesIntoFields(category.Key is null ? "\x200b" : $"【 {category.Key} 】", category, r => $"◈ **{r.Role.Mention}** {r.Description}");
            }

            return roles;
        }

        private EmbedBuilder GetSocialInfo()
        {
            var info = new EmbedBuilder()
                .WithTitle("💮 【 Socials 】 💮")
                .WithColor(_pink)
                .WithImageUrl("https://chito.ge/5tJ9XvP.png?_raw=true")
                .WithDescription(
                    "◈ YouTube: **[Lyrica Ch.](https://youtube.com/c/LyricaCh)**" + Environment.NewLine +
                    "◈ Twitch: **[lyrica_ch](https://twitch.tv/lyrica_ch)**" + Environment.NewLine +
                    "◈ Discord: **[discord.gg/lyrica](https://discord.gg/lyrica)**" + Environment.NewLine +
                    "◈ Twitter: **[@Lyrica_ch](https://twitter.com/Lyrica_ch)**" + Environment.NewLine +
                    "◈ Marshmallow: **[lyrica_ch](https://marshmallow-qa.com/lyrica_ch)**")
                .AddField("🌟 【 Tags/Names 】 🎨",
                    "◈ Fans: **SiniGANG**" + Environment.NewLine +
                    "◈ Art: **[#LyriGanda](https://twitter.com/search?q=LyriGanda)**");

            return info;
        }
    }
}
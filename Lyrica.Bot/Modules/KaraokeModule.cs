using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lyrica.Data;
using Lyrica.Data.Karaoke;
using Lyrica.Services.Core.Messages;
using Lyrica.Services.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lyrica.Bot.Modules
{
    [Group("karaoke")]
    [Alias("k")]
    public class KaraokeModule :
        ModuleBase<SocketCommandContext>,
        INotificationHandler<UserVoiceStateNotification>
    {
        private static readonly Dictionary<IGuild, CancellationTokenSource> IntermissionTokens =
            new Dictionary<IGuild, CancellationTokenSource>();

        private static readonly Dictionary<IUser, string?> Nicknames = new Dictionary<IUser, string?>();

        private readonly CommandService _commands;
        private readonly ILogger<KaraokeModule> _log;
        private readonly IServiceProvider _services;
        private readonly LyricaContext _db;

        public KaraokeModule(
            ILogger<KaraokeModule> log,
            IServiceProvider services,
            CommandService commands,
            LyricaContext db)
        {
            _log = log;
            _services = services;
            _commands = commands;
            _db = db;
        }

        private async Task<KaraokeSetting> GetKaraokeAsync(ulong guildId)
        {
            var guild = await _db.Guilds
                .Include(g => g.Karaoke)
                .ThenInclude(k => k.Queue)
                .ThenInclude(e => e.User)
                .FirstAsync(g => g.Id == guildId);

            return guild.Karaoke!;
        }

        public async Task Handle(
            UserVoiceStateNotification notification,
            CancellationToken cancellationToken)
        {
            if (notification.New.VoiceChannel is null && notification.Old?.VoiceChannel is null)
                return;

            var karaoke = await GetKaraokeAsync((ulong) (notification.New.VoiceChannel?.Guild.Id ?? notification.Old?.VoiceChannel.Guild.Id)!);
            var user = (SocketGuildUser) notification.User;
            if (notification.Old?.VoiceChannel?.Id == karaoke.KaraokeVc &&
                notification.New.VoiceChannel?.Id != karaoke.KaraokeVc)
            {
                if (user.IsMuted)
                    await user.ModifyAsync(u => u.Mute = false);
                return;
            }

            if (notification.New.VoiceChannel?.Id != karaoke.KaraokeVc) return;

            if (karaoke.CurrentSinger?.User.Id == user.Id)
            {
                if (!user.HasRole(karaoke.SingingRole))
                {
                    _log.LogWarning("Current singer {0} had no Singing Role. It was given to {1}",
                        karaoke.CurrentSinger?.User,
                        notification.User);
                    var guild = notification.New.VoiceChannel.Guild;
                    var role = guild.GetRole(karaoke.SingingRole);
                    await user.AddRoleAsync(role);
                }

                if (user.IsMuted)
                {
                    _log.LogWarning("Current singer {0} was muted. Unmuting {1}",
                        karaoke.CurrentSinger?.User,
                        notification.User);
                    await user.ModifyAsync(u => u.Mute = false);
                }
            }

            if (!karaoke.Intermission && notification.User.Id != karaoke.CurrentSinger?.User.Id &&
                !notification.New.IsSelfMuted && !notification.New.IsMuted)
            {
                _log.LogDebug("Current singer is {0} so {1} was muted",
                    karaoke.CurrentSinger?.User,
                    notification.User);
                await user.ModifyAsync(u => u.Mute = true);
            }
            else if (notification.New.IsSelfMuted && notification.New.IsMuted)
            {
                await user.ModifyAsync(u => u.Mute = false);
            }
        }

        [Command(null, true)]
        [Alias("help")]
        [Priority(-1)]
        [Summary("Views the karaoke help command")]
        public Task ViewHelpAsync() => _commands.ExecuteAsync(Context, "help module karaoke", _services);

        [Command("queue", true)]
        [Alias("list")]
        [Summary("View the Karaoke Queue")]
        public Task ViewQueueAsync() => UpdateOrSendQueue(mention: false);

        [Command("reset", true)]
        [Summary("Resets the Karaoke Queue")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task ResetQueueAsync()
        {
            var karaoke = await GetKaraokeAsync(Context.Guild.Id);

            _db.RemoveRange(karaoke.Queue);
            await _db.SaveChangesAsync();

            karaoke.Queue.Clear();

            await PauseKaraokeAsync();
            await ReplyAsync("Queue has been reset!");
        }

        [Command("refresh", true)]
        [Summary("Refreshes the Queue and Permissions")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task RefreshQueueAsync()
        {
            var karaoke = await GetKaraokeAsync(Context.Guild.Id);
            await ReplyAsync("Refreshing Karaoke permissions");
            await PauseKaraokeAsync();
            await StartKaraokeAsync();
            _log.LogDebug("Queue {0}", karaoke.Queue);
            _log.LogDebug("Current Singer {0}", karaoke.CurrentSinger?.User);
            await ReplyAsync("Queue and permissions has been reset!");
            await UpdateOrSendQueue();
        }

        [Command("skip", true)]
        [Summary("Force Skip the Current Singer")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public Task SkipUserAsync() => ShowNextUserAsync();

        [Command("next", true)]
        [Alias("finish", "finished", "done")]
        [Summary("Goes to the next user in the queue when you finished singing")]
        public async Task NextUserAsync()
        {
            var karaoke = await GetKaraokeAsync(Context.Guild.Id);

            if (Context.User.Id == karaoke.CurrentSinger?.User.Id)
                await ShowNextUserAsync();
            else
                await ReplyAsync("You are not the current singer!");
        }

        [Command("voteskip", true)]
        [Summary("Vote skips the current singer")]
        public async Task VoteSkipUserAsync()
        {
            var user = await _db.Users.FindAsync(Context.User.Id);
            var guild = await _db.Guilds
                .Include(g => g.Karaoke)
                .ThenInclude(k => k.VoteSkippedUsers)
                .FirstOrDefaultAsync(g => g.Id == Context.Guild.Id);

            if (guild?.Karaoke is null || user is null)
                return;

            var karaoke = guild.Karaoke;
            if (karaoke.VoteSkippedUsers.Contains(user))
            {
                await ReplyAsync("You already vote skipped!");
                return;
            }

            if (!Context.Guild.GetVoiceChannel(karaoke.KaraokeVc).Users.Contains(Context.User))
            {
                await ReplyAsync("You're not in the Karaoke VC");
                return;
            }

            karaoke.VoteSkippedUsers.Add(user);
            await _db.SaveChangesAsync();

            var channel = Context.Guild.GetVoiceChannel(karaoke.KaraokeVc);
            var threshold = channel.Users.Count / 2;
            if (karaoke.VoteSkippedUsers.Count < threshold)
            {
                await ReplyAsync($"{Context.User} voted to skip! ({karaoke.VoteSkippedUsers.Count}/{threshold})");
            }
            else
            {
                karaoke.VoteSkippedUsers.Clear();
                await _db.SaveChangesAsync();
                await ShowNextUserAsync();
            }
        }

        [Command("pause", true)]
        [Summary("Pauses the Karaoke")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task PauseKaraokeAsync(bool announce = true)
        {
            var token = ResetToken();
            var karaoke = await GetKaraokeAsync(Context.Guild.Id);
            karaoke.Intermission = true;
            await _db.SaveChangesAsync(token);

            var channel = Context.Guild.GetVoiceChannel(karaoke.KaraokeVc);
            await channel
                .AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                    new OverwritePermissions(useVoiceActivation: PermValue.Inherit));

            await channel.ModifyAsync(c => c.Bitrate = 8000);

            foreach (var user in channel.Users
                .Where(u => u.IsMuted))
            {
                await user.ModifyAsync(u => u.Mute = false);
                await HoistUserAsync(user, false);
            }

            if (announce)
                await ReplyAsync("The Karaoke has been paused!");
        }

        [Command("start", true)]
        [Summary("Starts the Karaoke")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task StartKaraokeAsync()
        {
            var token = ResetToken();
            var karaoke = await GetKaraokeAsync(Context.Guild.Id);
            if (await QueueIsEmptyAsync(karaoke)) return;

            var channel = Context.Guild.GetVoiceChannel(karaoke.KaraokeVc);
            var role = Context.Guild.GetRole(karaoke.SingingRole);

            var currentSinger = Context.Guild.GetUser(karaoke.CurrentSinger!.User.Id);
            await currentSinger.AddRoleAsync(role);
            await currentSinger.ModifyAsync(u => u.Mute = false);

            foreach (var user in role.Members
                .Where(m => m != currentSinger))
            {
                await user.RemoveRoleAsync(role);
            }

            foreach (var user in channel.Users.Where(u =>
                u != currentSinger && !u.IsSelfMuted))
            {
                await user.ModifyAsync(u => u.Mute = true);
            }

            foreach (var user in channel.Users.Where(u =>
                u.IsMuted && u.IsSelfMuted))
            {
                await user.ModifyAsync(u => u.Mute = false);
            }

            await channel.ModifyAsync(c => c.Bitrate = 64000);

            karaoke.Intermission = false;
            await _db.SaveChangesAsync(token);

            await UpdateOrSendQueue(mention: true);
        }

        [Command("add")]
        [Summary("Add yourself to the Queue")]
        public async Task AddUserAsync([Remainder] [Summary("The title of the song you will sing")]
            string? song = null)
        {
            var karaoke = await GetKaraokeAsync(Context.Guild.Id);
            if (Context.User.Id == karaoke.CurrentSinger?.User.Id)
            {
                if (karaoke.Intermission) return;

                await ShowNextUserAsync();
            }

            if (karaoke.HasUser(Context.User))
            {
                await ReplyAsync($"{Context.User}, you are already in the queue!");
                return;
            }

            var user = await _db.Users.FindAsync(Context.User.Id);
            karaoke.Add(user, song);

            await _db.SaveChangesAsync();
            await ReplyAsync($"{Context.User.Mention}, you have been added to the queue.");

            if (karaoke.Queue.Count == 1)
            {
                var token = ResetToken();
                await ReplyAsync("A new queue has started! Karaoke will begin in 30 seconds!");
                await Task.Delay(TimeSpan.FromSeconds(30), token)
                    .ContinueWith(t => { });
                if (token.IsCancellationRequested)
                {
                    _log.LogInformation("The current singer cancelled their start of the queue.");
                    return;
                }

                await StartKaraokeAsync();
            }
        }

        [Priority(10)]
        [Command("force add")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task ForceAdd(IGuildUser user, [Remainder] string? song = null)
        {
            var karaoke = await GetKaraokeAsync(Context.Guild.Id);
            var dbUser = await _db.Users.FindAsync(Context.User.Id);
            karaoke.Queue.Add(new KaraokeEntry(dbUser, song));

            await _db.SaveChangesAsync();
        }

        [Priority(10)]
        [Command("force remove")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task ForceRemove(int position)
        {
            var karaoke = await GetKaraokeAsync(Context.Guild.Id);
            karaoke.Queue.RemoveAt(position);
            await _db.SaveChangesAsync();
        }

        [Command("remove", true)]
        [Summary("Remove yourself from the Queue")]
        public async Task RemoveUserAsync()
        {
            var karaoke = await GetKaraokeAsync(Context.Guild.Id);
            if (!karaoke.HasUser(Context.User))
            {
                await ReplyAsync("You are not in the queue!");
                return;
            }

            if (karaoke.CurrentSinger?.User.Id == Context.User.Id)
            {
                await ShowNextUserAsync();
                return;
            }

            if (karaoke.RemoveSinger(Context.User, out var entry))
            {
                _db.Remove(entry);
                await _db.SaveChangesAsync();
            }

            await ReplyAsync($"{Context.User.Mention} You were removed from the queue.");
        }

        private CancellationToken ResetToken()
        {
            IntermissionTokens.TryGetValue(Context.Guild, out var token);
            token?.Cancel();
            token = IntermissionTokens[Context.Guild] = new CancellationTokenSource();
            return token.Token;
        }

        private static string GetSong(KaraokeEntry entry) => $"**【 {entry.Song ?? "Secret"} 】**";

        private async Task ShowNextUserAsync()
        {
            var token = ResetToken();

            var karaoke = await GetKaraokeAsync(Context.Guild.Id);
            if (await QueueIsEmptyAsync(karaoke)) return;

            var last = karaoke.CurrentSinger!;
            var next = karaoke.NextUp.FirstOrDefault();

            _db.Remove(last);
            await _db.SaveChangesAsync(token);

            karaoke.RemoveSinger(last);
            karaoke.VoteSkippedUsers.Clear();

            if (next is not null)
            {
                var lastUser = Context.Guild.GetUser(last.User.Id);
                var nextUser = Context.Guild.GetUser(next.User.Id);

                await HoistUserAsync(lastUser, false);
                await PauseKaraokeAsync(false);

                var message = await ReplyAsync(
                    $"{lastUser} just finished singing {GetSong(last)}! Everyone can now speak for 30 seconds.\n" +
                    $"The next user to sing is {nextUser}~ {GetSong(next)}");
                await message.AddReactionAsync(new Emoji("👏"));

                token = IntermissionTokens[Context.Guild].Token;
                await Task.Delay(TimeSpan.FromSeconds(30), token)
                    .ContinueWith(t => { });
            }

            if (token.IsCancellationRequested)
                return;

            await StartKaraokeAsync();
            await UpdateOrSendQueue(last, announce: false);
        }

        private async Task HoistUserAsync(SocketGuildUser user, bool hoist = true)
        {
            if (Context.Guild.CurrentUser.Roles.All(r => user.Roles.All(u => r.Position < u.Position)))
            {
                _log.LogInformation("Cannot hoist this user {0} as all their roles are above or equal to mine", user);
                return;
            }

            var name = user.Nickname ?? user.Username;
            if (hoist)
            {
                if (name.StartsWith("!"))
                    return;

                Nicknames[user] = name;
                await user.ModifyAsync(u => u.Nickname = $"!{name}");
            }
            else
            {
                if (Nicknames.TryGetValue(user, out var nickname))
                    await user.ModifyAsync(u => u.Nickname = nickname);
                else if (name.StartsWith("!")) await user.ModifyAsync(u => u.Nickname = Regex.Replace(name, @"^!*", string.Empty));
            }
        }

        private async Task UpdateOrSendQueue(KaraokeEntry? lastSinger = null, bool mention = true, bool announce = true)
        {
            var karaoke = await GetKaraokeAsync(Context.Guild.Id);
            if (await QueueIsEmptyAsync(karaoke, announce)) return;

            IMessage? message;
            if (karaoke.KaraokeMessage is not null)
            {
                var channel = Context.Guild.GetTextChannel(karaoke.KaraokeChannel);
                message = await channel.GetMessageAsync((ulong) karaoke.KaraokeMessage);
                if (message != null)
                    _ = message.DeleteAsync();
            }

            var embed = new EmbedBuilder()
                .WithTitle("Karaoke Queue")
                .WithUserAsAuthor(Context.User)
                .AddField("Currently Singing",
                    $"{GetSong(karaoke.CurrentSinger!)} sang by <@!{karaoke.CurrentSinger!.User.Id}>");

            if (karaoke.NextUp.Any())
                embed.AddLinesIntoFields("Next Up", karaoke.NextUp,
                    (u, i) => $"{i + 1}. {GetSong(u)} sang by <@!{u.User.Id}>");

            if (lastSinger is not null)
                embed.WithDescription(
                    $"The last song was {GetSong(lastSinger)} by <@!{lastSinger.User.Id}>");

            var user = Context.Client.GetUser(karaoke.CurrentSinger.User.Id);
            message = await ReplyAsync($"It is now {(mention ? user.Mention : user.ToString())}'s turn to sing! They're singing {GetSong(karaoke.CurrentSinger)}!", embed: embed.Build());
            karaoke.KaraokeMessage = message.Id;
            await _db.SaveChangesAsync();
        }

        private async Task<bool> QueueIsEmptyAsync(KaraokeSetting karaoke, bool announce = true)
        {
            if (karaoke.CurrentSinger is not null) return false;
            await PauseKaraokeAsync(false);
            if (announce)
                await ReplyAsync("Queue is empty! Add yourself by doing `l!k add [song]`");
            return true;
        }
    }
}
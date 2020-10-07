using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Lyrica.Data.Karaoke;
using Lyrica.Services.Core.Messages;
using Lyrica.Services.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lyrica.Bot.Modules
{
    [Group("karaoke")]
    [Alias("k")]
    public class KaraokeModule :
        ModuleBase<SocketCommandContext>,
        INotificationHandler<UserVoiceStateNotification>
    {
        private const ulong KaraokeVc = 746265675797889025;
        private const ulong SingingRole = 759257307648884746;

        private static readonly List<IUser> VoteSkippedUsers = new List<IUser>();
        private static readonly KaraokeQueue Queue = new KaraokeQueue();
        private static IUserMessage? _queueMessage;
        private static bool _intermission = true;
        private static CancellationTokenSource _intermissionToken = new CancellationTokenSource();

        private readonly CommandService _commands;
        private readonly ILogger<KaraokeModule> _log;
        private readonly IServiceProvider _services;

        public KaraokeModule(
            IServiceProvider services,
            CommandService commands,
            ILogger<KaraokeModule> log)
        {
            _commands = commands;
            _services = services;
            _log = log;
        }

        public async Task Handle(
            UserVoiceStateNotification notification,
            CancellationToken cancellationToken)
        {
            var user = (IGuildUser) notification.User;
            if (notification.Old?.VoiceChannel?.Id == KaraokeVc &&
                notification.New.VoiceChannel?.Id != KaraokeVc)
            {
                if (user.IsMuted)
                    await user.ModifyAsync(u => u.Mute = false);
                return;
            }

            if (notification.New.VoiceChannel?.Id != KaraokeVc) return;

            if (Queue.CurrentSinger?.User.Id == user.Id)
            {
                if (!user.HasRole(SingingRole))
                {
                    _log.LogWarning("Current singer {0} had no Singing Role. It was given to {1}",
                        Queue.CurrentSinger?.User,
                        notification.User);
                    var guild = notification.New.VoiceChannel.Guild;
                    var role = guild.GetRole(SingingRole);
                    await user.AddRoleAsync(role);
                }

                if (user.IsMuted)
                {
                    _log.LogWarning("Current singer {0} was muted. Unmuting {1}",
                        Queue.CurrentSinger?.User,
                        notification.User);
                    await user.ModifyAsync(u => u.Mute = false);
                }
            }

            if (!notification.New.IsSelfMuted && !notification.New.IsMuted)
            {
                if (!_intermission && notification.User.Id != Queue.CurrentSinger?.User.Id)
                {
                    _log.LogDebug("Current singer is {0} so {1} was muted",
                        Queue.CurrentSinger?.User,
                        notification.User);
                    await user.ModifyAsync(u => u.Mute = true);
                }
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
            Queue.Queue.Clear();
            await UpdateVcRolesAsync();
            await ReplyAsync("Queue has been reset!");
        }

        [Command("refresh", true)]
        [Summary("Refreshes the Queue and Permissions")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task RefreshQueueAsync()
        {
            await ReplyAsync("Refreshing Karaoke permissions");
            await UpdateVcRolesAsync(true);
            await UpdateVcRolesAsync();
            _log.LogDebug("Queue {0}", Queue.Queue);
            _log.LogDebug("Current Singer {0}", Queue.CurrentSinger?.User);
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
        public Task NextUserAsync()
        {
            if (_intermission)
                return Task.CompletedTask;

            return Context.User.Id == Queue.CurrentSinger?.User.Id
                ? ShowNextUserAsync(Context.User)
                : ReplyAsync("You are not the current singer!");
        }

        [Command("voteskip", true)]
        [Summary("Vote skips the current singer")]
        public Task VoteSkipUserAsync()
        {
            if (VoteSkippedUsers.Contains(Context.User))
                return ReplyAsync("You already vote skipped!");

            if (!Context.Guild.GetVoiceChannel(KaraokeVc).Users.Contains(Context.User))
                return ReplyAsync("You're not in the Karaoke VC");

            VoteSkippedUsers.Add(Context.User);

            var channel = Context.Guild.GetVoiceChannel(KaraokeVc);
            var threshold = channel.Users.Count / 2;
            if (VoteSkippedUsers.Count < threshold)
                return ReplyAsync($"{Context.User} voted to skip! ({VoteSkippedUsers.Count}/{threshold})");
            VoteSkippedUsers.Clear();
            return ShowNextUserAsync();
        }

        private async Task ShowNextUserAsync(IUser? user = null)
        {
            var last = Queue.CurrentSinger;
            var next = Queue.NextUp.FirstOrDefault();
            if (last != null && next != null)
            {
                var message = await ReplyAsync(
                    $"{last.User} just finished singing {GetSong(last)}! Everyone can now speak for 30 seconds.\n" +
                    $"The next user to sing is {next.User}~ {GetSong(next)}");
                await message.AddReactionAsync(new Emoji("👏"));
                await UpdateVcRolesAsync(true);
                await Task.Delay(TimeSpan.FromSeconds(30));
            }

            Queue.NextSinger(user);
            VoteSkippedUsers.Clear();

            await UpdateVcRolesAsync();
            if (Queue.CurrentSinger is null)
            {
                await ReplyAsync("The queue is now empty! Everyone can now speak.");
                return;
            }

            await UpdateOrSendQueue(last);
        }

        [Command("add")]
        [Summary("Add yourself to the Queue")]
        public async Task AddUserAsync([Remainder] [Summary("The title of the song you will sing")]
            string? song = null)
        {
            if (Context.User.Id == Queue.CurrentSinger?.User.Id)
            {
                if (_intermission)
                    return;
                await ShowNextUserAsync(Context.User);
            }

            if (Queue.HasUser(Context.User))
            {
                await ReplyAsync($"{Context.User}, you are already in the queue!");
                return;
            }

            Queue.Add(Context.User, song);
            await ReplyAsync($"{Context.User.Mention}, you have been added to the queue.");

            if (Queue.Queue.Count == 1)
            {
                await ReplyAsync("A new queue has started! Karaoke will begin in 30 seconds!");
                await UpdateVcRolesAsync(true);
                await Task.Delay(TimeSpan.FromSeconds(30), _intermissionToken.Token);
                if (_intermissionToken.IsCancellationRequested)
                {
                    _log.LogInformation("The current singer cancelled their start of the queue.");
                    _intermissionToken = new CancellationTokenSource();
                }

                await ReplyAsync(
                    $"{Queue.CurrentSinger!.User.Mention}, it's now your turn to sing! {GetSong(Queue.CurrentSinger)}");
                await UpdateVcRolesAsync();
            }
        }

        [Priority(10)]
        [Command("force add")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task ForceAdd(IGuildUser user, int? position = null, [Remainder] string? song = null)
        {
            var entry = new KaraokeEntry(user, song);
            Queue.Queue.Insert(position ?? 0, entry);
        }

        [Command("remove", true)]
        [Summary("Remove yourself from the Queue")]
        public Task RemoveUserAsync()
        {
            if (!Queue.HasUser(Context.User))
                return ReplyAsync("You are not in the queue!");

            if (Queue.CurrentSinger?.User.Id == Context.User.Id) return ShowNextUserAsync();

            Queue.Remove(Context.User);
            return ReplyAsync($"{Context.User.Mention} You were removed from the queue.");
        }

        private string GetSong(KaraokeEntry entry) => $"**【 {entry.Song ?? "Secret"} 】**";

        private async Task UpdateVcRolesAsync(bool? intermission = null)
        {
            var channel = Context.Guild.GetVoiceChannel(KaraokeVc);
            if (intermission == true || Queue.CurrentSinger is null)
            {
                _intermission = true;
                await channel
                    .AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                        new OverwritePermissions(useVoiceActivation: PermValue.Inherit));

                foreach (var user in channel.Users
                    .Where(u => u.IsMuted))
                {
                    await user.ModifyAsync(u => u.Mute = false);
                }
            }
            else
            {
                _intermission = false;
                var role = Context.Guild.GetRole(SingingRole);
                var currentSinger = Queue.CurrentSinger.User;

                if (!currentSinger.HasRole(role)) await currentSinger.AddRoleAsync(role);

                foreach (var user in role.Members
                    .Where(m => m != currentSinger))
                {
                    await user.RemoveRoleAsync(role);
                }

                foreach (var user in channel.Users
                    .Where(u => u != currentSinger && !u.IsSelfMuted && !u.IsMuted))
                {
                    await user.ModifyAsync(u => u.Mute = true);
                }

                await channel
                    .AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                        new OverwritePermissions(useVoiceActivation: PermValue.Deny));
            }
        }

        private async Task UpdateOrSendQueue(KaraokeEntry? lastSinger = null, bool mention = true)
        {
            if (_queueMessage != null) _ = _queueMessage.DeleteAsync();

            if (Queue.CurrentSinger is null)
            {
                await ReplyAsync("There is no current singer, add yourself by doing `l!karaoke add [song]`!");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Karaoke Queue")
                .WithUserAsAuthor(Context.User)
                .AddField("Currently Singing",
                    $"{GetSong(Queue.CurrentSinger)} sang by {Queue.CurrentSinger.User}");

            if (Queue.NextUp.Any())
                embed.AddField("Next Up",
                    string.Join(
                        Environment.NewLine,
                        Queue.NextUp.Select((u, i) =>
                            $"{i + 1}. {GetSong(u)} sang by {u.User}")));

            if (lastSinger != null)
                embed.WithDescription(
                    $"The last song was {GetSong(lastSinger)} by {lastSinger.User}");

            _queueMessage =
                await ReplyAsync(
                    $"It is now {(mention ? Queue.CurrentSinger.User.Mention : Queue.CurrentSinger.User.ToString())}'s turn to sing! They're singing {GetSong(Queue.CurrentSinger)}!",
                    embed: embed.Build());
        }
    }
}
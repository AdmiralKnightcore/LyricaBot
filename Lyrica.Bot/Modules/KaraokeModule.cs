using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Lyrica.Data.Karaoke;
using Lyrica.Services.Utilities;

namespace Lyrica.Bot.Modules
{
    [Group("karaoke")]
    public class KaraokeModule : ModuleBase<SocketCommandContext>
    {
        private const ulong KaraokeVc = 746265675797889025;
        private const ulong SingingRole = 759257307648884746;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        private static readonly List<IUser> _voteSkippedUsers = new List<IUser>();

        private static KaraokeQueue _queue = new KaraokeQueue();

        private static IUserMessage? _queueMessage;

        public KaraokeModule(
            IServiceProvider services,
            CommandService commands)
        {
            _commands = commands;
            _services = services;
        }

        [Command]
        [Summary("Views the karaoke help command")]
        public Task ViewHelpAsync() => _commands.ExecuteAsync(Context, "help module karaoke", _services);

        [Command("queue")]
        [Alias("list")]
        [Summary("View the Karaoke Queue")]
        public Task ViewQueueAsync() => UpdateOrSendQueue(mention: false);

        [Command("reset")]
        [Summary("Resets the Karaoke Queue")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task ResetQueueAsync()
        {
            KaraokeQueue.Queue.Clear();
            await UpdateVcRolesAsync();
            await ReplyAsync("Queue has been reset!");
        }

        [Command("skip")]
        [Summary("Force Skip the Current Singer")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public Task SkipUserAsync() => ShowNextUserAsync();

        [Command("next")]
        [Summary("Goes to the next user in the queue when you finished singing")]
        public Task NextUserAsync() => (IGuildUser) Context.User == _queue.CurrentSinger?.User
            ? ShowNextUserAsync()
            : ReplyAsync("You are not the current singer!");

        [Command("voteskip")]
        [Summary("Vote skips the current singer")]
        public Task VoteSkipUserAsync()
        {
            if (_voteSkippedUsers.Contains(Context.User))
                return ReplyAsync("You already vote skipped!");
            _voteSkippedUsers.Add(Context.User);

            var channel = Context.Guild.GetVoiceChannel(KaraokeVc);
            var threshold = channel.Users.Count / 2;
            if (_voteSkippedUsers.Count >= threshold)
            {
                _voteSkippedUsers.Clear();
                return ShowNextUserAsync();
            }

            return ReplyAsync($"{Context.User} voted to skip! ({_voteSkippedUsers.Count}/{threshold})");
        }

        private async Task ShowNextUserAsync()
        {
            var last = _queue.CurrentSinger;
            var next = _queue.NextUp.FirstOrDefault();
            if (last != null && next != null)
            {
                var message = await ReplyAsync(
                    $"{last.User} just finished singing 【{last.Song ?? "Secret"}】! Everyone can now speak for 30 seconds.\n" +
                    $"The next user to sing is {next.User}~");
                await message.AddReactionAsync(new Emoji("👏"));
                await UpdateVcRolesAsync(true);
                await Task.Delay(TimeSpan.FromSeconds(30));
            }

            _queue.NextSinger();
            await UpdateVcRolesAsync();

            if (_queue.CurrentSinger is null)
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
            if (_queue.HasUser(Context.User))
            {
                await ReplyAsync($"{Context.User}, you are already in the queue!");
                return;
            }

            _queue.Add(Context.User, song);
            await ReplyAsync($"{Context.User.Mention}, you have been added to the queue.");

            if (!KaraokeQueue.Queue.Any())
            {
                await ReplyAsync("A new queue has started! Karaoke will begin in 30 seconds!");
                await Task.Delay(TimeSpan.FromSeconds(30));
                await ShowNextUserAsync();
            }
        }

        [Command("remove")]
        [Summary("Remove yourself from the Queue")]
        public Task RemoveUserAsync()
        {
            if (_queue.HasUser(Context.User))
            {
                _queue.Remove(Context.User);
                return ReplyAsync($"{Context.User.Mention} You were removed from the queue.");
            }

            return ReplyAsync("You are not in the queue!");
        }

        private async Task UpdateVcRolesAsync(bool intermission = false)
        {
            var channel = Context.Guild.GetVoiceChannel(KaraokeVc);
            if (intermission || _queue.CurrentSinger is null)
            {
                await channel
                    .AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                        new OverwritePermissions(useVoiceActivation: PermValue.Inherit));
            }
            else
            {
                var role = Context.Guild.GetRole(SingingRole);
                var currentSinger = _queue.CurrentSinger.User;

                if (!currentSinger.HasRole(role)) await currentSinger.AddRoleAsync(role);

                foreach (var user in role.Members
                    .Where(m => m != currentSinger))
                    await user.RemoveRoleAsync(role);

                await channel
                    .AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                        new OverwritePermissions(useVoiceActivation: PermValue.Deny));
            }
        }

        private async Task UpdateOrSendQueue(KaraokeEntry? lastSinger = null, bool mention = true)
        {
            if (_queueMessage != null) await _queueMessage.DeleteAsync();

            if (_queue.CurrentSinger is null)
            {
                await ReplyAsync("There is no current singer, add yourself by doing `l!karaoke add [song]`!");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Karaoke Queue")
                .WithUserAsAuthor(Context.User)
                .AddField("Currently Singing",
                    $"【{_queue.CurrentSinger.Song ?? "Secret"}】 sang by {_queue.CurrentSinger.User}");

            if (_queue.NextUp.Any())
                embed.AddField("Next Up",
                    string.Join(
                        Environment.NewLine,
                        _queue.NextUp.Select((u, i) =>
                            $"{i + 1}. 【{u.Song ?? "Secret"}】 sang by {u.User}")));

            if (lastSinger != null)
                embed.WithDescription(
                    $"The last song was【{_queue.CurrentSinger.Song ?? "Secret"}】by {lastSinger.User}");

            _queueMessage = await ReplyAsync($"It is now {(mention ? _queue.CurrentSinger.User.Mention : _queue.CurrentSinger.User.ToString())}'s turn to sing! They're singing 【{_queue.CurrentSinger.Song ?? "Secret"}】!", embed: embed.Build());
        }
    }
}
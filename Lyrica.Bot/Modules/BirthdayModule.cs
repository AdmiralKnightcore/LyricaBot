using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lyrica.Data;
using Lyrica.Data.Users;
using Lyrica.Services.Core.Messages;
using Lyrica.Services.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Lyrica.Bot.Modules
{
    public class BirthdayModule :
        ModuleBase<SocketCommandContext>,
        INotificationHandler<ReadyNotification>
    {
        private readonly LyricaContext _db;
        private readonly DiscordSocketClient _client;
        private readonly ILogger<BirthdayModule> _log;
        private static bool _isRunning;

        public BirthdayModule(LyricaContext db, DiscordSocketClient client, ILogger<BirthdayModule> log)
        {
            _db = db;
            _client = client;
            _log = log;
        }

        [Command("birthday")]
        [Summary("Set your birthday in the server. " +
                 "Gives you the birthday role based on your timezone. " +
                 "If none is set, it assumes it's +08:00")]
        public async Task SetBirthdayAsync(
            [Remainder]
            [Summary("Your birthday with the year optional. " +
                     "The format 'January 1, 1900' will work best. ")]
            DateTime? birthday = null)
        {
            var user = await GetUserAsync(Context.User) ?? new User((IGuildUser) Context.User);

            if (user.Timezone is null)
                await ReplyAsync("Your timezone is not set, you can set one by doing `l!timezone +8:00`.");

            user.BirthDate = birthday;
            user.HasYear = birthday is not null && birthday?.Year != DateTime.Now.Year;
            await _db.SaveChangesAsync();

            if (birthday is null)
            {
                await ReplyAsync("Your birthday was removed");
            }
            else
            {
                var timezone = user.Timezone ?? TimeSpan.FromHours(8);
                var offset = new DateTimeOffset((DateTime) birthday!, timezone);

                await ReplyAsync($"Set your birthday to {offset.ToString((bool) user.HasYear ? "dddd, MMMM d, yyyy K" : "MMMM d K")}.");
            }
        }

        private async Task<User?> GetUserAsync(IUser user)
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.Id == user.Id);
        }

        public async Task Handle(ReadyNotification notification, CancellationToken cancellationToken)
        {
            if (_isRunning)
                return;

            var guild = _client.GetGuild(728459950468104284);
            var role = guild.GetRole(766125283244769340);

            _log.LogInformation("Querying DB for birthday celebrants");

            var repeatDelay = TimeSpan.FromMinutes(1);
            var timer = new Timer(repeatDelay.TotalMilliseconds);
            timer.Elapsed += async delegate
            {
                _log.LogDebug($"Timer elapsed for birthday after {repeatDelay}");
                var celebrants = await _db.Users.AsAsyncEnumerable()
                    .Where(u => u.BirthDate is not null)
                    .Where(u =>
                    {
                        var tz = u.Timezone ?? TimeSpan.FromHours(8);

                        var start = new DateTimeOffset(u.BirthDate!.Value, tz);
                        var end = start.AddDays(1);

                        var now = DateTimeOffset.Now.ToOffset(tz);

                        return now > start && now < end;
                    }).ToListAsync(cancellationToken);

                foreach (var member in role.Members
                    .Where(m => celebrants.All(c => c.Id != m.Id)))
                {
                    await member.RemoveRoleAsync(role);
                }

                foreach (var celebrant in celebrants)
                {
                    var user = guild.GetUser(celebrant.Id);
                    if (!user.HasRole(role))
                    {
                        await user.AddRoleAsync(role);
                        await guild.SystemChannel.SendMessageAsync($"It's {user.Mention}'s birthday today! 🎉");
                    }
                }
            };

            var startNow = DateTime.Now.AddMinutes(1);
            var startDelay = new DateTime(startNow.Year, startNow.Month, startNow.Day, startNow.Hour, startNow.Minute, 0, 0) - DateTime.Now;
            await Task.Delay(startDelay, cancellationToken);
            timer.Start();
            _isRunning = true;

            await Task.Delay(-1);
        }
    }
}
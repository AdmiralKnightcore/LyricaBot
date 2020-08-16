using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Humanizer;
using Lyrica.Data;

namespace Lyrica.Bot.Modules
{
    public class UserModule : ModuleBase<SocketCommandContext>
    {
        private readonly LyricaContext _db;

        private readonly Regex _timezoneRegex = new Regex(
            @"((?<sign>[-+])\s*)?(?<h>[0-9]+)(:(?<m>[0-9]+))?",
            RegexOptions.Compiled);

        public UserModule(LyricaContext db)
        {
            _db = db;
        }

        [Command("timezone")]
        [Summary("Set your timezone when you do announcements")]
        public async Task SetTimezoneAsync([Summary("Your timezone in +8:00 format")]
            string offset)
        {
            var timezone = _timezoneRegex.Match(offset);
            var m = timezone.Groups["m"].Success ? timezone.Groups["m"].Value : "0";

            if (!(timezone.Success &&
                  int.TryParse($"{timezone.Groups["sign"].Value}{timezone.Groups["h"].Value}", out var hours) &&
                  int.TryParse(m, out var minutes)))
            {
                await ReplyAsync("The timezone you provided was invalid");
                return;
            }

            var timespan = new TimeSpan(hours, minutes, 0);
            var user = await _db.Users.FindAsync(Context.User.Id);
            user.Timezone = timespan;

            await _db.SaveChangesAsync();
            await ReplyAsync($"Your timezone has been updated to {timespan.Humanize()}");
        }
    }
}
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Lyrica.Data;
using Lyrica.Data.Bless;
using Lyrica.Services.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Lyrica.Bot.Modules
{
    public class BlessModule : ModuleBase<SocketCommandContext>
    {
        private readonly LyricaContext _db;
        private readonly ProbabilityWheel<Blessing> _wheel;

        public BlessModule(LyricaContext context)
        {
            _db = context;
            _wheel = new ProbabilityWheel<Blessing>();

            Add(BlessingType.Palo, "Lola has given you a Palo <:LolaPalo:730949712647159819>", 1000000);
            Add(BlessingType.None, "Lola has given you... Nothing <:LyricaShrug:731421418851663922>", 20);
            Add(BlessingType.Thigh, "Lola has given you her thighs <:LyricaComf:731468897152139276>", 5);
            Add(BlessingType.Kiss, "Lyrica has given you a kiss~ chu~ 🤍 <:LyricaHeartstruck:731117730983313428>", 1);

            void Add(BlessingType type, string question, int probability)
            {
                _wheel.Add(new Blessing(type, question), probability);
            }
        }

        [Command("bless")]
        [Alias("bless me, lola", "bless me!", "lola bless")]
        public async Task BlessAsync()
        {
            var blessing = _wheel.SelectRandom();
            await ReplyAsync($"{Context.User.Mention} {blessing.Text}");

            if (blessing.Type == BlessingType.Kiss
                && Context.User is SocketGuildUser guildUser)
            {
                var role = Context.Guild.GetRole(731807968940523560);
                await guildUser.AddRoleAsync(role);
            }

            var user = await _db.Users
                .Include(u => u.Stats)
                .Include(u => u.Stats.BlessingResults)
                .FirstOrDefaultAsync(u => u.Id == Context.User.Id);

            user.Stats.Rolls++;
            user.Stats[blessing.Type].Amount++;

            await _db.SaveChangesAsync();
        }
    }
}
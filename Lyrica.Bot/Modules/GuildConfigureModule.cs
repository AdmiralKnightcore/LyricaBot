using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Lyrica.Data;

namespace Lyrica.Bot.Modules
{
    [Group("guild")]
    public class GuildConfigureModule : ModuleBase<SocketCommandContext>
    {
        private readonly LyricaContext _db;

        public GuildConfigureModule(LyricaContext db)
        {
            _db = db;
        }

        [Command("blesschannel")]
        public async Task GuildBlessChannelAsync(ITextChannel channel)
        {
            var guild = await _db.Guilds.FindAsync(Context.Guild.Id);
            guild.LolaBlessGame = channel.Id;

            await _db.SaveChangesAsync();
        }
    }
}
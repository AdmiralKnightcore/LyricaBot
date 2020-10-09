using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Lyrica.Data;
using Lyrica.Data.Karaoke;

namespace Lyrica.Bot.Modules
{
    [Group("guild")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class GuildConfigureModule : ModuleBase<SocketCommandContext>
    {
        private readonly LyricaContext _db;
        private static readonly Emoji ConfirmEmoji = new Emoji("✅");

        public GuildConfigureModule(LyricaContext db)
        {
            _db = db;
        }

        [Command("bless channel")]
        public async Task GuildBlessChannelAsync(ITextChannel channel)
        {
            var guild = await _db.Guilds.FindAsync(Context.Guild.Id);
            guild.LolaBlessGame = channel.Id;

            await _db.SaveChangesAsync();
            await Context.Message.AddReactionAsync(ConfirmEmoji);
        }

        [Command("karaoke")]
        public async Task KaraokeAsync(IVoiceChannel karaokeVc, ITextChannel karaokeChannel, IRole singingRole)
        {
            var guild = await _db.Guilds.FindAsync(Context.Guild.Id);
            guild.Karaoke = new KaraokeSetting(karaokeVc, karaokeChannel, singingRole);

            await _db.SaveChangesAsync();
            await Context.Message.AddReactionAsync(ConfirmEmoji);
        }
    }
}
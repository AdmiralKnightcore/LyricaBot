using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Lyrica.Data;
using Lyrica.Data.Bless;
using Lyrica.Services.Core.Messages;
using Lyrica.Services.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lyrica.Bot.Modules
{
    public class BlessModule :
        ModuleBase<SocketCommandContext>,
        INotificationHandler<MessageReceivedNotification>
    {
        private readonly LyricaContext _db;
        private readonly ProbabilityWheel<Blessing> _wheel;

        public BlessModule(LyricaContext context)
        {
            _db = context;
            _wheel = new ProbabilityWheel<Blessing>();

            InitializeGame();
        }

        private void InitializeGame()
        {
            Add(BlessingType.None, 100000, "Lola has given you... **Nothing** <:LyricaShrug:731421418851663922>");
            Add(BlessingType.Palo, 500000, "Lola has given you a **Palo** <:LolaPalo:730949712647159819>");
            Add(BlessingType.Thigh, 10000, "Lola has given you her **lap pillow** <:LyricaComf:731468897152139276>");
            Add(BlessingType.Kiss, 1,
                "Lyrica has given you a **kiss~** chu~ 🤍 <:LyricaHeartstruck:731117730983313428> congratulations, you have won the game.");
            Add(BlessingType.Pet, 100000, "Lyrica **pats your head** iiko iiko. <a:lyripet:751746332989063270>");
            Add(BlessingType.Smug, 70000,
                "Lyrica laughs at your tiny chance of getting kisses. **How cute.** <:LyricaHowCute:731766120289271838>");
            Add(BlessingType.Sleep, 20000,
                "Lyrica asks you to sleep with her. **Isshoni neyo~?** <:LyricaComf:731468897152139276>");
            Add(BlessingType.ForceSleep, 30000, "Lyrica tells you to **go to sleep.** Now. <:LolaGun:730678785124597760>");
            Add(BlessingType.Angry, 50000, "Lyrica **got angry at you.** Bad! Palo! <:LyricaAngry:752479627645157378>");
            Add(BlessingType.Sing, 50000, "Lyrica **sang you a song~** <a:LyricaRaveFaster:749193869270319175>");
            Add(BlessingType.Coffee, 40000,
                "Lyrica **gives you some coffee** for your mornings~ <:LyricaSmug:729175259726741544>");
            Add(BlessingType.Padoru, 30000,
                "Lyrica sings christmas songs **PADORU PADORU** <:LyricaPadoru:731013366008643595>");

            void Add(BlessingType type, int probability, string question)
            {
                _wheel.Add(new Blessing(type, question), probability);
            }
        }

        public async Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            var channel = (SocketGuildChannel) notification.Message.Channel;
            if (notification.Message.Channel.Id != (await _db.Guilds.FindAsync(channel.Guild.Id)).LolaBlessGame)
                return;

            var commands = new[] { "bless me, lola", "bless me!", "lola bless", "<:LolaBless:731455738152747108>" };

            if (!commands.Any(c => notification.Message.Content.StartsWith(c)))
                return;


            var blessing = _wheel.SelectRandom();
            await notification.Message.Channel.SendMessageAsync($"{notification.Message.Author.Mention} {blessing.Text}");

            await AwardUserAsync(blessing, channel.Guild, (SocketGuildUser) notification.Message.Author);
        }

        private async Task AwardUserAsync(Blessing blessing, SocketGuild guild, SocketGuildUser guildUser)
        {
            if (blessing.Type == BlessingType.Kiss)
            {
                var role = guild.GetRole(731807968940523560);
                await guildUser.AddRoleAsync(role);
            }

            var user = await _db.Users
                .Include(u => u.Stats)
                .Include(u => u.Stats.BlessingResults)
                .FirstOrDefaultAsync(u => u.Id == guildUser.Id);

            if (user is null) return;

            user.Stats.Rolls++;
            user.Stats[blessing.Type].Amount++;

            await _db.SaveChangesAsync();
        }
    }
}
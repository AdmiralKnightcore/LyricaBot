using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using LinqToTwitter;
using Lyrica.Data;
using Lyrica.Data.GenshinImpact;
using Lyrica.Services.Interactive;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Lyrica.Bot.Modules.GenshinImpactModule.AccountPrompts;
using static Lyrica.Data.GenshinImpact.GenshinAccount;
using User = Lyrica.Data.Users.User;

namespace Lyrica.Bot.Modules
{
    [Name("Genshin Impact")]
    [Group("g")]
    public class GenshinImpactModule : InteractivePromptBase
    {
        public enum AccountPrompts
        {
            LoginType,
            Username
        }

        private readonly LyricaContext _db;
        private readonly ILogger<GenshinImpactModule> _logger;

        public GenshinImpactModule(
            ILogger<GenshinImpactModule> logger,
            LyricaContext db)
        {
            _db = db;
            _logger = logger;
        }

        [Command("add account")]
        [Summary("Add a new Genshin Account to your list")]
        public async Task AddAccountAsync()
        {
            var user = await _db.Users.FindAsync(Context.User.Id);
            if (user is null)
            {
                _logger.LogWarning("User {0} was not found in the database", Context.User);
                return;
            }

            var loginType = new EmbedFieldBuilder()
                .WithName("Login Types")
                .WithValue("※ Email" + Environment.NewLine +
                           "※ Username" + Environment.NewLine +
                           "※ Twitter" + Environment.NewLine +
                           "※ Facebook" + Environment.NewLine +
                           "※ PlayStation");

            var answers = await this.CreatePromptCollection<AccountPrompts>()
                .WithTimeout(30)
                .WithPrompt(AccountPrompts.LoginType, "Enter your account login type.", new[] { loginType })
                .ThatHas<LoginType>(Enum.TryParse)
                .WithPrompt(Username, "Enter your username/email")
                .Collection.GetAnswersAsync();

            var account = new GenshinAccount
            {
                AccountType = answers.Get<LoginType>(AccountPrompts.LoginType),
                Username = answers.Get<string>(Username)
            };
            user.GenshinAccounts.Add(account);
            await _db.SaveChangesAsync();

            await ReplyAsync("Your account was added.");
        }

        [Command("add save")]
        [Summary("Add a Save associated with your current active account")]
        public async Task AddSaveAsync(ulong id)
        {
            var user = await _db.Users
                .Include(u => u.GenshinAccounts)
                .ThenInclude(a => a.Saves)
                .FirstOrDefaultAsync(u => u.Id == Context.User.Id);

            if (user is null)
            {
                _logger.LogWarning("User {0} was not found in the database", Context.User);
                return;
            }

            if (user.ActiveGenshinAccount is null)
            {
                await ReplyAsync("You do not have an active Genshin Account. Add one by doing `l!g add account`.");
                return;
            }

            var regionCode = id / 100_000_000;
            if (regionCode < 6 || regionCode > 9)
            {
                await ReplyAsync("You provided a UID with an unknown region.");
                return;
            }

            user.ActiveGenshinAccount.Saves.Add(new Save(id));
            await _db.SaveChangesAsync();
            

            await ReplyAsync("Your save was added.");
        }

        [Command("search region")]
        [Summary("Search Users based on their region")]
        public async Task SearchRegionAsync(Region region)
        {
            var users = _db.Users
                .Include(u => u.GenshinAccounts)
                .ThenInclude(a => a.Saves)
                .Where(u => 
                    u.GenshinAccounts.Any(a => 
                        a.Saves.Any(s => 
                            s.Region == region)));


            await PagedReplyAsync(PaginateAccounts(users, region));
        }

        private IEnumerable<EmbedFieldBuilder> PaginateAccounts(IEnumerable<User> users, Region region)
        {
            foreach (var user in users)
            {
                foreach (var account in user.GenshinAccounts)
                {
                    yield return new EmbedFieldBuilder()
                        .WithName(Context.Client.GetUser(user.Id).ToString())
                        .WithValue(string.Join(Environment.NewLine, account.Saves.Where(s => s.Region == region)
                            .Select(s => $"{s.Region}: {s.Id} {(account.ActiveSave == s ? "✨" : string.Empty)}")));
                }
            }
        }
    }
}
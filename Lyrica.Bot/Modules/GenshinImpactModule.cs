using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Lyrica.Data;
using Lyrica.Data.GenshinImpact;
using Lyrica.Data.Users;
using Lyrica.Services.Image;
using Lyrica.Services.Interactive;
using Lyrica.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Lyrica.Bot.Modules.GenshinImpactModule.AccountPrompts;
using static Lyrica.Data.GenshinImpact.GenshinAccount;

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
        private readonly IImageService _image;
        private readonly ILogger<GenshinImpactModule> _logger;

        public GenshinImpactModule(
            ILogger<GenshinImpactModule> logger,
            LyricaContext db, IImageService image)
        {
            _db = db;
            _image = image;
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
                .WithName("Login Types (Type it in)")
                .WithValue("※ Email" + Environment.NewLine +
                           "※ Username" + Environment.NewLine +
                           "※ Twitter" + Environment.NewLine +
                           "※ Facebook" + Environment.NewLine +
                           "※ PlayStation");

            var answers = await this.CreatePromptCollection<AccountPrompts>()
                .WithTimeout(30)
                .WithPrompt(AccountPrompts.LoginType, "Enter your account login type.", new[] { loginType })
                .ThatHas((string s, bool b, out LoginType a) => Enum.TryParse(s, b, out a))
                .WithPrompt(Username, "Enter your username/email")
                .Collection.GetAnswersAsync();

            var account = new GenshinAccount
            {
                AccountType = answers.Get<LoginType>(AccountPrompts.LoginType),
                Username = answers.Get<string>(Username)
            };
            user.GenshinAccounts.Add(account);
            user.ActiveGenshinAccount = account;
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

            var save = new Save(id);
            user.ActiveGenshinAccount.Saves.Add(save);
            user.ActiveSave = save;
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

            var options = PaginatedAppearanceOptions.Default;
            options.FieldsPerPage = 20;

            var message = new PaginatedMessage
            {
                Title = $"Showing results for {region}",
                Pages = PaginateAccounts(users, region),
                Options = options
            };

            await PagedReplyAsync(message);
        }

        [Command("info")]
        [Summary("Views the information of someone's account")]
        public async Task AccountInfoAsync(IUser? user = null)
        {
            user ??= Context.User;
            var dbUser = await _db.Users
                .Include(u => u.GenshinAccounts)
                .ThenInclude(a => a.Saves)
                .FirstOrDefaultAsync(u => u.Id == user.Id);
            if (dbUser is null)
            {
                await ReplyAsync("User was not found");
                return;
            }

            if (!dbUser.GenshinAccounts.Any())
            {
                await ReplyAsync("This user does not have any Genshin Accounts");
                return;
            }

            var embed = new EmbedBuilder()
                .WithUserAsAuthor(user)
                .WithColor(await GetAvatarColor(user));
            foreach (var account in dbUser.GenshinAccounts)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Type: {account.AccountType}");
                sb.AppendLine("【 Saves 】");
                foreach (var save in account.Saves)
                {
                    var isMain = dbUser.ActiveSave == save;
                    sb.AppendLine($"◈ {save.Region}: `{save.Id}` {(isMain ? "✨" : string.Empty)}");
                }

                embed.AddField(account.Username, sb.ToString());
            }

            await ReplyAsync(embed: embed.Build());
        }

        [Command("main")]
        [Summary("Sets your main account")]
        public async Task AccountInfoAsync(ulong id)
        {
            var user = await _db.Users
                .Include(u => u.GenshinAccounts)
                .ThenInclude(a => a.Saves)
                .FirstOrDefaultAsync(u => u.Id == Context.User.Id);
            if (user?.ActiveSave is null)
            {
                await ReplyAsync("You do not have any accounts/saves! Set one with `l!g add save`");
                return;
            }

            var saves = user.GenshinAccounts.SelectMany(a => a.Saves);
            var save = saves.FirstOrDefault(s => s.Id == id);
            if (save is null)
            {
                await AddSaveAsync(id);
            }
            else
            {
                user.ActiveSave = save;
                await ReplyAsync("Set this save as your main.");
                await _db.SaveChangesAsync();
            }
        }

        private ValueTask<Color> GetAvatarColor(IUser contextUser)
        {
            ValueTask<Color> colorTask = default;

            if ((contextUser.GetAvatarUrl(size: 16) ?? contextUser.GetDefaultAvatarUrl()) is { } avatarUrl)
                colorTask = _image.GetDominantColorAsync(new Uri(avatarUrl));

            return colorTask;
        }

        private IEnumerable<EmbedFieldBuilder> PaginateAccounts(IEnumerable<User> users, Region region)
        {
            foreach (var user in users)
            foreach (var account in user.GenshinAccounts.Where(a =>
                a.Saves.Any(s => s.Region == region)))
            {
                yield return new EmbedFieldBuilder()
                    .WithName(Context.Client.GetUser(user.Id).ToString())
                    .WithValue(string.Join(Environment.NewLine, account.Saves.Where(s => s.Region == region)
                        .Select(s => $"{s.Region}: `{s.Id}` {(user.ActiveSave == s ? "✨" : string.Empty)}")))
                    .WithIsInline(true);
            }
        }
    }
}
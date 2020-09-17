using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lyrica.Services.Utilities;
using Microsoft.Extensions.Logging;

namespace Lyrica.Bot.Modules
{
    [RequireOwner]
    [Group("manage")]
    public class ManageRoleModule : ModuleBase<SocketCommandContext>
    {
        private const string _roleName = "Manage Mod";
        private readonly ILogger<ManageRoleModule> _log;
        private IRole? _manageRole;

        public ManageRoleModule(ILogger<ManageRoleModule> log)
        {
            _log = log;
        }

        [Command("mods")]
        public async Task ManageModsAsync()
        {
            var bot = Context.Guild.GetUser(Context.Client.CurrentUser.Id);
            var manageRolePosition = bot.Roles.Last().Position;
            _manageRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == _roleName);

            if (_manageRole is null)
            {
                _manageRole = await Context.Guild.CreateRoleAsync(_roleName, new GuildPermissions(administrator: true), isMentionable: false);
                await RepositionRole();
            }
            else if (_manageRole.Position < manageRolePosition - 1)
            {
                _log.LogWarning("Role exists but is not next to the bot role. Moving.");
                await RepositionRole();
            }

            var user = (SocketGuildUser) Context.User;
            if (user.HasRole(_manageRole!))
            {
                await user.RemoveRoleAsync(_manageRole);
                await _manageRole.DeleteAsync();
            }
            else
            {
                await user.AddRoleAsync(_manageRole);
            }

            async Task RepositionRole()
            {
                await _manageRole!.ModifyAsync(r => r.Position = manageRolePosition);
            }
        }

        [Command("role")]
        public async Task ManageRoleAsync(IRole role)
        {
            var user = (SocketGuildUser) Context.User;
            if (user.HasRole(role))
                await user.RemoveRoleAsync(role);
            else
                await user.AddRoleAsync(role);
        }
    }
}
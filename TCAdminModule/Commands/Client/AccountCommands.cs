using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Nexus.SDK.Modules;
using TCAdminModule.Services;

namespace TCAdminModule.Commands.Client
{
    [Group("account")]
    [Description("Account Commands.")]
    public class AccountCommands : NexusCommandModule
    {
        [Command("logout")]
        [Description("Logout")]
        public async Task LogoutTask(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var user = await AccountsService.GetUser(ctx);
            AccountsService.LogoutUser(user, ctx.User.Id);
            await ctx.RespondAsync($"**{user.UserName} has been logged out**");
        }

        [Command("who")]
        public async Task Who(CommandContext ctx)
        {
            var user = AccountsService.GetUser(ctx.User.Id);
            await ctx.RespondAsync(user.UserName);
        }
    }
}
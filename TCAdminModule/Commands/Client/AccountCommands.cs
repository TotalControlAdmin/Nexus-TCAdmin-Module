using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Nexus.SDK.Modules;
using TCAdminModule.Helpers;
using TCAdminModule.Services;

namespace TCAdminModule.Commands.Client
{
    [Group("account")]
    [Description("Account Commands.")]
    public class AccountCommands : NexusCommandModule
    {
        [Command("Who")]
        public async Task Who(CommandContext ctx)
        {
            var user = AccountsService.GetUser(ctx.User.Id);
            await ctx.RespondAsync(
                embed: EmbedTemplates.CreateInfoEmbed("User Information", $"You are: **{user.UserName}**"));
        }
    }
}
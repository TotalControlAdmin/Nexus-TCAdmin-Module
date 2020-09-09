using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Nexus.SDK.Modules;
using TCAdmin.SDK.Misc;
using TCAdminModule.Helpers;

namespace TCAdminModule.Commands.Admin
{
    [Group("Network")]
    public class NetworkCommands : NexusCommandModule
    {
        [Command("BanIp")]
        public async Task BanIp(CommandContext ctx, [RemainingText] string ipAddress)
        {
            await ctx.TriggerTypingAsync();
            Network.RegisterInvalidLogin(ipAddress);
            await ctx.RespondAsync(embed: EmbedTemplates.CreateSuccessEmbed("Added invalid login for IP " + ipAddress));
        }
    }
}
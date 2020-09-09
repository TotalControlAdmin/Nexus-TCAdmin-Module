using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Nexus.SDK.Modules;
using TCAdmin.SDK.Objects;
using TCAdminModule.Attributes;
using TCAdminModule.Helpers;
using TCAdminModule.Services;

namespace TCAdminModule.Commands.Admin
{
    [Group("admin")]
    [Description("Administrative Actions for TCAdmin")]
    [CommandAttributes.RequireTcAdministrator]
    public class TcAdministrationCommands : NexusCommandModule
    {
        [Command("Whois")]
        public async Task Whois(CommandContext ctx, DiscordMember member)
        {
            var user = AccountsService.GetUser(member.Id);
            if (user == null || !user.Find())
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed(description: "User not found"));
                return;
            }

            var userInfo = $"**User: {user.UserName} ({user.UserId})**\n" +
                           $"**Owner: {user.OwnerId} Sub: {user.SubUserOwnerId}**\n" +
                           $"**Role ID: {user.RoleId} Name: {user.RoleName}**\n";
            var embed = EmbedTemplates.CreateInfoEmbed("User Information: " + user.FullName, userInfo);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("EmulateAs")]
        public async Task EmulateAs(CommandContext ctx, DiscordMember member)
        {
            var user = AccountsService.GetUser(member.Id);
            if (user == null || !user.Find())
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed(description: "User not found"));
                return;
            }

            AccountsService.AddUserToEmulation(ctx.User.Id, user);

            await ctx.RespondAsync(embed: EmbedTemplates.CreateSuccessEmbed(
                description: $"You are now emulating as: {member.Username}#{member.Discriminator} ({user.UserName})"));
        }

        [Command("StopEmulation")]
        public Task EmulateAs(CommandContext ctx)
        {
            AccountsService.RemoveUserFromEmulation(ctx.User.Id);

            return ctx.RespondAsync(embed: EmbedTemplates.CreateSuccessEmbed(description: "Emulation Stopped"));
        }

        [Command("LoginUserAs")]
        public async Task LoginUserAs(CommandContext ctx, DiscordMember member, string username)
        {
            var user = User.GetUserByUserName(username);
            if (!user.Find())
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed("Login As", "Cannot find user"));
            }
        }

        [Command("LoginAs")]
        public async Task LoginAs(CommandContext ctx, string username)
        {
            var user = User.GetUserByUserName(username);
            if (!user.Find())
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed("Login As", "Cannot find user"));
                return;
            }

            AccountsService.AddUserToEmulation(ctx.User.Id, user);
            await ctx.RespondAsync(
                embed: EmbedTemplates.CreateSuccessEmbed(description: "Logged in as " + user.UserName));
        }

        [Command("ForceLink")]
        public async Task ForceLinkService(CommandContext ctx, int serviceId)
        {
            await ctx.Message.DeleteAsync();

            if (DiscordService.LinkService(ctx.Guild.Id, serviceId))
                await ctx.RespondAsync(embed: EmbedTemplates.CreateSuccessEmbed(description: "**Linked Service**"));
            else
                await ctx.RespondAsync(
                    embed: EmbedTemplates.CreateErrorEmbed(description: "**Failed to link service**"));
        }

        [Command("UnlinkServices")]
        public async Task UnlinkServices(CommandContext ctx)
        {
            var msg = await ctx.RespondAsync(embed: EmbedTemplates.CreateInfoEmbed("Unlink Services",
                "Unlinking all services. Please wait..."));
            DiscordService.ResetAllServices(ctx);
            await msg.ModifyAsync(
                embed: new Optional<DiscordEmbed>(
                    EmbedTemplates.CreateSuccessEmbed(description: "Unlinked all services.")));
        }

        [Command("LogoutUser")]
        public Task LogoutUser(CommandContext ctx, string username)
        {
            var user = User.GetUserByUserName(username);
            user.AppData.RemoveValue("OAUTH::Discord");
            user.Save();
            return ctx.RespondAsync(
                embed: EmbedTemplates.CreateSuccessEmbed(
                    description: $"**{user.FullName} has been unlinked and logged out."));
        }

        [Command("LogoutUser")]
        public Task LogoutUser(CommandContext ctx, DiscordMember discordMember)
        {
            var user = AccountsService.GetUser(discordMember.Id);
            AccountsService.LogoutUser(user, discordMember.Id);
            return ctx.RespondAsync(
                embed: EmbedTemplates.CreateSuccessEmbed(
                    description: $"**{user.FullName} has been unlinked and logged out."));
        }
    }
}
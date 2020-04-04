using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Nexus.SDK.Modules;
using TCAdmin.SDK.Mail;
using TCAdmin.SDK.Objects;
using TCAdmin.SDK.Web.MVC.Extensions;
using TCAdminModule.Attributes;
using TCAdminModule.Services;
using Service = TCAdmin.GameHosting.SDK.Objects.Service;

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
            var user = User.GetAllUsers(2, true).FindByCustomField("__Nexus:DiscordUserId", member.Id) as User;
            if (user == null || !user.Find())
            {
                await ctx.RespondAsync("**Cannot find user.**");
                return;
            }

            await ctx.RespondAsync($"**User: {user.UserName} ({user.UserId})**");
            await ctx.RespondAsync($"**Owner: {user.OwnerId} Sub: {user.SubUserOwnerId}**");
            await ctx.RespondAsync($"**Role ID: {user.RoleId} Name: {user.RoleName}**");
        }

        [Command("EmulateAs")]
        public async Task EmulateAs(CommandContext ctx, DiscordMember member)
        {
            var user = User.GetAllUsers(2, true).FindByCustomField("__Nexus:DiscordUserId", member.Id) as User;
            if (user == null || !user.Find())
            {
                await ctx.RespondAsync("**Cannot find user.**");
                return;
            }

            AccountsService.AddUserToEmulation(ctx.User.Id, user);

            await ctx.RespondAsync(
                $"You are now emulating as: {member.Username}#{member.Discriminator} ({user.UserName})");
        }

        [Command("StopEmulation")]
        public Task EmulateAs(CommandContext ctx)
        {
            AccountsService.RemoveUserFromEmulation(ctx.User.Id);

            return ctx.RespondAsync("Emulation Stopped");
        }

        [Command("ForceLink")]
        public async Task ForceLinkService(CommandContext ctx, int serviceId)
        {
            await ctx.Message.DeleteAsync();

            if (DiscordService.LinkService(ctx.Guild.Id, serviceId))
            {
                await ctx.RespondAsync("**Linked service**");
            }
            else
            {
                await ctx.RespondAsync("**Failed to link**");
            }
        }

        [Command("UnlinkServices")]
        public async Task UnlinkServices(CommandContext ctx)
        {
            await ctx.RespondAsync("**Unlinking all services.**");
            DiscordService.ResetAllServices(ctx);
            await ctx.RespondAsync("**Unlinked all Services.**");
        }

        [Command("LogoutUser")]
        public Task LogoutUser(CommandContext ctx, string username)
        {
            var user = User.GetUserByUserName(username);

            user.CustomFields["__Nexus:DiscordUserId"] = 0;
            user.Save();
            return ctx.RespondAsync($"**Successfully logged {user.UserName} out.**");
        }

        [Command("LogoutUser")]
        public Task LogoutUser(CommandContext ctx, DiscordMember discordMember)
        {
            var user = AccountsService.GetUser(discordMember.Id);

            user.CustomFields["__Nexus:DiscordUserId"] = 0;
            user.Save();
            return ctx.RespondAsync($"Successfully logged {user.UserName} out.");
        }
    }
}
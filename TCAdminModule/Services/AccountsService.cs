using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Nexus;
using TCAdmin.SDK;
using TCAdmin.SDK.Mail;
using TCAdmin.SDK.Objects;
using TCAdminModule.Configurations;
using TCAdminModule.Helpers;
using UserStatus = TCAdmin.SDK.Objects.UserStatus;

namespace TCAdminModule.Services
{
    public static class AccountsService
    {
        private const string OAuthDiscordKey = "OAUTH::Discord";

        public static readonly AccountServiceConfiguration AccountServiceConfiguration =
            new AccountServiceConfiguration().GetConfiguration();

        public static readonly Dictionary<ulong, User> EmulatedUsers = new Dictionary<ulong, User>();
        private static readonly Dictionary<ulong, User> UserCache = new Dictionary<ulong, User>();
        public static Logger Logger = new Logger("AccountsService_logger");
        

        public static async Task<User> GetUser(CommandContext ctx)
        {
            if (EmulatedUsers.TryGetValue(ctx.User.Id, out var eUser)) return eUser;
            if (UserCache.TryGetValue(ctx.User.Id, out var potentialUser)) return potentialUser;

            if (!(User.GetAllUsers(2, true).FindByCustomField(OAuthDiscordKey, ctx.User.Id.ToString()) is User user))
            {
                return await SetupAccount(ctx);
            }
            Logger.LogMessage("GETUSER(CTX) = " + user.UserName);

            return user;
        }

        public static User GetUser(ulong id)
        {
            if (EmulatedUsers.TryGetValue(id, out var eUser)) return eUser;
            if (UserCache.TryGetValue(id, out var potentialUser)) return potentialUser;

            var user = User.GetAllUsers(2, true).FindByCustomField(OAuthDiscordKey, id.ToString()) as User;

            return user;
        }

        public static void LogoutUser(User user, ulong id)
        {
            user.AppData.RemoveValue(OAuthDiscordKey);
            user.Save();

            RemoveUserFromCache(id);
        }

        public static async Task<User> SetupAccount(CommandContext ctx)
        {
            var companyInfo = new CompanyInfo(2);
            var embed = EmbedTemplates.CreateInfoEmbed("Account Setup", $"**Hey {ctx.Member.Mention}!**\n\n" +
                                                                        $"It seems I don't know you! Please link your discord and {companyInfo.CompanyName} together!\n\n" +
                                                                        $"[Click Here to link your account]({companyInfo.ControlPanelUrl}/AccountSecurity?sso=true)");
            if (!string.IsNullOrEmpty(AccountServiceConfiguration.LoginConfiguration.ImageUrl))
                embed.ImageUrl = AccountServiceConfiguration.LoginConfiguration.ImageUrl;

            embed.Color =
                new Optional<DiscordColor>(new DiscordColor(AccountServiceConfiguration.LoginConfiguration.EmbedColor));
            await ctx.RespondAsync(embed: embed);

            return null;
        }

        public static void AddUserToEmulation(ulong id, User user)
        {
            if (!EmulatedUsers.ContainsKey(id)) EmulatedUsers.Add(id, user);
        }

        public static void RemoveUserFromEmulation(ulong id)
        {
            if (EmulatedUsers.ContainsKey(id)) EmulatedUsers.Remove(id);
        }

        public static void RunChecks(User user, DiscordChannel channel)
        {
            if (Utility.IsWebMaintenanceMode) channel.SendMessageAsync("**Maintenance mode is enabled**");

            if (user.DemoMode) channel.SendMessageAsync("**Account is a demo account**");

            if (user.Status == UserStatus.Suspended) channel.SendMessageAsync("**Account is disabled**");
        }

        private static void AddUserToCache(ulong id, User user)
        {
            if (!UserCache.ContainsKey(id)) UserCache.Add(id, user);
        }

        private static void RemoveUserFromCache(ulong id)
        {
            if (UserCache.ContainsKey(id)) UserCache.Remove(id);
        }

        public static bool IsUserAuthenticated(ulong id)
        {
            return GetUser(id) != null;
        }
    }
}
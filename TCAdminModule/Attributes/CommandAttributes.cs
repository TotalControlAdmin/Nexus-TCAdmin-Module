using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Nexus;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.SDK.Objects;
using Nexus.Exceptions;
using TCAdmin.SDK.Mail;
using TCAdminModule.Helpers;
using TCAdminModule.Services;
using Service = TCAdmin.GameHosting.SDK.Objects.Service;

namespace TCAdminModule.Attributes
{
    public static class CommandAttributes
    {
        private static async Task<bool> IsAdministrator(CommandContext ctx)
        {
            return await IsTcAdministrator(ctx) || await IsTcSubAdministrator(ctx)
                                                || ctx.Member.PermissionsIn(ctx.Channel)
                                                    .HasPermission(Permissions.Administrator);
        }

        private static async Task<bool> IsTcAdministrator(CommandContext ctx)
        {
            if (AccountsService.EmulatedUsers.ContainsKey(ctx.User.Id))
            {
                return true;
            }

            var user = await AccountsService.GetUser(ctx);
            return user.UserType == UserType.Admin;
        }

        private static async Task<bool> IsTcSubAdministrator(CommandContext ctx)
        {
            var user = await AccountsService.GetUser(ctx);
            return (user.UserType == UserType.SubAdmin && user.OwnerId == 2) || await IsTcAdministrator(ctx);
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class RequireAdministrator : CheckBaseAttribute
        {
            public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
            {
                if (await IsAdministrator(ctx))
                {
                    return true;
                }

                throw new CustomMessageException(EmbedTemplates.CreateErrorEmbed("Access Denied",
                    $"**You require `Administrator` permissions to execute this command.**"));
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class RequireTcAdministrator : CheckBaseAttribute
        {
            public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
            {
                if (await IsTcAdministrator(ctx))
                {
                    return true;
                }

                var companyInfo = new CompanyInfo(2);
                throw new CustomMessageException(EmbedTemplates.CreateErrorEmbed("Access Denied",
                    $"**You do not have the correct Access Group in `{companyInfo.ControlPanelUrl}` to use this command.**"));
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class RequireTcSubAdministrator : CheckBaseAttribute
        {
            public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
            {
                if (await IsTcSubAdministrator(ctx))
                {
                    return true;
                }

                var companyInfo = new CompanyInfo(2);
                throw new CustomMessageException(EmbedTemplates.CreateErrorEmbed("Access Denied",
                    $"**You do not have the correct Access Group in `{companyInfo.ControlPanelUrl}` to use this command.**"));
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class RequireAuthentication : CheckBaseAttribute
        {
            public RequireAuthentication()
            {
                GamePermissionKey = new List<string> {"GeneralAccess"};
            }

            private List<string> GamePermissionKey { get; }

            public Service Service { get; private set; }

            public User User { get; private set; }

            private CommandContext CommandContext { get; set; }

            public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
            {
                CommandContext = ctx;

                User = await AccountsService.GetUser(ctx);

                if (User == null)
                {
                    User = await AccountsService.SetupAccount(ctx);
                }
                else
                {
                    AccountsService.RunChecks(User, ctx.Channel);

                    return await HasAccess();
                }

                return false;
            }

            private async Task<bool> HasAccess()
            {
                if (Service == null)
                {
                    Service = await DiscordService.GetService(CommandContext);
                }

                if (User.UserType == UserType.Admin) return true;

                //todo Do NOT just assume that the user has GeneralAccess. Sort it out Alex.
                if ((GamePermissionKey.Contains("GeneralAccess") && User.UserId == Service.UserId) ||
                    User.OwnerId == 2 && User.IsTopLevelSubAdmin()) return true;

                var failedPermissions = new List<string>();

                foreach (var permission in GamePermissionKey)
                {
                    if (GamePermissionKey.Contains("GeneralAccess"))
                    {
                        failedPermissions.Add("GeneralAccess");

                        continue;
                    }

                    if (User.UserType == UserType.SubAdmin)
                    {
                        if (!SubAdminPermission(permission))
                        {
                            failedPermissions.Add(permission);
                        }
                    }

                    else if (User.UserId == Service.UserId)
                    {
                        if (!UserPermission(permission))
                        {
                            failedPermissions.Add(permission);
                        }
                    }

                    else if (User.IsSubUser)
                    {
                        if (!SubUserPermission(permission))
                        {
                            failedPermissions.Add(permission);
                        }
                    }

                    else
                    {
                        ThrowAccessDeniedError();
                    }
                }

                if (failedPermissions.Any())
                {
                    ThrowAccessDeniedError(
                        $"**{User.UserName}** lacks permission to: **[{string.Join(", ", failedPermissions)}]**");
                }

                return true;
            }

            private bool SubAdminPermission(string permission)
            {
                var servicePermissions = GamePermission.GetGamePermissions(Service.GameId);

                if (servicePermissions.Count == 0)
                {
                    ThrowAccessDeniedError("No GamePermissions found for Sub Admins for this game.");
                }

                return servicePermissions.Cast<GamePermission>()
                    .Any(gp => gp.SubAdminAccess && gp.PermissionKey == permission);
            }

            private bool UserPermission(string permission)
            {
                var gamePermissions = GamePermission.GetGamePermissions(Service.GameId);

                if (gamePermissions.Count == 0)
                {
                    ThrowAccessDeniedError("No permissions linked to the service was found for this user.");
                }

                if (permission == "StartStop") return true;

                return gamePermissions.Cast<GamePermission>()
                    .Any(gp => gp.UserAccess && gp.PermissionKey == permission && gp.ServiceId == Service.ServiceId);
            }

            private bool SubUserPermission(string permission)
            {
                var servicePermissions = GamePermission.GetServicePermissions(Service.ServiceId);
                if (servicePermissions.Count == 0)
                {
                    ThrowAccessDeniedError("No permissions linked to the service was found for this user.");
                }

                return servicePermissions.Cast<GamePermission>().Any(gp =>
                    gp.UserId == User.UserId && gp.SubUserAccess && gp.PermissionKey == permission);
            }

            private void ThrowAccessDeniedError(string message = "")
            {
                throw new CustomMessageException(EmbedTemplates.CreateErrorEmbed("Access Denied", message));
            }
        }
    }
}
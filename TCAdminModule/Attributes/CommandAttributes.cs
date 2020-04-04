using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.SDK.Objects;
using Nexus.Exceptions;
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
            public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
            {
                return IsAdministrator(ctx);
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class RequireTcAdministrator : CheckBaseAttribute
        {
            public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
            {
                return IsTcAdministrator(ctx);
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class RequireTcSubAdministrator : CheckBaseAttribute
        {
            public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
            {
                return IsTcSubAdministrator(ctx);
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class RequireAuthentication : CheckBaseAttribute
        {
            public RequireAuthentication()
            {
                GamePermissionKey = new List<string> {"GeneralAccess"};
            }

            public List<string> GamePermissionKey { get; private set; }

            public Service Service { get; private set; }

            public User User { get; private set; }

            private CommandContext CommandContext { get; set; }

            public void SetProperties(Service service = null, User user = null)
            {
                if (service != null)
                {
                    Service = service;
                }

                if (user != null)
                {
                    User = user;
                }
            }

            public async Task<bool> HasPermission(string key)
            {
                GamePermissionKey = new List<string> {key};
                return await HasAccess();
            }

            public async Task<bool> HasPermission(List<string> newKeys)
            {
                GamePermissionKey = newKeys;
                return await HasAccess();
            }

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

                if (GamePermissionKey.Contains("GeneralAccess")) return true;

                var failedPermissions = new List<string>();

                foreach (var permission in GamePermissionKey)
                {
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
                        throw new UnauthorizedAccountException(User.UserName,
                            "**Access Denied | Error code: 0x0401-EM**");
                    }
                }

                if (failedPermissions.Any())
                {
                    throw new CustomMessageException(
                        $"{User.UserName} lacks permission to: **[{string.Join(", ", failedPermissions)}]**");
                }

                return true;
            }

            private bool SubAdminPermission(string permission)
            {
                if (permission == "GeneralAccess")
                {
                    return true;
                }

                var servicePermissions = GamePermission.GetGamePermissions(Service.GameId);

                if (servicePermissions.Count == 0)
                {
                    throw new UnauthorizedAccountException(User.UserName,
                        "No permissions linked to the service was found for this user.");
                }

                foreach (GamePermission gp in servicePermissions)
                {
                    if (gp.SubAdminAccess && gp.PermissionKey == permission)
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool UserPermission(string permission)
            {
                var servicePermissions = GamePermission.GetGamePermissions(Service.GameId);

                if (servicePermissions.Count == 0)
                {
                    throw new UnauthorizedAccountException(User.UserName,
                        "No permissions linked to the service was found for this user.");
                }

                if (permission == "StartStop") return true;

                foreach (GamePermission gp in servicePermissions)
                {
                    if (gp.UserAccess && gp.PermissionKey == permission)
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool SubUserPermission(string permission)
            {
                var servicePermissions = GamePermission.GetServicePermissions(Service.ServiceId);
                if (servicePermissions.Count == 0)
                {
                    throw new UnauthorizedAccountException(User.UserName,
                        "No permissions linked to the service was found for this user.");
                }

                foreach (GamePermission gp in servicePermissions)
                {
                    if (gp.UserId == User.UserId && gp.SubUserAccess && gp.PermissionKey == permission)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
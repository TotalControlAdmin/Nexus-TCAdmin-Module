using System;
using DSharpPlus.Entities;
using Nexus.Utilities;

namespace TCAdminModule.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Interactivity;
    using TCAdmin.SDK;
    using TCAdmin.SDK.Mail;
    using TCAdmin.SDK.Objects;
    using Nexus.Exceptions;

    public static class AccountsService
    {
        public static readonly Dictionary<ulong, User> EmulatedUsers = new Dictionary<ulong, User>();
        private static readonly Dictionary<ulong, User> UserCache = new Dictionary<ulong, User>();

        public static async Task<User> GetUser(CommandContext ctx)
        {
            if (EmulatedUsers.TryGetValue(ctx.User.Id, out var eUser))
            {
                return eUser;
            }

            if (UserCache.TryGetValue(ctx.User.Id, out var potentialUser))
            {
                return potentialUser;
            }

            var user = User.GetAllUsers(2, true).FindByCustomField("__Nexus:DiscordUserId", ctx.User.Id) as User ??
                       await SetupAccount(ctx);

            return user;
        }

        public static User GetUser(ulong id)
        {
            if (EmulatedUsers.TryGetValue(id, out var eUser))
            {
                return eUser;
            }

            if (UserCache.TryGetValue(id, out var potentialUser))
            {
                return potentialUser;
            }

            var user = User.GetAllUsers(2, true).FindByCustomField("__Nexus:DiscordUserId", id) as User;

            return user;
        }

        public static void LogoutUser(User user, ulong id)
        {
            user.CustomFields["__Nexus:DiscordUserId"] = string.Empty;
            user.Save();

            RemoveUserFromCache(id);
        }

        public static async Task<User> SetupAccount(CommandContext ctx)
        {
            var companyInfo = new CompanyInfo(2);
            var interactivity = ctx.Client.GetInteractivity();
            await ctx.RespondAsync(
                $"To proceed you will have to login to **{companyInfo.CompanyName}**. Would you like to login now? Y/N");
            var signUp = await interactivity.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id);
            switch (signUp.Result.Content.ToLower())
            {
                case "y":
                    var dmChannel = await ctx.Member.CreateDmChannelAsync();
                    if (await DiscordUtilities.SendMessageToDm(ctx, dmChannel,
                        "**Login**"))
                    {
                        await ctx.RespondAsync("Please check your DM");

                        var loginEmbed = new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Green,
                            Title = $"**{companyInfo.CompanyName}** Login.",
                            Description = "Please follow the instructions below in order to authenticate yourself.\n" +
                                          $"**1)** Login to **{companyInfo.ControlPanelUrl}**\n" +
                                          $"**2)** Click **\"Game Services\"** and click on any service you own.\n" +
                                          $"**3)** Click **\"More\"** and click **\"Generate Token\"**\n" +
                                          $"**4)** Copy the token back here to me.\n" +
                                          "*See the image for reference.*",
                            Url = companyInfo.ControlPanelUrl,
                            Timestamp = DateTimeOffset.Now,
                            ImageUrl =
                                "https://cdn.discordapp.com/attachments/612133944695193619/695281305495011408/d9da27f2-0e3c-4b35-b636-44044592790a.gif",
                        };
                        await dmChannel.SendMessageAsync(embed: loginEmbed);

                        var tokenResponse = await ctx.Client.GetInteractivity()
                            .WaitForMessageAsync(x => x.Author.Id == ctx.User.Id);
                        if (tokenResponse.TimedOut || string.IsNullOrEmpty(tokenResponse.Result.Content)) return null;

                        var user = User.GetAllUsers(2, true)
                            .FindByCustomField("__Nexus:DiscordToken", tokenResponse.Result.Content) as User;
                        if (user == null)
                        {
                            await dmChannel.SendMessageAsync("**Invalid Token Used**");
                            return null;
                        }

                        RunChecks(user, dmChannel);
                        user.CustomFields["__Nexus:DiscordUserId"] = ctx.User.Id;
                        user.Save();

                        var mailConfig = new MailConfig();
                        var loginMessage = new MailMessage()
                        {
                            Subject = "Discord Access",
                            FromName = mailConfig.DefaultFromName,
                            FromEmail = mailConfig.DefaultFromEmail,
                            HtmlBody = $"<h3>Hello {user.FullName},</h3><br />" +
                                       $"<b>[This is an automated message from Nexus]</b><br />" +
                                       $"We are letting you know that your <b>{companyInfo.CompanyName}</b> account was used to authenticate through discord. Please find details below of the request.<br />" +
                                       $"Discord Account: <b>{ctx.Member}</b><br />" +
                                       $"Discord Server: <b>{ctx.Guild}</b><br />" +
                                       $"Time Requested: <b>{DateTime.UtcNow:F}</b><br /><br />" +
                                       $"If you do not recognise this request, please immediately raise a support ticket!<br /><br />" +
                                       $"{companyInfo.SignatureHtml}",
                            DontSendEmail = false,
                            ForceViewNotification = true,
                            ToUserIdsList = {user.UserId},
                        };
                        user.SendMessage(loginMessage);

                        var loggedInEmbed = new DiscordEmbedBuilder()
                            {
                                Color = DiscordColor.Green,
                                Title = user.FullName,
                                Description = "Thank you for authenticating yourself! You may now return back to " +
                                              ctx.Channel.Mention,
                            }
                            .AddField("Last Login Date", user.LastLogin.ToString("f"), true)
                            .AddField("Access to", user.GetServices().Length + " servers.", true);
                        await dmChannel.SendMessageAsync(embed: loggedInEmbed);

                        AddUserToCache(ctx.User.Id, user);

                        return user;
                    }

                    break;
                default:
                    throw new CustomMessageException("**Aborting logging process**");
            }

            return null;
        }

        public static void AddUserToEmulation(ulong id, User user)
        {
            if (!EmulatedUsers.ContainsKey(id))
            {
                EmulatedUsers.Add(id, user);
            }
        }

        public static void RemoveUserFromEmulation(ulong id)
        {
            if (EmulatedUsers.ContainsKey(id))
            {
                EmulatedUsers.Remove(id);
            }
        }

        public static void RunChecks(User user, DiscordChannel channel)
        {
            if (Utility.IsWebMaintenanceMode) channel.SendMessageAsync("**Maintenance mode is enabled**");

            if (user.DemoMode) channel.SendMessageAsync("**Account is a demo account**");

            if (user.Status == UserStatus.Suspended) channel.SendMessageAsync("**Account is disabled**");
        }

        private static void AddUserToCache(ulong id, User user)
        {
            if (!UserCache.ContainsKey(id))
            {
                UserCache.Add(id, user);
            }
        }

        private static void RemoveUserFromCache(ulong id)
        {
            if (UserCache.ContainsKey(id))
            {
                UserCache.Remove(id);
            }
        }
    }
}
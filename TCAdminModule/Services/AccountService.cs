using System;
using DSharpPlus.Entities;
using Nexus.SDK.Modules;
using Nexus.Utilities;
using TCAdminModule.Configurations;
using TCAdminModule.Helpers;

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
        public static readonly AccountServiceConfiguration AccountServiceConfiguration = new AccountServiceConfiguration().GetConfiguration();
        public static readonly Dictionary<ulong, User> EmulatedUsers = new Dictionary<ulong, User>();
        private static readonly Dictionary<ulong, User> UserCache = new Dictionary<ulong, User>();

        private static readonly LoginNotificationSettings LoginNotificationSettings =
            new NexusModuleConfiguration<LoginNotificationSettings>("LoginNotificationSettings",
                "./Config/TCAdminModule/Services/AccountService/").GetConfiguration();

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
            if (user == null)
            {
                user = User.GetAllUsers(2, true).FindByCustomField("OAUTH::Discord", id) as User;
            }

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
            var questionMsg = await ctx.RespondAsync(embed: EmbedTemplates.CreateInfoEmbed("Login",
                $"To proceed you will have to login to **{companyInfo.CompanyName}**. Would you like to login now? **Y/N**"));
            var signUpResponse = await interactivity.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id);
            if (signUpResponse.TimedOut)
            {
                await questionMsg.ModifyAsync(
                    embed: new Optional<DiscordEmbed>(EmbedTemplates.CreateErrorEmbed("Login", "No response")));
                return null;
            }

            switch (signUpResponse.Result.Content.ToLower())
            {
                case "y":
                case "yes":
                    var dmChannel = await ctx.Member.CreateDmChannelAsync();
                    if (await DiscordUtilities.SendMessageToDm(ctx, dmChannel,
                        "**Login**"))
                    {
                        await signUpResponse.Result.DeleteAsync();
                        await questionMsg.ModifyAsync(
                            embed: new Optional<DiscordEmbed>(
                                EmbedTemplates.CreateInfoEmbed("Login", "Please check your DM")));

                        var loginEmbed = new DiscordEmbedBuilder()
                        {
                            Color = new Optional<DiscordColor>(new DiscordColor(AccountServiceConfiguration.LoginConfiguration.EmbedColor)),
                            Title = $"**{companyInfo.CompanyName}** Login.",
                            Description = "Please follow the instructions below in order to authenticate yourself.\n" +
                                          $"**1)** Login to **{companyInfo.ControlPanelUrl}**\n" +
                                          $"**2)** Click **\"Game Services\"** and click on any service you own.\n" +
                                          $"**3)** Click **\"More\"** and click **\"Nexus Authentication\"**\n" +
                                          $"**4)** Copy the token back here to me.\n" +
                                          "*See the image for reference.*",
                            Url = companyInfo.ControlPanelUrl,
                            Timestamp = DateTimeOffset.Now,
                            ImageUrl =
                                AccountServiceConfiguration.LoginConfiguration.ImageUrl,
                        };
                        await dmChannel.SendMessageAsync(embed: loginEmbed);

                        var tokenResponse = await ctx.Client.GetInteractivity()
                            .WaitForMessageAsync(x => x.Author.Id == ctx.User.Id, TimeSpan.FromMinutes(5));
                        if (tokenResponse.TimedOut || string.IsNullOrEmpty(tokenResponse.Result.Content))
                        {
                            await ctx.RespondAsync(embed: EmbedTemplates.CreateInfoEmbed("Login",
                                "**Timeout**: To resume logging in, please do the `;login` command again. " +
                                ctx.Channel.Mention));
                            return null;
                        }

                        var user = User.GetAllUsers(2, true)
                            .FindByCustomField("__Nexus:DiscordToken", tokenResponse.Result.Content) as User;
                        if (user == null)
                        {
                            await dmChannel.SendMessageAsync(
                                embed: EmbedTemplates.CreateErrorEmbed("Login", "**Invalid Token Used**"));
                            return null;
                        }

                        RunChecks(user, dmChannel);
                        user.CustomFields["__Nexus:DiscordUserId"] = ctx.User.Id;
                        user.Save();

                        var mailConfig = new MailConfig();
                        var mailVariables = new Dictionary<string, object>()
                        {
                            {"CompanyInfo", new CompanyInfo(2)},
                            {"ThisUser", user},
                            {"Guild", ctx.Guild},
                            {"Member", ctx.Member},
                            {"DateTime", DateTime.UtcNow.ToString("F")}
                        };

                        var loginMessage = new MailMessage()
                        {
                            Subject = LoginNotificationSettings.Subject,
                            FromName = LoginNotificationSettings.FromName,
                            FromEmail = LoginNotificationSettings.FromEmail,
                            HtmlBody = LoginNotificationSettings.MessageContents.ReplaceWithVariables(mailVariables),
                            DontSendEmail = mailConfig.MailServer.Contains("localhost") || !LoginNotificationSettings.SendEmail,
                            ForceViewNotification = LoginNotificationSettings.ForceToViewOnPanel,
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
                    throw new CustomMessageException(EmbedTemplates.CreateErrorEmbed("Aborted logging process"));
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
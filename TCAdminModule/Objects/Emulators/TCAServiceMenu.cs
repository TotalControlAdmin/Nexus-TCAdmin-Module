using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Nexus.SDK.Modules;
using TCAdmin.Interfaces.Server;
using TCAdmin.SDK.Mail;
using TCAdmin.SDK.Objects;
using TCAdminModule.Attributes;
using TCAdminModule.Configurations;
using NexusServiceMenuModule = TCAdminModule.Modules.NexusServiceMenuModule;

namespace TCAdminModule.Objects.Emulators
{
    public class TcaServiceMenu
    {
        private readonly string[] _sizeSuffixes = {"bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};

        private NexusModuleConfiguration<ServiceMenuConfiguration> Configuration =
            new NexusModuleConfiguration<ServiceMenuConfiguration>("ServiceMenuConfig", "./Config/TCAdminModule/");

        public async Task MenuEmulation(CommandContext ctx, CommandAttributes.RequireAuthentication context)
        {
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            var config = Configuration.GetConfiguration();

            var ftpInfo = context.Service.FtpInfo.Split(':');
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = context.Service.Name,
                ThumbnailUrl = config.ThumbnailUrl,
                Footer =
                    new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Requested by {ctx.User.Username}#{ctx.User.Discriminator}",
                        IconUrl = ctx.User.AvatarUrl
                    },
                Timestamp = DateTime.Now,
                Url = new CompanyInfo(2).ControlPanelUrl +
                      "/Service/Home/" + context.Service.ServiceId,
                Description = $"**Name: **{context.Service.Name}\n"
                              + $"**Owner: **{context.Service.UserName}\n"
                              + $"**Server Info: **{context.Service.IpAddress} Port: {context.Service.GamePort}\n"
                              + $"**FTP: **{ftpInfo[0]} Port: {ftpInfo[1]}\n"
                              + $"{(context.User.UserType != UserType.User ? "**Server: **" + context.Service.ServerName : string.Empty)}\n"
            };

            var currentStatistics = config.ShowCpu ? $"**CPU:** {context.Service.CurrentCpu}%\n" : "";
            currentStatistics +=
                config.ShowMemory
                    ? $"**Memory:** {SizeSuffix(context.Service.CurrentMemory)}/{SizeSuffix(context.Service.MemoryLimitMB)} **[{context.Service.CurrentMemoryPercent}%]**\n"
                    : "";
            currentStatistics += config.ShowPlayers
                ? $"**Players:** {context.Service.CurrentPlayers}/{context.Service.CurrentMaxPlayers}"
                : "";

            if (!string.IsNullOrEmpty(currentStatistics))
            {
                embedBuilder.AddField("Stats", currentStatistics, true);
            }

            switch (context.Service.Status.ServiceStatus)
            {
                case ServiceStatus.Running:
                case ServiceStatus.Starting:
                case ServiceStatus.Resuming:
                    embedBuilder.Color = DiscordColor.Green;
                    break;
                case ServiceStatus.StartError:
                case ServiceStatus.Disabled:
                case ServiceStatus.Stopped:
                case ServiceStatus.Stopping:
                    embedBuilder.Color = DiscordColor.Red;
                    break;
                case ServiceStatus.Processing:
                case ServiceStatus.Pausing:
                case ServiceStatus.Paused:
                    embedBuilder.Color = DiscordColor.Orange;
                    break;
                case ServiceStatus.Unknown:
                    embedBuilder.Color = DiscordColor.DarkRed;
                    break;
                default:
                    embedBuilder.Color = DiscordColor.Gray;
                    break;
            }

            var modules = NexusServiceMenuModules.ToList();
            embedBuilder.AddField("Actions", GenerateActions(ctx.Client, modules));

            var menuMsg = await ctx.RespondAsync(embed: embedBuilder);

            foreach (NexusServiceMenuModule module in modules.OrderBy(x => x.Settings.ViewOrder))
            {
                module.CommandContext = ctx;
                module.Authentication = context;
                module.MenuMessage = menuMsg;
                await menuMsg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client,
                    module.Settings.ActionCommandAttribute.EmojiName));
                await Task.Delay(250);
            }

            while (true)
            {
                var reactAction = await menuMsg.WaitForReactionAsync(ctx.User, TimeSpan.FromMinutes(5));

                if (reactAction.TimedOut) return;
                if (reactAction.Result.User.Id != ctx.User.Id) continue;

                await ctx.TriggerTypingAsync();
                await menuMsg.DeleteReactionAsync(reactAction.Result.Emoji, ctx.User);

                var modulePressed = modules.FirstOrDefault(x =>
                    x.Settings.ActionCommandAttribute.EmojiName == reactAction.Result.Emoji.GetDiscordName());
                if (modulePressed != null)
                {
#pragma warning disable 4014
                    Task.Run(async () => { await modulePressed.DoAction(); });
#pragma warning restore 4014
                }
            }
        }

        private IEnumerable<NexusServiceMenuModule> NexusServiceMenuModules
        {
            get
            {
                IEnumerable<NexusServiceMenuModule> modules = typeof(NexusServiceMenuModule)
                    .Assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(NexusServiceMenuModule)) && !t.IsAbstract)
                    .Select(t => (NexusServiceMenuModule) Activator.CreateInstance(t));
                return modules;
            }
        }

        private string GenerateActions(DiscordClient client, IEnumerable<NexusServiceMenuModule> modules)
        {
            string actions = string.Empty;
            foreach (var module in modules.OrderBy(x => x.Settings.ViewOrder))
            {
                actions +=
                    $"**{DiscordEmoji.FromName(client, module.Settings.ActionCommandAttribute.EmojiName)} {module.Settings.ActionCommandAttribute.Description}**\n";
            }

            return actions;
        }

        private string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces));
            }

            if (value < 0)
            {
                return "-" + SizeSuffix(-value);
            }

            if (value == 0)
            {
                return string.Format("{0:n" + decimalPlaces + "} bytes", 0);
            }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            var mag = (int) Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            var adjustedSize = (decimal) value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) < 1000)
                return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, _sizeSuffixes[mag]);
            mag += 1;
            adjustedSize /= 1024;

            return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, _sizeSuffixes[mag]);
        }
    }
}
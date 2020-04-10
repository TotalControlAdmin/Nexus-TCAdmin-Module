using TCAdminModule.Helpers;

namespace TCAdminModule.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Interactivity;
    using TCAdmin.SDK.Objects;
    using Nexus.Exceptions;
    using Service = TCAdmin.GameHosting.SDK.Objects.Service;

    public static class DiscordService
    {
        public static async Task<Service> GetService(CommandContext ctx)
        {
            var guildServices = new List<Service>();
            
            var servicesFresh = Service.GetServices();

            foreach (Service service in servicesFresh)
                if (service.Variables["__Nexus::DiscordGuild"] != null &&
                    service.Variables["__Nexus::DiscordGuild"].ToString() ==
                    ctx.Guild.Id.ToString() /*&& ShowServiceVar(service)*/)
                {
                    guildServices.Add(service);
                }

            if (guildServices.Count == 0)
            {
                throw new CustomMessageException(EmbedTemplates.CreateErrorEmbed("Service Link",
                    "**No Services could be found. Add one by doing the `;Link` command!**"));
            }

            if (guildServices.Count == 1)
            {
                return guildServices[0];
            }

            if (guildServices.Count > 1)
            {
                return await ChooseServiceFromList(ctx, guildServices);
            }

            return null;
        }

        public static async Task<Service> LinkService(CommandContext ctx, User user)
        {
            if (user.IsSubUser)
            {
                throw new CustomMessageException(
                    "**Sorry!** Sub Users cannot yet link services to Discord. Please get the owner of the services to link them.");
            }

            var userServices = Service.GetServices(user, true);

            var services = new List<Service>();
            foreach (Service sv in userServices)
            {
                if (sv.UserId == user.UserId)
                {
                    services.Add(sv);
                }
            }

            var service = await ChooseServiceFromList(ctx, services.AsReadOnly());

            UpdateService(service, ctx.Guild.Id);

            return service;
        }

        public static bool LinkService(ulong guildId, int serviceId)
        {
            var service = new Service(serviceId);

            if (service.Find())
            {
                UpdateService(service, guildId);
                return true;
            }

            return false;
        }

        public static void ResetAllServices(CommandContext ctx)
        {
            var services = Service.GetServices();

            foreach (Service service in services)
                if (service.Variables["__Nexus::DiscordGuild"] != null)
                {
                    if (!ulong.TryParse(service.Variables["__Nexus::DiscordGuild"].ToString(), out var s)) continue;
                    if (s != ctx.Guild.Id) continue;

                    service.Variables["__Nexus::DiscordGuild"] = 0;
                    service.Save();
                }
        }

        private static async Task<Service> ChooseServiceFromList(CommandContext ctx, IReadOnlyList<Service> services)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var serviceId = 1;
            var servicesString = services.Aggregate(
                string.Empty,
                (current, service) =>
                    current + $"**{serviceId++}**) {service.Name} **({service.IpAddress}:{service.GamePort})**\n");

            if (servicesString.Length >= 1900)
            {
                await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User,
                    interactivity.GeneratePagesInContent(servicesString));
                await ctx.RespondAsync(
                    embed: EmbedTemplates.CreateInfoEmbed("Tip!",
                        "When you have found the number of the server press the **STOP** Button then type the ID number."));
            }
            else
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateInfoEmbed("Service Picker", servicesString));
            }

            var serviceOption =
                await interactivity.WaitForMessageAsync(x =>
                    x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

            if (serviceOption.TimedOut)
            {
                throw new CustomMessageException(EmbedTemplates.CreateInfoEmbed("Timeout", ""));
            }

            if (int.TryParse(serviceOption.Result.Content, out var result) && result <= serviceId && result > 0)
            {
                return services[result - 1];
            }

            throw new CustomMessageException(EmbedTemplates.CreateErrorEmbed(description: "Not a number!"));
        }

        private static void UpdateService(Service service, ulong id)
        {
            service.Variables["__Nexus::DiscordGuild"] = id;
            service.Variables["NexusShowOnDiscord"] = true;
            service.Save();
        }
    }
}
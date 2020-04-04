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
        private static readonly Dictionary<ulong, List<Service>> CacheServices = new Dictionary<ulong, List<Service>>();

        public static async Task<Service> GetService(CommandContext ctx)
        {
            var guildServices = new List<Service>();
            var servicesInCache = false;

            if (CacheServices.TryGetValue(ctx.Guild.Id, out var services))
            {
                foreach (var service in services)
                    if (service.Variables["__Nexus::DiscordGuild"] != null && service.Variables["__Nexus::DiscordGuild"].ToString() == ctx.Guild.Id.ToString())
                    {
                        guildServices.Add(service);
                    }

                servicesInCache = true;
            }

            if (!servicesInCache)
            {
                var servicesFresh = Service.GetServices();

                foreach (Service service in servicesFresh)
                    if (service.Variables["__Nexus::DiscordGuild"] != null && service.Variables["__Nexus::DiscordGuild"].ToString() == ctx.Guild.Id.ToString() /*&& ShowServiceVar(service)*/)
                    {
                        guildServices.Add(service);
                    }
            }

            if (guildServices.Count == 0)
            {
                throw new CustomMessageException("**No Services could be found. Add one by doing the `;Link` command!**");
            }

            if (guildServices.Count == 1)
            {
                AddServiceToCache(ctx.Guild.Id, guildServices[0]);
                return guildServices[0];
            }

            if (guildServices.Count > 1)
            {
                AddServicesToCache(ctx.Guild.Id, guildServices);
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
                AddServiceToCache(guildId, new Service(serviceId));
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
                    
                    RemoveServiceFromCache(ctx.Guild.Id, service);
                    service.Variables["__Nexus::DiscordGuild"] = 0;
                    service.Save();
                }
        }

        private static void AddServicesToCache(ulong id, List<Service> services)
        {
            if (!CacheServices.ContainsKey(id))
            {
                CacheServices.Add(id, services);
            }
        }

        private static void AddServiceToCache(ulong id, Service service)
        {
            if (CacheServices.ContainsKey(id))
            {
                CacheServices.FirstOrDefault(x => x.Key == id).Value.Add(service);
            }
        }

        private static async Task<Service> ChooseServiceFromList(CommandContext ctx, IReadOnlyList<Service> services)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var serviceId = 1;
            var servicesString = services.Aggregate(
                string.Empty,
                (current, service) => current + $"**{serviceId++}**) {service.Name} **({service.IpAddress}:{service.GamePort})**\n");

            if (servicesString.Length >= 1900)
            {
                await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, interactivity.GeneratePagesInContent(servicesString));
                await ctx.RespondAsync("When you have found the number of the server press the **STOP** Button then type the Number.");
            }
            else
            {
                await ctx.RespondAsync("**Choose a Server**\n" + servicesString);
            }

            var serviceOption = await interactivity.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

            if (serviceOption.TimedOut)
            {
                throw new CustomMessageException("No response from " + ctx.User.Mention);
            }

            if (int.TryParse(serviceOption.Result.Content, out var result) && result <= serviceId && result > 0)
            {
                return services[result - 1];
            }

            await ctx.RespondAsync("**Not a valid number!**");

            return null;
        }

        private static void RemoveServiceFromCache(ulong id, Service service)
        {
            if (CacheServices.ContainsKey(id))
            {
                var services = CacheServices.FirstOrDefault(x => x.Key == id).Value;
                services.RemoveAll(x => x.ServiceId == service.ServiceId);
                CacheServices.Remove(id);
                CacheServices.Add(id, services);
            }
        }

        private static void RemoveServicesFromCache(ulong id)
        {
            if (CacheServices.ContainsKey(id))
            {
                CacheServices.Remove(id);
            }
        }

        private static bool ShowServiceVar(Service service)
        {
            if (service.Variables["NexusShowOnDiscord"] == null)
            {
                service.Variables["NexusShowOnDiscord"] = true;
                service.Save();

                return true;
            }

            if (bool.TryParse(service.Variables["NexusShowOnDiscord"].ToString(), out var result))
            {
                return result;
            }

            return false;
        }

        private static void UpdateService(Service service, ulong id)
        {
            service.Variables["__Nexus::DiscordGuild"] = id;
            service.Variables["NexusShowOnDiscord"] = true;
            service.Save();
        }

        private static bool GetService(string billingId, out Service serviceOut)
        {
            if (Service.GetServicesByBillingId(billingId)[0] is Service service)
            {
                serviceOut = service;
                return true;
            }

            serviceOut = null;
            return false;
        }
    }
}
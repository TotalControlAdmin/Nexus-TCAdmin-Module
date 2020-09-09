using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Nexus;
using Nexus.SDK.Modules;
using Quartz;
using TCAdmin.SDK.Objects;
using TCAdminModule.Configurations;
using Server = TCAdmin.GameHosting.SDK.Objects.Server;

namespace TCAdminModule.Crons
{
    public class NetworkStatusCron : NexusScheduledTaskModule
    {
        private readonly Logger _logger = new Logger("NetworkStatusCron");

        private readonly NetworkStatusCronSettings _settings =
            new NexusModuleConfiguration<NetworkStatusCronSettings>("NetworkStatusSettings",
                "./Config/TCAdminModule/Crons/").GetConfiguration();

        private DiscordClient _client;

        private DiscordChannel _privateStatusChannel;

        private DiscordChannel _publicStatusChannel;

        private Dictionary<Datacenter, List<Server>> _serversDown;

        public NetworkStatusCron()
        {
            Name = "Network Status";
            RepeatEveryMilliseconds = 120000;
        }

        public override async Task DoAction(IJobExecutionContext context)
        {
            if (!_settings.Enabled)
            {
                _logger.LogMessage("NetworkStatus is disabled. Skipping cron.");
                return;
            }

            _client = (DiscordClient) context.Scheduler.Context.Get("Client");
            _publicStatusChannel = await _client.GetChannelAsync(_settings.PublicStatusChannelId);
            _privateStatusChannel = await _client.GetChannelAsync(_settings.PrivateStatusChannelId);
            _serversDown = new Dictionary<Datacenter, List<Server>>();

            var publicStatusMessage = await GetPublicStatusMessage();
            var privateStatusMessage = await GetPrivateStatusMessage();

            try
            {
                await publicStatusMessage.ModifyAsync("", GeneratePublicStatuses());
                await privateStatusMessage.ModifyAsync("", GeneratePrivateStatuses(GetLocationsDictionary()));
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                Console.Write(e.StackTrace);
            }
        }

        private DiscordEmbed GeneratePrivateStatuses(Dictionary<Datacenter, List<Server>> locations)
        {
            DiscordEmbedBuilder embed;
            embed = new DiscordEmbedBuilder
            {
                Title = "Network Status",
                Description = $"**Network Status Across *{locations.Count}*  Locations**",
                Timestamp = DateTimeOffset.Now
            };

            foreach (var location in locations.OrderBy(x => x.Key.Location))
            {
                if (location.Value.Count == 0) continue;

                var serversOperationalStatus = string.Empty;
                foreach (var server in location.Value)
                {
                    var task = Task.Run(() => GetOperationalString(server));
                    if (task.Wait(TimeSpan.FromSeconds(2)))
                    {
                        serversOperationalStatus += task.Result;
                    }
                    else
                    {
                        AddToOrCreateDictionary(location.Key, server);
                        serversOperationalStatus +=
                            $"***{server.Name}*** **[{server.PrimaryIp}] (Stock: {(server.NumberOfServices >= server.MaxServices ? "Not Available" : "Available")})** | **Unknown**\n";
                    }
                }

                embed.AddField(location.Key.Location, serversOperationalStatus);
            }

            embed.Color = _serversDown.Count == 0 ? DiscordColor.Green : DiscordColor.Red;

            return embed;
        }

        private string GetOperationalString(Server server)
        {
            var serversOperationalStatus = string.Empty;
            try
            {
                serversOperationalStatus +=
                    $"***{server.Name}*** **[{server.PrimaryIp}] (Stock: {(server.NumberOfServices >= server.MaxServices ? "Not Available" : "Available")})** | **{(server.MonitorVersion != null ? "Operational!" : "Down!")}**\n";
            }
            catch
            {
                AddToOrCreateDictionary(new Datacenter(server.DatacenterId), server);
                serversOperationalStatus +=
                    $"***{server.Name}*** **[{server.PrimaryIp}] (Stock: {(server.NumberOfServices >= server.MaxServices ? "Not Available" : "Available")})** | **Unknown**\n";
            }

            return serversOperationalStatus;
        }

        private async Task<DiscordMessage> GetPrivateStatusMessage()
        {
            var pinnedMessages = await _privateStatusChannel.GetPinnedMessagesAsync();

            if (pinnedMessages.Count > 0) return pinnedMessages[0];

            var statusMessage = await _privateStatusChannel.SendMessageAsync("**Placeholder**");
            await statusMessage.PinAsync();

            return statusMessage;
        }

        private async Task<DiscordMessage> GetPublicStatusMessage()
        {
            var pinnedMessages = await _publicStatusChannel.GetPinnedMessagesAsync();

            if (pinnedMessages.Count > 0) return pinnedMessages[0];

            var statusMessage = await _publicStatusChannel.SendMessageAsync("**Placeholder**");
            await statusMessage.PinAsync();
            return statusMessage;
        }

        private static Dictionary<Datacenter, List<Server>> GetLocationsDictionary()
        {
            var dictionary = new Dictionary<Datacenter, List<Server>>();
            foreach (Datacenter dc in Datacenter.GetDatacenters())
            {
                if (dc.DatacenterId == 27 || dc.DatacenterId == 9) continue;

                var servers = new List<Server>();

                foreach (Server server in Server.GetServersFromDatacenter(dc.DatacenterId)) servers.Add(server);

                dictionary.Add(dc, servers);
            }

            return dictionary;
        }

        private DiscordEmbed GeneratePublicStatuses()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Network Status",
                Description = "**All Systems Are Operational!**",
                Color = DiscordColor.Green,
                Timestamp = DateTime.Now
            };

            if (_serversDown.Count == 0) return embed;

            embed.Description =
                "**There are outages!\nCheck below for a list of the IP addresses that are affected. We are working to get back up as soon as possible!**";
            embed.Color = DiscordColor.Red;

            foreach (var location in _serversDown)
            {
                var operationalStatus = string.Empty;
                if (location.Value.Count == 0) continue;

                foreach (var server in location.Value)
                {
                    var ips = string.Empty;
                    foreach (ServerIp ip in server.ServerIps)
                    {
                        if (!ip.IsPublic) continue;
                        ips += $" - {ip.IpAddress}\n";
                    }

                    operationalStatus += $"**Affected IPs:\n{ips}\n\n**";
                }

                embed.AddField(location.Key.Location, operationalStatus, true);
            }

            return embed;
        }

        private void AddToOrCreateDictionary(Datacenter dc, Server server)
        {
            if (_serversDown.TryGetValue(dc, out var servers))
            {
                servers.Add(server);
                return;
            }

            _serversDown.Add(dc, new List<Server> {server});
        }
    }
}
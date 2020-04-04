using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Nexus.SDK.Modules;
using TCAdmin.GameHosting.SDK.GameMonitor;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdminModule.Services;

namespace TCAdminModule.Commands.Client
{
    public class QueryCommands : NexusCommandModule
    {
        [Command("Help")]
        [Description("Displays the help embed")]
        public Task HelpTask(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
                {
                    Title = "Commands",
                    Color = DiscordColor.Green,
                    Description = "Shows Basic Commands"
                }
                .AddField(";Players", "Show all players on the server.", true)
                .AddField(";Service", "Show the Service Interface.", true)
                .AddField(";Link", "Link a Service to Discord.", true);

            return ctx.RespondAsync(embed: embed);
        }

        [Command("logout")]
        [Aliases("signout")]
        public async Task SignOut(CommandContext ctx)
        {
            var user = AccountsService.GetUser(ctx.User.Id);
            if (user == null)
            {
                await ctx.RespondAsync("**You have to be signed in, in order to sign out.**");
                return;
            }
            
            AccountsService.LogoutUser(user, ctx.User.Id);
            await ctx.RespondAsync("**You have been signed out**");
        }
        
        [Command("login")]
        [Aliases("signin")]
        public async Task Signin(CommandContext ctx)
        {
            var user = AccountsService.GetUser(ctx.User.Id);
            if (user == null)
            {
                await AccountsService.SetupAccount(ctx);
                return;
            }
            
            await ctx.RespondAsync("**You are already signed in**");
        }

        [Command("Players")]
        [Description("View Players on the server.")]
        [Cooldown(1, 15.0, CooldownBucketType.User)]
        public async Task PlayersTask(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var service = await DiscordService.GetService(ctx);
            var server = new Server(service.ServerId);

            var query = ServerQuery.GetQueryResults(server, new TCAdmin.GameHosting.SDK.Objects.Game(service.GameId),
                service);

            if (query.NumPlayers == 0)
            {
                await ctx.RespondAsync($"**No players online on {service.Name}**");
                return;
            }

            if (query.NumPlayers != query.Players.Count)
            {
                await ctx.RespondAsync($"There are **{query.NumPlayers}/{query.MaxPlayers}** online!");
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{service.Name} | Players: {query.NumPlayers}/{query.MaxPlayers}",
                Color = DiscordColor.Blue,
                Timestamp = DateTime.Now,
                ThumbnailUrl = "https://cdn.alexr03.com/images/controller-clipart-simple-3.png"
            };

            if (query.NumPlayers <= 24)
            {
                foreach (var player in query.Players.OrderBy(x => x.Name))
                    embed.AddField(player.Name, "Score: " + player.Score, true);
            }
            else
            {
                foreach (var player in query.Players)
                    embed.Description += $"**{player.Name}** | Score: {player.Score}\n";
            }

            await ctx.RespondAsync(embed: embed);
        }
    }
}
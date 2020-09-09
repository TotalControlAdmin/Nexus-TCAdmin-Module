using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Nexus.SDK.Modules;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.SDK.Web.FileManager;
using TCAdminModule.Attributes;
using TCAdminModule.Helpers;

namespace TCAdminModule.Commands.Admin
{
    [Group("Node")]
    [Description("Node Commands")]
    [CommandAttributes.RequireTcAdministrator]
    public class NodeCommands : NexusCommandModule
    {
        [Command("CMD")]
        public async Task ServerCmd(CommandContext ctx, string serverName, [RemainingText] string script)
        {
            await ctx.TriggerTypingAsync();

            if (!(Server.GetServers(true, serverName)[0] is Server server))
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed("Server Not Found",
                    $"{serverName} could not be found."));
                return;
            }

            var console = new RemoteConsole(server, @"C:\\Windows\\System32\\cmd.exe", "/c " + script,
                "Command Prompt - Script", true);

            await ctx.RespondAsync(embed: EmbedTemplates.CreateInfoEmbed("Live Output",
                $"To start the command, click [here]({console.GetUrl()})"));
        }

        [Command("PowerShell")]
        public async Task ServerPowerShell(CommandContext ctx, string serverName, [RemainingText] string script)
        {
            await ctx.TriggerTypingAsync();
            if (!script.StartsWith("```\n") && !script.EndsWith("```"))
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed("PowerShell",
                    "**No script found!** Example Script: ```\necho 'test'\n```"));
                return;
            }

            if (!(Server.GetServers(true, serverName)[0] is Server server))
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed("Server Not Found",
                    $"{serverName} could not be found."));
                return;
            }

            server.FileSystemService.CreateTextFile(@"C:\\DiscordScripts\\script.ps1",
                Encoding.ASCII.GetBytes(script.Replace("```\n", string.Empty).Replace("```", string.Empty)));
            var console = new RemoteConsole(server, @"C:\\Windows\\System32\\cmd.exe",
                "/c powershell \"C:\\DiscordScripts\\script.ps1\"", "Powershell - Script", true);

            await ctx.RespondAsync(embed: EmbedTemplates.CreateInfoEmbed("Live Output",
                $"To start the command, click [here]({console.GetUrl()})"));
        }

        [Command("RunAllPowerShell")]
        public async Task RunAllPowerShell(CommandContext ctx, [RemainingText] string script)
        {
            if (!script.StartsWith("```\n") && !script.EndsWith("```"))
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed("PowerShell",
                    "**No script found!** Example Script: ```\necho 'test'\n```"));
                return;
            }

            foreach (Server enabledServer in Server.GetEnabledServers())
            {
                if (enabledServer.IsMaster) continue;

                await ServerPowerShell(ctx, enabledServer.Name, script);
            }
        }

        [Command("RestartMonitor")]
        public async Task RestartMonitor(CommandContext ctx, string serverName)
        {
            if (!(Server.GetServers(true, serverName)[0] is Server server))
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed("Server Not Found",
                    $"{serverName} could not be found."));
                return;
            }

            server.ServerUtilitiesService.RestartMonitor();
            await ctx.RespondAsync(embed: EmbedTemplates.CreateSuccessEmbed("Monitor Restart",
                $"{server.Name}'s monitor is restarting."));
        }

        [Command("RestartAllMonitors")]
        public async Task RestartMonitor(CommandContext ctx)
        {
            foreach (Server enabledServer in Server.GetEnabledServers())
            {
                if (enabledServer.IsMaster) continue;

                await RestartMonitor(ctx, enabledServer.Name);
            }
        }

        // [Command("FileSystem")]
        // public async Task FileSystem(CommandContext ctx, string serverName)
        // {
        //     await ctx.TriggerTypingAsync();
        //     var server = Server.GetServers(true, serverName)[0] as Server;
        //
        //     if (IsUnauthedDruConnection(server, ctx.User))
        //     {
        //         await ctx.RespondAsync("No.");
        //         return;
        //     }
        //
        //     var fileManager = new TcaFileManager(ctx, server, "C:\\");
        //     fileManager.DirectoryChange += async (sender, args) => { await ctx.RespondAsync(args.CurrentDirectory); };
        // }
    }
}
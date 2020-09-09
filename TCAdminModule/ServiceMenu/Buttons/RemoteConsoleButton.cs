using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Interactivity;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.Interfaces.Server;
using TCAdminModule.Helpers;
using TCAdminModule.Modules;
using TCAdminModule.Objects;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class RemoteConsoleButton : NexusServiceMenuModule
    {
        public override void DefaultSettings()
        {
            Name = "Remote Console Button";
            var attribute =
                new ActionCommandAttribute("Remote Console", "Access the servers Remote Console", ":desktop:",
                    new List<string> {"StartStop"});
            Settings.ViewOrder = 7;
            Settings.ActionCommandAttribute = attribute;
            Configuration.SetConfiguration(Settings);
        }

        public override async Task DoAction()
        {
            await base.DoAction();
            await CommandContext.TriggerTypingAsync();

            await RconTask();
        }

        public async Task RconTask()
        {
            var interactivity = CommandContext.Client.GetInteractivity();
            await CommandContext.RespondAsync(embed: EmbedTemplates.CreateInfoEmbed("Remote Console",
                "Please enter the RCON command to send to the server."));

            var command = await interactivity.WaitForMessageAsync(
                x => x.Author.Id == CommandContext.User.Id && x.Channel.Id == CommandContext.Channel.Id);
            await RconTask(command.Result.Content);
        }

        public async Task RconTask(string command)
        {
            await CommandContext.TriggerTypingAsync();

            if (Authentication.Service.Status.ServiceStatus != ServiceStatus.Running)
            {
                await CommandContext.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed("Remote Console",
                    $"{Authentication.Service.NameNoHtml} is **Offline**"));
                return;
            }

            var server = new Server(Authentication.Service.ServerId);

            var rconResponse = server.GameAdminService.ExecuteRConCommand(Authentication.Service.ServiceId,
                Authentication.Service.Variables["RConPassword"].ToString(), command);

            rconResponse = rconResponse.Length > 1500 ? rconResponse.Substring(0, 1500) : rconResponse;
            await CommandContext.RespondAsync($"```yaml\n{rconResponse}\n```");
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using TCAdminModule.Helpers;
using TCAdminModule.Modules;
using TCAdminModule.Objects;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class StopButton : NexusServiceMenuModule
    {
        public override void DefaultSettings()
        {
            Name = "Stop Button";
            var attribute =
                new ActionCommandAttribute("Stop", "Stop Server", ":stop_button:", new List<string> {"StartStop"});
            Settings.ViewOrder = 2;
            Settings.ActionCommandAttribute = attribute;

            Configuration.SetConfiguration(Settings);
        }

        public override async Task DoAction()
        {
            await base.DoAction();
            var service = Authentication.Service;
            service.Stop("Stopped by Nexus.");
            // await CommandContext.RespondAsync($"**{service.NameNoHtml} has been stopped**");

            var embed = EmbedTemplates.CreateSuccessEmbed($"{service.NameNoHtml}", "**Stopped successfully**");
            await CommandContext.RespondAsync(embed: embed);
        }
    }
}
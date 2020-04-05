using System.Collections.Generic;
using System.Threading.Tasks;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdminModule.Models;
using TCAdminModule.Modules;
using TCAdminModule.Objects;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class StopButton : NexusServiceMenuModule
    {
        public override void DefaultSettings()
        {
            this.Name = "Stop Button";
            var attribute =
                new ActionCommandAttribute("Stop", "Stop Server", ":stop_button:", new List<string> {"StartStop"});
            this.Settings.ViewOrder = 2;
            this.Settings.ActionCommandAttribute = attribute;

            this.Configuration.SetConfiguration(this.Settings);
        }

        public override async Task DoAction()
        {
            await base.DoAction();
            Service service = this.Authentication.Service;
            service.Stop("Started by Nexus.");
            await CommandContext.RespondAsync($"**{service.NameNoHtml} has been stopped**");
        }
    }
}
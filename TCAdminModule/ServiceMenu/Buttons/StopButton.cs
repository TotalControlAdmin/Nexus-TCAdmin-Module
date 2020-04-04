using System.Collections.Generic;
using System.Threading.Tasks;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdminModule.Modules;
using TCAdminModule.Objects;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class StopButton : NexusServiceMenuModule
    {
        public StopButton()
        {
            this.Name = "Stop Button";
            var attribute =
                new ActionCommandAttribute("Stop", "Stop Server", ":stop_button:", new List<string> {"StartStop"});
            this.ViewOrder = 2;
            this.ActionCommandAttribute = attribute;
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
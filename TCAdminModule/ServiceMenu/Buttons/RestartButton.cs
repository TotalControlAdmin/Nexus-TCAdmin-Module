using System.Collections.Generic;
using System.Threading.Tasks;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdminModule.Helpers;
using TCAdminModule.Modules;
using TCAdminModule.Objects;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class RestartButton : NexusServiceMenuModule
    {
        public override void DefaultSettings()
        {
            this.Name = "Restart Button";
            var attribute =
                new ActionCommandAttribute("Restart", "Restart Server", ":arrows_counterclockwise:", new List<string> {"StartStop"},
                    false);
            this.Settings.ViewOrder = 2;
            this.Settings.ActionCommandAttribute = attribute;

            this.Configuration.SetConfiguration(this.Settings);
        }

        public override async Task DoAction()
        {
            await base.DoAction();
            Service service = this.Authentication.Service;
            service.Restart("Restarted by Nexus.");
            var embed = EmbedTemplates.CreateSuccessEmbed($"{service.NameNoHtml}", "**Restarted successfully**");
            await CommandContext.RespondAsync(embed: embed);
        }
    }
}
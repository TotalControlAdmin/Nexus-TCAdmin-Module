using System.Collections.Generic;
using System.Threading.Tasks;
using TCAdminModule.Helpers;
using TCAdminModule.Modules;
using TCAdminModule.Objects;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class RestartButton : NexusServiceMenuModule
    {
        public override void DefaultSettings()
        {
            Name = "Restart Button";
            var attribute =
                new ActionCommandAttribute("Restart", "Restart Server", ":arrows_counterclockwise:",
                    new List<string> {"StartStop"});
            Settings.ViewOrder = 2;
            Settings.ActionCommandAttribute = attribute;

            Configuration.SetConfiguration(Settings);
        }

        public override async Task DoAction()
        {
            await base.DoAction();
            var service = Authentication.Service;
            service.Restart("Restarted by Nexus.");
            var embed = EmbedTemplates.CreateSuccessEmbed($"{service.NameNoHtml}", "**Restarted successfully**");
            await CommandContext.RespondAsync(embed: embed);
        }
    }
}
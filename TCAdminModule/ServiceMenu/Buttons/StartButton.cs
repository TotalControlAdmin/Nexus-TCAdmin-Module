using System.Collections.Generic;
using System.Threading.Tasks;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdminModule.Helpers;
using TCAdminModule.Modules;
using TCAdminModule.Objects;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class StartButton : NexusServiceMenuModule
    {
        public override void DefaultSettings()
        {
            this.Name = "Start Button";
            var attribute =
                new ActionCommandAttribute("Start", "Start Server", ":arrow_forward:",
                    new List<string> {"StartStop"},
                    false);
            this.Settings.ViewOrder = 1;
            this.Settings.ActionCommandAttribute = attribute;

            this.Configuration.SetConfiguration(this.Settings);
        }

        public override async Task DoAction()
        {
            await base.DoAction();
            Service service = this.Authentication.Service;
            service.Start("Started by Nexus.");
            var embed = EmbedTemplates.CreateSuccessEmbed($"{service.NameNoHtml}", "**Started successfully**");
            await CommandContext.RespondAsync(embed: embed);
        }
    }
}
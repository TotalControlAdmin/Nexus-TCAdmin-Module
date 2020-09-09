using System.Collections.Generic;
using TCAdminModule.Modules;
using TCAdminModule.Objects;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class ExitModule : NexusServiceMenuModule
    {
        public override void DefaultSettings()
        {
            Name = "Exit";
            var attribute =
                new ActionCommandAttribute("Exit", "Exit", ":octagonal_sign:",
                    new List<string> {string.Empty},
                    true);
            Settings.ViewOrder = 0;
            Settings.ActionCommandAttribute = attribute;

            Configuration.SetConfiguration(Settings);
        }
    }
}
using System.Collections.Generic;
using TCAdminModule.Modules;
using TCAdminModule.Objects;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class ExitModule : NexusServiceMenuModule
    {
        public ExitModule()
        {
            this.Name = "Exit";
            var attribute =
                new ActionCommandAttribute("Exit", "Exit", ":octagonal_sign:", new List<string> {string.Empty},
                    true);
            this.ViewOrder = 0;
            this.ActionCommandAttribute = attribute;
        }
    }
}
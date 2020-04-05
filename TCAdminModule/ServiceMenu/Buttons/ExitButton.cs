using System.Collections.Generic;
using TCAdminModule.Modules;
using TCAdminModule.Objects;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class ExitModule : NexusServiceMenuModule
    {
        public ExitModule()
        {
            
        }
        
        

        public override void DefaultSettings()
        {
            this.Name = "Exit";
            var attribute =
                new ActionCommandAttribute("Exit", "Exit", ":octagonal_sign:",
                    new List<string> {string.Empty},
                    true);
            this.Settings.ViewOrder = 0;
            this.Settings.ActionCommandAttribute = attribute;

            this.Configuration.SetConfiguration(this.Settings);
        }
    }
}
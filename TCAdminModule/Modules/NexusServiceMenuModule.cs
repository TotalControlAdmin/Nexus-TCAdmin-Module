using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Nexus.SDK.Modules;
using TCAdminModule.Attributes;
using TCAdminModule.Objects;

namespace TCAdminModule.Modules
{
    public class NexusServiceMenuModule : NexusModule
    {
        public DiscordMessage MenuMessage { get; set; }
        
        public CommandContext CommandContext { get; set; }
        
        public ActionCommandAttribute ActionCommandAttribute { get; set; }
        
        public CommandAttributes.RequireAuthentication Authentication { get; internal set; }

        public int ViewOrder;
        
        public NexusServiceMenuModule()
        {
            
        }

        public NexusServiceMenuModule(string name, ActionCommandAttribute actionCommandAttribute)
        {
            this.Name = name;
            this.ActionCommandAttribute = actionCommandAttribute;
        }

        /// <summary>
        /// This is fired when the user clicks on the emoji.
        /// </summary>
        public virtual async Task DoAction()
        {
            if (this.ActionCommandAttribute.DeleteMenu)
            {
                await this.MenuMessage.DeleteAsync();
            }
        }
    }
}
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Nexus.SDK.Modules;
using TCAdminModule.Attributes;
using TCAdminModule.Models;

namespace TCAdminModule.Modules
{
    public class NexusServiceMenuModule : NexusModule
    {
        [JsonIgnore] public CommandAttributes.RequireAuthentication Authentication { get; internal set; }

        [JsonIgnore] public NexusModuleConfiguration<ServiceMenuActionSettings> Configuration { get; }

        [JsonIgnore] public DiscordMessage MenuMessage { get; set; }

        [JsonIgnore] public CommandContext CommandContext { get; set; }

        public ServiceMenuActionSettings Settings = new ServiceMenuActionSettings();

        public NexusServiceMenuModule()
        {
            Configuration = new NexusModuleConfiguration<ServiceMenuActionSettings>(this.GetType().Name,
                "./Config/TCAdminModule/ServiceMenuButtons/");
            var config = Configuration.GetConfiguration(false);

            if (config != null)
            {
                this.Settings.ActionCommandAttribute = config.ActionCommandAttribute;
                this.Settings.ViewOrder = config.ViewOrder;
            }
            else
            {
                DefaultSettings();
            }
        }

        public virtual void DefaultSettings()
        {
        }

        /// <summary>
        /// This is fired when the user clicks on the emoji.
        /// </summary>
        public virtual async Task DoAction()
        {
            if (this.Settings.ActionCommandAttribute.DeleteMenu)
            {
                await this.MenuMessage.DeleteAsync();
            }
        }
    }
}
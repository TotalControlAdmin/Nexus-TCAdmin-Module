using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Nexus.SDK.Modules;
using TCAdminModule.Attributes;
using TCAdminModule.Configurations;

namespace TCAdminModule.Modules
{
    public class NexusServiceMenuModule : NexusModule
    {
        public readonly ServiceMenuActionSettings Settings = new ServiceMenuActionSettings();

        public NexusServiceMenuModule()
        {
            Configuration = new NexusModuleConfiguration<ServiceMenuActionSettings>(GetType().Name,
                "./Config/TCAdminModule/ServiceMenuButtons/");
            var config = Configuration.GetConfiguration(false);

            if (config != null)
            {
                Settings.ActionCommandAttribute = config.ActionCommandAttribute;
                Settings.ViewOrder = config.ViewOrder;
            }
            else
            {
                DefaultSettings();
            }
        }

        public CommandAttributes.RequireAuthentication Authentication { get; internal set; }

        public NexusModuleConfiguration<ServiceMenuActionSettings> Configuration { get; }

        public DiscordMessage MenuMessage { get; set; }

        public CommandContext CommandContext { get; set; }

        public virtual void DefaultSettings()
        {
        }

        /// <summary>
        ///     This is fired when the user clicks on the emoji.
        /// </summary>
        public virtual async Task DoAction()
        {
            if (Settings.ActionCommandAttribute.DeleteMenu) await MenuMessage.DeleteAsync();
        }
    }
}
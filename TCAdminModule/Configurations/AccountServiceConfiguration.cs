using Nexus.SDK.Modules;

namespace TCAdminModule.Configurations
{
    public class AccountServiceConfiguration : NexusModuleConfiguration<AccountServiceConfiguration>
    {
        public AccountServiceConfiguration() : base("AccountServiceConfiguration", "./Config/TCAdminModule/")
        {
        }

        public LoginConfiguration LoginConfiguration { get; set; } = new LoginConfiguration();
    }
}
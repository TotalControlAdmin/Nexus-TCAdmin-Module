using DSharpPlus.Entities;
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

    public class LoginConfiguration
    {
        public int EmbedColor { get; set; } = DiscordColor.Green.Value;

        public string ImageUrl { get; set; } =
            "https://cdn.discordapp.com/attachments/612133944695193619/696486508760268870/0673e9e0-4179-4e87-be57-357199ff5e01.gif";
    }
}
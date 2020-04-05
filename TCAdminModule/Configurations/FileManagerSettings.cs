using DSharpPlus.Entities;

namespace TCAdminModule.Configurations
{
    public class FileManagerSettings
    {
        public string ThumbnailUrl { get; set; } = "";

        public DiscordColor Color { get; set; } = DiscordColor.Azure;

        public string[] ExitCommand { get; set; } = {"exit", "quit", "q"};
        
        public string[] GoBackCommand { get; set; } = {"..", "..."};
    }
}
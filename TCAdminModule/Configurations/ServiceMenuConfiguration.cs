using DSharpPlus.Entities;

namespace TCAdminModule.Configurations
{
    public class ServiceMenuConfiguration
    {
        public string ThumbnailUrl { get; set; }

        public bool ShowCpu { get; set; } = true;
        
        public bool ShowMemory { get; set; } = true;
        
        public bool ShowPlayers { get; set; } = true;
    }
}
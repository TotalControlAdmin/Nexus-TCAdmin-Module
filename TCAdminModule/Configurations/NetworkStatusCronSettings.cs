namespace TCAdminModule.Configurations
{
    public class NetworkStatusCronSettings
    {
        public bool Enabled { get; set; } = false;
        
        public ulong PublicStatusChannelId { get; set; } = new ulong();
        
        public ulong PrivateStatusChannelId { get; set; } = new ulong();
    }
}
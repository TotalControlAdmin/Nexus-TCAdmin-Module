using Nexus.SDK.Modules;

namespace TCAdminModule
{
    public class TcAdminModuleConfig : NexusModuleConfiguration<TcAdminModule>
    {
        public string SqlString { get; set; }
        public bool SqlEncrypted { get; set; }
        
        public bool DebugTcAdmin { get; set; }
        
        public bool DebugTcAdminSql { get; set; }

        public TcAdminModuleConfig()
        {
            this.SqlString = "";
            this.SqlEncrypted = false;
            this.DebugTcAdmin = false;
            this.DebugTcAdminSql = false;
        }
    }
}
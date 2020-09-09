using Nexus.SDK.Modules;

namespace TCAdminModule
{
    public class TcAdminModuleConfig : NexusModuleConfiguration<TcAdminModule>
    {
        public TcAdminModuleConfig()
        {
            SqlString = "";
            SqlEncrypted = false;
            DebugTcAdmin = false;
            DebugTcAdminSql = false;
        }

        public string SqlString { get; set; }
        public bool SqlEncrypted { get; set; }

        public bool DebugTcAdmin { get; set; }

        public bool DebugTcAdminSql { get; set; }
    }
}
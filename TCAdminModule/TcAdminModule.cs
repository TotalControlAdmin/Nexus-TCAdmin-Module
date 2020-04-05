using System.Configuration;
using System.IO;
using Nexus.SDK.Modules;
using TCAdminWrapper;

namespace TCAdminModule
{
    public class TcAdminModule : NexusAssemblyModule
    {
        public TcAdminModule()
        {
            this.Name = "TCAdmin Module";
            TCAdmin.SDK.Utility.AppSettings = ConfigurationManager.AppSettings;
            RegisterToTcAdmin();
            DeleteTcaFilesNexusFolder();
        }

        private void RegisterToTcAdmin()
        {
            var moduleConfig = new NexusModuleConfiguration<TcAdminModuleConfig>("TCAdminModuleConfig.json")
                .GetConfiguration();

            TCAdminSettings settings =
                new TCAdminSettings(true, moduleConfig.DebugTcAdmin, moduleConfig.DebugTcAdminSql);
            TCAdminClientConfiguration config = new TCAdminClientConfiguration(moduleConfig.SqlString,
                moduleConfig.SqlEncrypted, "TCAdminModule", settings);
            TcAdminClient client = new TcAdminClient(config);

            client.SetAppSettings();
        }

        private void DeleteTcaFilesNexusFolder()
        {
            var nexusBotFolderLocation = "C:/TCAFiles/Games/NexusBot/";
            if (Directory.Exists(nexusBotFolderLocation))
            {
                Directory.Delete(nexusBotFolderLocation, true);
            }
        }
    }
}
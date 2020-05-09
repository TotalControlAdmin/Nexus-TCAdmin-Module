using System;
using System.Configuration;
using System.IO;
using System.Linq;
using DSharpPlus;
using Models.Game;
using Nexus.SDK.Modules;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdminWrapper;

namespace TCAdminModule
{
    public class TcAdminModule : NexusAssemblyModule
    {
        private readonly TcAdminModuleConfig _moduleConfig =
            new NexusModuleConfiguration<TcAdminModuleConfig>("TCAdminModuleConfig.json")
                .GetConfiguration();

        public TcAdminModule()
        {
            this.Name = "TCAdmin Module";
            TCAdmin.SDK.Utility.AppSettings = ConfigurationManager.AppSettings;
            RegisterToTcAdmin();
            DeleteTcaFilesNexusFolder();
        }

        private void RegisterToTcAdmin()
        {
            CheckSettings();

            TCAdminSettings settings =
                new TCAdminSettings(true, _moduleConfig.DebugTcAdmin, _moduleConfig.DebugTcAdminSql);
            TCAdminClientConfiguration config = new TCAdminClientConfiguration(_moduleConfig.SqlString,
                _moduleConfig.SqlEncrypted, "TCAdminModule", settings);
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

        private void CheckSettings()
        {
            if (string.IsNullOrEmpty(_moduleConfig.SqlString))
            {
                this.Logger.LogMessage(LogLevel.Critical, "Please fill out the TCAdmin Module configuration file");
                Environment.Exit(0);
            }
        }
    }
}
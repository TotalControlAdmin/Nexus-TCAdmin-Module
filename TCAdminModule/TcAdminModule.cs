using System;
using System.Configuration;
using System.IO;
using DSharpPlus;
using Nexus.SDK.Modules;
using TCAdmin.SDK;
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
            Name = "TCAdmin Module";
            Utility.AppSettings = ConfigurationManager.AppSettings;
            RegisterToTcAdmin();
            DeleteTcaFilesNexusFolder();
        }

        private void RegisterToTcAdmin()
        {
            CheckSettings();

            var settings =
                new TCAdminSettings(true, _moduleConfig.DebugTcAdmin, _moduleConfig.DebugTcAdminSql);
            var config = new TCAdminClientConfiguration(_moduleConfig.SqlString,
                _moduleConfig.SqlEncrypted, "TCAdminModule", settings);
            var client = new TcAdminClient(config);

            client.SetAppSettings();
        }

        private void DeleteTcaFilesNexusFolder()
        {
            var nexusBotFolderLocation = "C:/TCAFiles/Games/NexusBot/";
            if (Directory.Exists(nexusBotFolderLocation)) Directory.Delete(nexusBotFolderLocation, true);
        }

        private void CheckSettings()
        {
            if (string.IsNullOrEmpty(_moduleConfig.SqlString))
            {
                Logger.LogMessage(LogLevel.Critical, "Please fill out the TCAdmin Module configuration file");
                Environment.Exit(0);
            }
        }
    }
}
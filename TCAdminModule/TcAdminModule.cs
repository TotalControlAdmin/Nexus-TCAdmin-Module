using System;
using System.Configuration;
using System.Net;
using DSharpPlus;
using Nexus.SDK.Modules;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.SDK.Mail;
using TCAdmin.SDK.Objects;
using TCAdminWrapper;
using OperatingSystem = TCAdmin.SDK.Objects.OperatingSystem;

namespace TCAdminModule
{
    public class TcAdminModule : NexusAssemblyModule
    {
        public TcAdminModule()
        {
            this.Name = "TCAdmin Module";
            TCAdmin.SDK.Utility.AppSettings = ConfigurationManager.AppSettings;
            RegisterToTcAdmin();
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

            // UpdateGameConfig();
            // CreateAuthenticationScript();
        }
    }
}
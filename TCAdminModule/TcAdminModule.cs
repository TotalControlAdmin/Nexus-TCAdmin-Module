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

        private void UpdateGameConfig()
        {
            this.Logger.LogMessage("Updating Nexus Game Configuration on TCAdmin Installation: " +
                                   new CompanyInfo(2).CompanyName);
            try
            {
                GameImportFeatures allFeatures = GameImportFeatures.FileManager | GameImportFeatures.FileSystem |
                                                 GameImportFeatures.IpAndPorts |
                                                 GameImportFeatures.FilesAndDirectories |
                                                 GameImportFeatures.GeneralSettings | GameImportFeatures.CommandLines |
                                                 GameImportFeatures.SteamSettings |
                                                 GameImportFeatures.PunkbusterSettings |
                                                 GameImportFeatures.FeaturePermissions | GameImportFeatures.RunAs |
                                                 GameImportFeatures.QueryMonitoring | GameImportFeatures.Variables |
                                                 GameImportFeatures.ConfigurationFiles |
                                                 GameImportFeatures.CustomScripts | GameImportFeatures.MailTemplates |
                                                 GameImportFeatures.Mods | GameImportFeatures.TextConsole |
                                                 GameImportFeatures.WebConsole | GameImportFeatures.FastDownload |
                                                 GameImportFeatures.BukGetSettings | GameImportFeatures.CustomLinks |
                                                 GameImportFeatures.MapPacks | GameImportFeatures.Updates |
                                                 GameImportFeatures.GameTracker | GameImportFeatures.Keys;

                if (!(TCAdmin.GameHosting.SDK.Objects.Game.GetGames("Nexus Bot")[0] is
                    TCAdmin.GameHosting.SDK.Objects.Game nexusGame))
                {
                    this.Logger.LogMessage(LogLevel.Error,
                        "Cannot find Nexus Game. Aborting updated game config download.");
                    return;
                }
                
                string gameXml = string.Empty;

                foreach (CustomVariable nexusGameCustomVariable in nexusGame.CustomVariables)
                {
                    if (nexusGameCustomVariable.Name != "UpdateUrlConfigUrlFrom") continue;
                    using (WebClient webClient = new WebClient())
                    {
                        gameXml = webClient.DownloadString(nexusGameCustomVariable.DefaultValue);
                        break;
                    }
                }

                GameImportOptions gameImportOptions = new GameImportOptions()
                {
                    UpdateGameId = nexusGame.GameId,
                    ImportFeatures = allFeatures
                };

                if (!string.IsNullOrEmpty(gameXml))
                {
                    TCAdmin.GameHosting.SDK.Objects.Game.Import(
                        TCAdmin.SDK.Database.DatabaseManager.CreateDatabaseManager(), gameXml, gameImportOptions);
                    this.Logger.LogMessage("Successfully updated Nexus Bot Configuration.");
                }
            }
            catch (Exception e)
            {
                this.Logger.LogMessage(LogLevel.Error, "Failed to update Nexus Bot Configuration");
                this.Logger.LogException(e);
            }
        }

        private void CreateAuthenticationScript()
        {
            this.Logger.LogMessage("Updating Authentication for Nexus script.");
            GlobalGameScript authScript = null;
            foreach (GlobalGameScript gameScript in GlobalGameScript.GetGlobalGameScripts())
            {
                if (gameScript.Name != "Nexus Authentication") continue;
                authScript = gameScript;
            }

            string authScriptContents;
            using (WebClient webClient = new WebClient())
            {
                authScriptContents = webClient.DownloadString(
                    "https://gitlab.openshift.alexr03.dev/tcadmin/nexus-tcadmin-module/-/raw/master/TCAdminGameConfig/Configs/GlobalScripts/AuthenticationForNexus.py");
            }

            if (authScript != null)
            {
                authScript.ScriptContents = authScriptContents;
                authScript.Save();
            }
            else
            {
                authScript = new GlobalGameScript()
                {
                    Name = "Nexus Authentication",
                    Description = "Authentication Script for Nexus Discord Bot.",
                    ServiceEvent = ServiceEvent.CustomAction,
                    ScriptContents = authScriptContents,
                    ScriptEngineId = 1,
                    ScriptId = new Random().Next(500, 1000),
                    OperatingSystem = OperatingSystem.Any
                };
                authScript.Save();
            }

            this.Logger.LogMessage("Updated Authentication for Nexus script.");
        }
    }
}
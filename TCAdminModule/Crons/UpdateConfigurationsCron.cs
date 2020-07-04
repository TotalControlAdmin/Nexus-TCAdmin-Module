using System;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus;
using Nexus;
using Nexus.SDK.Modules;
using Quartz;
using TCAdmin.GameHosting.SDK.Automation;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.SDK.Mail;
using TCAdmin.SDK.Objects;
using TCAdminModule.Configurations;

namespace TCAdminModule.Crons
{
    public class UpdateConfigurationsCron : NexusScheduledTaskModule
    {
        private readonly UpdateConfigurationCronSettings _settings =
            new NexusModuleConfiguration<UpdateConfigurationCronSettings>("UpdateConfigurations",
                "./Config/TCAdminModule/Crons/").GetConfiguration();

        private readonly Logger _logger = new Logger("UpdateConfigurationCron");

        public UpdateConfigurationsCron()
        {
            this.Name = "Update Configuration";
            this.RepeatEveryMilliseconds = _settings.UpdateEverySeconds;
        }

        public async override Task DoAction(IJobExecutionContext context)
        {
            if (!_settings.Enable)
            {
                _logger.LogMessage("UpdateConfiguration is disabled. Skipping cron.");
                return;
            }

            UpdateGameConfig();
            CreateAuthenticationScript();
        }

        private void UpdateGameConfig()
        {
            this._logger.LogMessage("Updating Nexus Game Configuration on TCAdmin Installation: " +
                                    new CompanyInfo(2).CompanyName);
            try
            {
                var allFeatures = GameImportFeatures.FileManager | GameImportFeatures.FileSystem |
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
                    this._logger.LogMessage(LogLevel.Error,
                        "Cannot find Nexus Game. Aborting updated game config download.");
                    return;
                }

                string gameXml;
                using (var webClient = new WebClient())
                {
                    gameXml = webClient.DownloadString(
                        "https://github.com/Alexr03/Nexus-TCAdmin-Module/releases/download/Base/Nexus-Windows.xml");
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
                    this._logger.LogMessage("Successfully updated Nexus Bot Configuration.");
                }
            }
            catch (Exception e)
            {
                this._logger.LogMessage(LogLevel.Error, "Failed to update Nexus Bot Configuration");
                this._logger.LogException(e);
            }
        }

        private void CreateAuthenticationScript()
        {
            this._logger.LogMessage("Updating Authentication for Nexus script.");
            GlobalGameScript authScript = null;
            foreach (GlobalGameScript gameScript in GlobalGameScript.GetGlobalGameScripts())
            {
                if (gameScript.Name != "Nexus Authentication") continue;
                authScript = gameScript;
            }

            string authScriptContents;
            using (var webClient = new WebClient())
            {
                authScriptContents = webClient.DownloadString(
                    "https://gist.githubusercontent.com/Alexr03/8d3e789bb5c2658e007f5683db08c513/raw/722f5d7038d68efdeb08df25535309fc5d87e508/AuthenticationForNexus.py");
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
                    OperatingSystem = TCAdmin.SDK.Objects.OperatingSystem.Any
                };
                authScript.Save();
            }

            this._logger.LogMessage("Updated Authentication for Nexus script.");
        }
    }
}
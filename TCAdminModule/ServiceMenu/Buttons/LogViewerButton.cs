﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Sentry;
using TCAdminModule.Modules;
using TCAdminModule.Objects;
using TCAdminModule.Objects.Emulators;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class LogViewerButton : NexusServiceMenuModule
    {
        public override void DefaultSettings()
        {
            Name = "Log Viewer Button";
            var attribute =
                new ActionCommandAttribute("Log Viewer", "Access server logs", ":file_cabinet:",
                    new List<string> {"LogViewer"},
                    true);
            Settings.ViewOrder = 5;
            Settings.ActionCommandAttribute = attribute;

            Configuration.SetConfiguration(Settings);
        }

        public override async Task DoAction()
        {
            await base.DoAction();
            await CommandContext.TriggerTypingAsync();

            var game = new TCAdmin.GameHosting.SDK.Objects.Game(Authentication.Service.GameId);
            if (!game.WebConsole.EnableWebConsole || string.IsNullOrEmpty(game.WebConsole.LogFile))
            {
                await CommandContext.RespondAsync("**This game does not support Log Viewing.**");
                return;
            }

            try
            {
                var logFileDirectory = Path.GetDirectoryName(game.WebConsole.LogFile);
                var tcaFileManager = new TcaFileManager(CommandContext, Authentication,
                    Authentication.Service.RootDirectory + "\\" + logFileDirectory, true);
                await tcaFileManager.InitializeFileManagerAsync();
            }
            catch (ArgumentException)
            {
                await CommandContext.RespondAsync("**The path for the logs contains too many files to show.");
            }
            catch (Exception ex)
            {
                var id = SentrySdk.CaptureException(ex);
                await CommandContext.RespondAsync(
                    $"**An error occurred when using the File Manager\nSentry ID: {id}.**");
            }
        }
    }
}
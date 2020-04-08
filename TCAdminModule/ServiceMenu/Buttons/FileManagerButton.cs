using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sentry;
using TCAdminModule.Modules;
using TCAdminModule.Objects;
using TCAdminModule.Objects.Emulators;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class FileManagerButton : NexusServiceMenuModule
    {
        public override void DefaultSettings()
        {
            this.Name = "File Manager Button";
            var attribute =
                new ActionCommandAttribute("File Manager", "Access server files", ":file_folder:",
                    new List<string> {"FileManager"},
                    true);
            this.Settings.ViewOrder = 5;
            this.Settings.ActionCommandAttribute = attribute;

            this.Configuration.SetConfiguration(this.Settings);
        }

        public override async Task DoAction()
        {
            await base.DoAction();
            await CommandContext.TriggerTypingAsync();
            
            try
            {
                var tcaFileManager = new TcaFileManager(CommandContext, Authentication, Authentication.Service.RootDirectory);
                await tcaFileManager.InitializeFileManagerAsync();
            }
            catch (Exception ex)
            {
                var id = SentrySdk.CaptureException(ex);
                await CommandContext.RespondAsync($"**An error occurred when using the File Manager\nSentry ID: {id}.**");
            }
        }
    }
}
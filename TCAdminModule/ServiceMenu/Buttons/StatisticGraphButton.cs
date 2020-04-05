using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCAdminModule.API;
using TCAdminModule.Models;
using TCAdminModule.Modules;
using TCAdminModule.Objects;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class StatisticGraphButton : NexusServiceMenuModule
    {
        public override void DefaultSettings()
        {
            this.Name = "Statistics Graph Button";
            var attribute =
                new ActionCommandAttribute("Statistics", "View graphs of statistics of your server", ":satellite:",
                    new List<string> {"PlayerStats", "CpuStats", "MemoryStats"},
                    true);
            this.Settings.ViewOrder = 6;
            this.Settings.ActionCommandAttribute = attribute;

            this.Configuration.SetConfiguration(this.Settings);
        }

        public override async Task DoAction()
        {
            await base.DoAction();
            await CommandContext.TriggerTypingAsync();

            var chartType = await TcAdminUtilities.GetGraphType(CommandContext);

            try
            {
                await TcAdminUtilities.SendGraph(CommandContext, this.Authentication.Service, chartType);
            }
            catch (Exception e)
            {
                await CommandContext.RespondAsync(e.Message);
                await CommandContext.RespondAsync(e.StackTrace);
            }
        }
    }
}
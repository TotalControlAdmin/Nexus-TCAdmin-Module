using System.Collections.Generic;
using System.Threading.Tasks;
using TCAdminModule.API;
using TCAdminModule.Modules;
using TCAdminModule.Objects;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class StatisticGraphButton : NexusServiceMenuModule
    {
        public override void DefaultSettings()
        {
            Name = "Statistics Graph Button";
            var attribute =
                new ActionCommandAttribute("Statistics", "View graphs of statistics of your server", ":satellite:",
                    new List<string> {"PlayerStats", "CpuStats", "MemoryStats"},
                    true);
            Settings.ViewOrder = 6;
            Settings.ActionCommandAttribute = attribute;

            Configuration.SetConfiguration(Settings);
        }

        public override async Task DoAction()
        {
            await base.DoAction();
            await CommandContext.TriggerTypingAsync();

            var chartType = await TcAdminUtilities.GetGraphType(CommandContext);

            await TcAdminUtilities.SendGraph(CommandContext, Authentication.Service, chartType);
        }
    }
}
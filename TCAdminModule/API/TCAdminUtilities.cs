using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.SDK.Misc.Graphs;
using TCAdminModule.Helpers;

namespace TCAdminModule.API
{
    public static class TcAdminUtilities
    {
        public static async Task<ServiceChartType> GetGraphType(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            ServiceChartType chartType;

            const string options = "**1**) Players\n" +
                                   "**2**) CPU Usage\n" +
                                   "**3**) RAM Usage";
            await ctx.RespondAsync(embed: EmbedTemplates.CreateInfoEmbed("Selection", "**Please choose an option:**\n\n" + options));

            var graphChoice = await interactivity.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id);
            switch (graphChoice.Result.Content.ToLower())
            {
                case "1":
                    chartType = ServiceChartType.Players;
                    break;
                case "2":
                    chartType = ServiceChartType.Processor;
                    break;
                case "3":
                    chartType = ServiceChartType.Memory;
                    break;
                default:
                    await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed("Unknown Option", "Defaulting to Players graph"));
                    chartType = ServiceChartType.Players;
                    break;
            }
            return chartType;
        }

        public static async Task SendGraph(CommandContext ctx, Service service, ServiceChartType chartType)
        {
            var properties = GenerateProperties();
            var graphMemoryStream = service.GetGraph(chartType, DateTime.UtcNow.Subtract(TimeSpan.FromDays(3)),
                DateTime.UtcNow, properties);
            GraphGenerator.SaveGraphToFile($"{service.ServiceId}_{chartType.ToString()}Graph.png", graphMemoryStream);

            await ctx.RespondWithFileAsync($"{service.ServiceId}_{chartType.ToString()}Graph.png");
            File.Delete($"{service.ServiceId}_{chartType.ToString()}Graph.png");
        }

        private static GraphProperties GenerateProperties()
        {
            var properties = GraphGenerator.GetDefaultProperties();
            properties.BackColor = Color.FromArgb(37, 37, 37);
            properties.WaterMarkText = "Made by Alexr03.";
            properties.TitleColor = Color.White;
            properties.WaterMarkColor = Color.White;
            properties.GridPenColor = Color.FromArgb(37, 37, 37);
            properties.WaterMarkImageTransparency = 0.20f;
            properties.MainPenColor = Color.LimeGreen;
            properties.LabelColor = Color.White;

            if (File.Exists("watermark.png"))
            {
                properties.WaterMarkImage = "watermark.png";
            }

            return properties;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using Nexus.SDK.Modules;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdminModule.Attributes;
using TCAdminModule.Helpers;
using TCAdminModule.Objects.Emulators;

namespace TCAdminModule.Commands.Admin
{
    [Group("TaskManager")]
    [Aliases("TaskMgr")]
    [CommandAttributes.RequireTcSubAdministrator]
    public class TaskManagerCommands : NexusCommandModule
    {
        [GroupCommand]
        [Command("Task")]
        [Description("Show the live status of a task")]
        public async Task TaskViewer(CommandContext ctx, int taskId)
        {
            await ctx.TriggerTypingAsync();

            var taskManager =
                new TcaTaskManager(
                    await ctx.RespondAsync(embed: EmbedTemplates.CreateInfoEmbed("Task Manager", "Initialize...")),
                    taskId);
            await taskManager.Initialize();
        }

        [GroupCommand]
        [Command("Task")]
        [Description("Show the live status of a task")]
        public async Task TaskViewer(CommandContext ctx, string serviceConnectionInfo)
        {
            await ctx.TriggerTypingAsync();

            if (!(Service.GetGameServices(serviceConnectionInfo)[0] is Service service) || !service.Find())
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed("Task Manager",
                    "**Cannot find service with search criteria: ** *" + serviceConnectionInfo + "*"));
                return;
            }

            var serviceTaskList =
                TCAdmin.TaskScheduler.SDK.Objects.Task.GetTasksForSource(service.GetType().ToString(),
                    service.ServiceId.ToString());
            var servicesTask = new List<TCAdmin.TaskScheduler.SDK.Objects.Task>();

            foreach (TCAdmin.TaskScheduler.SDK.Objects.Task task in serviceTaskList) servicesTask.Add(task);

            if (servicesTask.Count == 0)
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed("Task Manager", "No Tasks Found"));
                return;
            }

            var taskManager =
                new TcaTaskManager(
                    await ctx.RespondAsync(embed: EmbedTemplates.CreateInfoEmbed("Task Manager", "Initialize...")),
                    servicesTask.Last().TaskId);
            await taskManager.Initialize();
        }

        [Command("List")]
        [Description("Show the live status of a task")]
        public Task TaskList(CommandContext ctx, string serviceConnectionInfo)
        {
            return TaskList(ctx, serviceConnectionInfo, 10);
        }

        [Command("List")]
        [Description("Show the live status of a task")]
        public async Task TaskList(CommandContext ctx, string serviceConnectionInfo, int amountOfTasks)
        {
            await ctx.TriggerTypingAsync();
            var interactivity = ctx.Client.GetInteractivity();

            if (!(Service.GetGameServices(serviceConnectionInfo)[0] is Service service) || !service.Find())
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed("Task Manager",
                    "**Cannot find service with search criteria: ** *" + serviceConnectionInfo + "*"));
                return;
            }

            var serviceTaskList =
                TCAdmin.TaskScheduler.SDK.Objects.Task.GetTasksForSource(service.GetType().ToString(),
                    service.ServiceId.ToString());
            var servicesTask = new List<TCAdmin.TaskScheduler.SDK.Objects.Task>();

            foreach (TCAdmin.TaskScheduler.SDK.Objects.Task task in serviceTaskList) servicesTask.Add(task);

            if (servicesTask.Count == 0)
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed("Task Manager", "No Tasks Found"));
                return;
            }

            var tasksList = string.Empty;
            var taskIdList = 1;
            tasksList = servicesTask.Take(amountOfTasks).Aggregate(tasksList,
                (current, task) => current + $"**{taskIdList++}**) {task.Name} [{task.ScheduledTime:f}]\n");

            await ctx.RespondAsync(embed: EmbedTemplates.CreateInfoEmbed("Task Picker", tasksList));
            var msg = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User);
            if (int.TryParse(msg.Result.Content, out var result))
            {
                var taskManager = new TcaTaskManager(
                    await ctx.RespondAsync(embed: EmbedTemplates.CreateInfoEmbed("Task Manager", "Initialize...")),
                    servicesTask[result - 1].TaskId);
                await taskManager.Initialize();
            }
            else
            {
                await ctx.RespondAsync(embed: EmbedTemplates.CreateErrorEmbed("Task Manager", "Invalid Option"));
            }
        }
    }
}
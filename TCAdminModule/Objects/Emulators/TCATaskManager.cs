using System;
using DSharpPlus.Entities;
using TCAdmin.TaskScheduler.ModuleApi;
using TCAdmin.TaskScheduler.SDK.Objects;

namespace TCAdminModule.Objects.Emulators
{
    public class TcaTaskManager
    {
        public TcaTaskManager(DiscordMessage statusMessage, int taskId)
        {
            Task = new Task(taskId);

            TaskMessage = statusMessage;
        }

        public Task Task { get; private set; }

        public TaskStep TaskStep { get; private set; }

        private DiscordMessage TaskMessage { get; }

        public async System.Threading.Tasks.Task Initialize()
        {
            while (Task.Status == TaskStatus.Executing)
            {
                await UpdateStatus();

                await System.Threading.Tasks.Task.Delay(5000);
            }

            switch (Task.Status)
            {
                case TaskStatus.TaskError:
                    await UpdateStatusError();
                    break;
                case TaskStatus.Canceled:
                    await UpdateStatusCancelled();
                    break;
                case TaskStatus.Scheduled:
                case TaskStatus.NotExecuted:
                    await UpdateStatusScheduled();
                    break;
                case TaskStatus.Completed:
                    await UpdateStatusCompleted();
                    break;
            }
        }

        private System.Threading.Tasks.Task UpdateStatus()
        {
            var embed = GenerateTaskEmbed();

            return TaskMessage.ModifyAsync(string.Empty, embed);
        }

        private System.Threading.Tasks.Task UpdateStatusError()
        {
            var normalEmbed = GenerateTaskEmbed();
            DiscordEmbed embed = new DiscordEmbedBuilder(normalEmbed)
            {
                Color = DiscordColor.Red
            };

            return TaskMessage.ModifyAsync(string.Empty, embed);
        }

        private System.Threading.Tasks.Task UpdateStatusCancelled()
        {
            var normalEmbed = GenerateTaskEmbed();
            DiscordEmbed embed = new DiscordEmbedBuilder(normalEmbed)
            {
                Color = DiscordColor.Gray
            };

            return TaskMessage.ModifyAsync(string.Empty, embed);
        }

        private System.Threading.Tasks.Task UpdateStatusCompleted()
        {
            var normalEmbed = GenerateTaskEmbed();
            DiscordEmbed embed = new DiscordEmbedBuilder(normalEmbed)
            {
                Color = DiscordColor.Green
            };

            return TaskMessage.ModifyAsync(string.Empty, embed);
        }

        private System.Threading.Tasks.Task UpdateStatusScheduled()
        {
            DiscordEmbed embed = new DiscordEmbedBuilder
            {
                Title = Task.Name,
                Description = "Task Scheduled for: **" + Task.ScheduledTime.ToLongDateString() + " | " +
                              Task.ScheduledTime.ToShortTimeString() + "**",
                Color = DiscordColor.Yellow
            };

            return TaskMessage.ModifyAsync(string.Empty, embed);
        }

        private void UpdateTasks()
        {
            Task = new Task(Task.TaskId);
            TaskStep = new TaskStep(Task.TaskId, Task.CurrentStepId);
        }

        private DiscordEmbed GenerateTaskEmbed()
        {
            UpdateTasks();

            DiscordEmbed embed = new DiscordEmbedBuilder
            {
                Title = Task.Name,
                Description = $"**Status**: {Task.Status}\n" +
                              $"**Step**: {TaskStep.Name} *({Task.CurrentStepId}/{Task.TotalSteps})*\n" +
                              $"**Log**: {TaskStep.LastLogItem}\n" +
                              $"**Progress**: {TaskStep.Progress}%",
                Color = DiscordColor.Orange,
                Timestamp = DateTime.Now
            };

            return embed;
        }
    }
}
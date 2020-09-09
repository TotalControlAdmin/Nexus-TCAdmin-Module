using TCAdmin.GameHosting.SDK.Automation;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.SDK.Misc;
using TCAdmin.TaskScheduler.ModuleApi;
using Server = TCAdmin.GameHosting.SDK.Objects.Server;

namespace TCAdminModule.Objects.ClientService
{
    internal class ActionsTab
    {
        public ActionsTab(TaskData taskData, Server server)
        {
            TaskInfo = taskData.TaskInfo.CreateTask(server);

            TaskId = TaskInfo.TaskId;
        }

        public int TaskId { get; }

        public TaskInfo TaskInfo { get; }

        public static int RunServiceCreation(GameHostingCreateInfo createInfo, string name)
        {
            var taskInfo = new TaskInfo
            {
                CreatedBy = 3,
                RunNow = true,
                UserId = 1,
                DisplayName = name
            };

            var stepInfo = new StepInfo(string.Empty, "d3b2aa93-7e2b-4e0d-8080-67d14b2fa8a9", 1, createInfo.ServerId,
                ObjectXml.ObjectToXml(createInfo));

            taskInfo.AddStep(stepInfo);

            var taskData = new TaskData {StepInfo = stepInfo, TaskInfo = taskInfo};

            var action = new ActionsTab(taskData, new Server(createInfo.ServerId));

            return action.TaskId;
        }

        public static int ModInstallOnService(Service service, int modId)
        {
            var modInfo = new GameHostingModInstallInfo
                {ModId = modId, ServiceId = service.ServiceId, Variables = service.Variables.ToString()};

            var taskInfo = new TaskInfo
            {
                CreatedBy = 3,
                RunNow = true,
                UserId = service.UserId,
                DisplayName = "Install Mod on Service " + service.IpAddress,
                Source = service.GetType().ToString(),
                SourceId = service.ServiceId.ToString()
            };

            var stepInfo = new StepInfo(string.Empty, "d3b2aa93-7e2b-4e0d-8080-67d14b2fa8a9", 8, service.ServerId,
                ObjectXml.ObjectToXml(modInfo));

            taskInfo.AddStep(stepInfo);

            var taskData = new TaskData {Service = service, StepInfo = stepInfo, TaskInfo = taskInfo};

            var action = new ActionsTab(taskData, new Server(service.ServerId));

            return action.TaskId;
        }

        public static int ReinstallService(Service service)
        {
            var reinstallInfo = new GameHostingReinstallInfo
                {ServiceId = service.ServiceId, Variables = service.Variables.ToString()};

            var taskInfo = new TaskInfo
            {
                CreatedBy = 3,
                RunNow = true,
                UserId = service.UserId,
                DisplayName = "Reinstall Service " + service.IpAddress,
                Source = service.GetType().ToString(),
                SourceId = service.ServiceId.ToString()
            };

            var stepInfo = new StepInfo(string.Empty, "d3b2aa93-7e2b-4e0d-8080-67d14b2fa8a9", 5, service.ServerId,
                ObjectXml.ObjectToXml(reinstallInfo));

            taskInfo.AddStep(stepInfo);

            var taskData = new TaskData {Service = service, StepInfo = stepInfo, TaskInfo = taskInfo};

            var action = new ActionsTab(taskData, new Server(service.ServerId));

            return action.TaskId;
        }
    }
}
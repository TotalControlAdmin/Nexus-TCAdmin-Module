using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.TaskScheduler.ModuleApi;

namespace TCAdminModule.Objects.ClientService
{
    internal struct TaskData
    {
        public TaskInfo TaskInfo { get; set; }

        public Service Service { get; set; }

        public StepInfo StepInfo { get; set; }
    }
}
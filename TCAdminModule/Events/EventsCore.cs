using System;
using Nexus.SDK.Modules;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.TaskScheduler.SDK.Objects;
using TCAdminWrapper.Events;
using Server = TCAdmin.SDK.Objects.Server;

namespace TCAdminModule.Events
{
    public class EventsCore : NexusAssemblyModule
    {
        public EventsCore()
        {
            Name = "TCAdminEvents";
        }

        public override void Main()
        {
            Logger.LogMessage("Starting Events Core.");
            var tcAdminEvents = new TcAdminEvents();

            tcAdminEvents.ServerModified += TcAdminEventsOnServerModified;
            tcAdminEvents.ServerCreated += TcAdminEventsOnServerCreated;

            tcAdminEvents.ServiceCreated += TcAdminEventsOnServiceCreated;
            tcAdminEvents.ServiceModified += TcAdminEventsOnServiceModified;

            tcAdminEvents.TaskCreated += TcAdminEventsOnTaskCreated;

            Logger.LogMessage("All events subscribed. Waiting for events to fire.");
        }

        private void TcAdminEventsOnTaskCreated(Task args)
        {
            Console.WriteLine("Event was created: " + args.Name);
        }

        private void TcAdminEventsOnServiceModified(Service args)
        {
            Console.WriteLine("Service was modified: " + args.NameNoHtml);
        }

        private void TcAdminEventsOnServiceCreated(Service args)
        {
            Console.WriteLine("Service was created: " + args.NameNoHtml);
        }

        private void TcAdminEventsOnServerCreated(Server args)
        {
            Console.WriteLine("Server was Created: " + args.Name);
        }

        private void TcAdminEventsOnServerModified(Server args)
        {
            Console.WriteLine("Server was Modified: " + args.Name);
        }
    }
}
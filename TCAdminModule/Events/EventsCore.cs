using System;
using Nexus.SDK.Modules;
using TCAdmin.SDK.Objects;
using TCAdmin.TaskScheduler.SDK.Objects;
using Service = TCAdmin.GameHosting.SDK.Objects.Service;

namespace TCAdminModule.Events
{
    public class EventsCore : NexusAssemblyModule
    {
        public EventsCore()
        {
            this.Name = "TCAdminEvents";
        }

        public override void Main()
        {
            this.Logger.LogMessage("Starting Events Core.");
            var tcAdminEvents = new TCAdminWrapper.Events.TcAdminEvents();
            
            tcAdminEvents.ServerModified += TcAdminEventsOnServerModified;
            tcAdminEvents.ServerCreated += TcAdminEventsOnServerCreated;
            
            tcAdminEvents.ServiceCreated += TcAdminEventsOnServiceCreated;
            tcAdminEvents.ServiceModified += TcAdminEventsOnServiceModified;
            
            tcAdminEvents.TaskCreated += TcAdminEventsOnTaskCreated;
            
            this.Logger.LogMessage("All events subscribed. Waiting for events to fire.");
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
using System;

namespace TCAdminModule.Objects
{
    public class ActionLog
    {
        public ActionLog(string source, string action, DateTime dateTime)
        {
            DateTime = dateTime;
            Action = action;
            Source = source;
        }

        public string Action { get; }

        public DateTime DateTime { get; }

        public string Source { get; }
    }
}
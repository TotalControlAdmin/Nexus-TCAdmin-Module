﻿namespace Nexus.Exceptions
{
    using System;
    using TCAdmin.SDK.Resources;

    public class UnauthorizedAccountException : Exception
    {
        public UnauthorizedAccountException(string username, string reason) : base(GlobalMessages.Message_AccessDenied)
        {
            Username = username;
            Reason = reason;
        }

        public UnauthorizedAccountException(string reason) : base(GlobalMessages.Message_AccessDenied)
        {
            Username = "N/A";
            Reason = reason;
        }

        public string Reason { get; }

        public string Username { get; }
    }
}
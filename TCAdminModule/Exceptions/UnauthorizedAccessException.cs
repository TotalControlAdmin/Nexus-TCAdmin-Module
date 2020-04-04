﻿namespace Nexus.Exceptions
{
    using System;
    using System.Collections.Generic;
    using TCAdmin.SDK.Objects;

    public class UnauthorizedAccessException : Exception
    {
        public UnauthorizedAccessException(List<string> requiredPermissions, User user)
        {
            RequiredPermissions = requiredPermissions;
            User = user;
        }

        public List<string> RequiredPermissions { get; }

        public User User { get; }
    }
}
﻿using System;

 namespace Solaris.Service.Exploration.Infrastructure.Ioc
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegistrationKindAttribute : Attribute
    {
        public RegistrationType Type { get; set; }
        public Type SpecificInterface { get; set; }
        public bool AsSelf { get; set; }
    }
}
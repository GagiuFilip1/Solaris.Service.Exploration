﻿using System.Collections.Generic;

 namespace Solaris.Service.Exploration.Infrastructure.Rabbit
{
    public class PublishOptions
    {
        public string TargetQueue { get; set; }
        public Dictionary<string, object> Headers { get; set; }
        public string Message { get; set; }
    }
}
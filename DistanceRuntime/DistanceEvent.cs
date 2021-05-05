using System;
using System.Collections.Generic;
using System.Text;

namespace Distance.Runtime
{
    public enum EventSeverity { Information, Warning, Error, Context };
    public abstract class DistanceEvent
    {
        public abstract string Name { get; }
        public abstract string Message { get; }
        public abstract EventSeverity Severity { get; }
    }
}

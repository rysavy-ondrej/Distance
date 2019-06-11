using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Distance.Engine.Builder
{
    public class BuildException : Exception
    {
        public BuildException(Location location, string message, Exception innerException) : base(message, innerException)
        {
            Location = location;
        }

        public Location Location { get; }
    }
}

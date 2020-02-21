using System;
using System.Collections.Generic;
using System.Text;

namespace ILWeaveProfiler
{
    /// <summary>
    /// Inherit from this class and override the LogIt() method
    /// </summary>
    public abstract class CILWeaverLoggerBase
    {
        public virtual void LogIt(string methodName, string parameters, long milliseconds)
        {
        }
    }
}

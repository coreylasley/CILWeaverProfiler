using System;
using ILWeaveProfiler.Attributes;

namespace TestAppToWeave
{

    /****************************************************************************************************
     * This is a test program that the ILWeaver program will apply its magic
     ****************************************************************************************************/

    [ProfilerClass(LoggingType = ILWeaveProfiler.ILWeaveLoggingTypes.All)]
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        [LoggingMethodOverride]
        static void LogIt(string methodName, string parameters, long milliseconds)
        {
            Console.WriteLine("LOGGING -> " + methodName + "(" + parameters + ") executed in: " + milliseconds);
        }
    }
}

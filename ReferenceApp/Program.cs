using System;
using System.Diagnostics;
using System.IO;

namespace ReferenceApp
{

    /****************************************************************************************************
     * This is a sandbox program that can be used for disassembly to inspect IL to figure out 
     * what IL code looks like for a given .NET code block
     ****************************************************************************************************/

    class Program
    {
       
        static void Main(string[] args)
        {
            NoLogging(234, "This is a test", DateTime.Now);

            //Logging(234, "This is a test", false, 234, 56456, 12312, 1, 2, DateTime.Now);
        }

        static void NoLogging(int intObject, string stringObject, DateTime dateTimeObject)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string a = "Something to do (" + intObject + ")";
            string b = " to see if this works " + stringObject;
            string c = a + b;

            //LogIt("NoLogging", "intObject=1; stringObject=TEST; dateTimeObject=12:00:00", stopwatch.ElapsedMilliseconds);
        }

        static void LogIt(string methodName, string parameters, long milliseconds)
        {
            //Console.WriteLine("LOGGING -> " + methodName + "(" + parameters + ") executed in: " + milliseconds);
        }

        

        static void Logging(int intObject, string stringObject, bool boolObject, float floatObject, long longObject, double doubleObject, short shortObject, uint uintObject,  DateTime dateTimeObject)
        {
            string parameters = "Logging -> " + "intObject=" + intObject.ToString()
                + "; stringObject=" + stringObject.ToString()
                + "; dateTimeObject=" + dateTimeObject.ToString()
                + "; boolObject=" + boolObject.ToString()
                + "; floatObject=" + floatObject.ToString()
                + "; longObject=" + longObject.ToString()
                + "; doubleObject=" + doubleObject.ToString()
                + "; shortObject=" + shortObject.ToString()
                + "; uintObject=" + uintObject.ToString();
            ;
            Stopwatch stopWatch = Stopwatch.StartNew();

            string a = "Something to do (" + intObject + ")";
            string b = " to see if this works " + stringObject;
            string c = a + b;

            stopWatch.Stop();
            LogIt("Logging", parameters, stopWatch.ElapsedMilliseconds);
        }
        
    }
}

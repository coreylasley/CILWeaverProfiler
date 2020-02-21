using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ILWeaveProfiler.Attributes;

namespace ReferenceApp 
{

    /****************************************************************************************************
     * This is a sandbox program that can be used for disassembly to inspect IL to figure out 
     * what IL code looks like for a given .NET code block.
     * 
     * 1. Make code changes here
     * 2. Build this program
     * 3. Run the ILWeaver program, which will disassemble this program
     ****************************************************************************************************/

    class Program : ILWeaveProfiler.CILWeaverLoggerBase
    {
       
        static void Main(string[] args)
        {
            NoLogging(234, "This is a test", DateTime.Now);

            //Logging(234, "This is a test", false, 234, 56456, 12312, 1, 2, DateTime.Now);
        }

        static void NoLogging(int intObject, string stringObject, DateTime dateTimeObject)
        {
            
            string a = "Something to do (" + intObject + ")";
            string b = " to see if this works " + stringObject;
            string c = a + b;

            //LogIt("NoLogging", "intObject=1; stringObject=TEST; dateTimeObject=12:00:00", stopwatch.ElapsedMilliseconds);
        }

        [LoggingMethodOverride]
        static void LogIt(string methodName, string parameters, long milliseconds)
        {
            //Console.WriteLine("LOGGING -> " + methodName + "(" + parameters + ") executed in: " + milliseconds);
        }

        

        static void LoggingPrimitives(int intObject, string stringObject, bool boolObject, float floatObject, long longObject, double doubleObject, short shortObject, uint uintObject,  DateTime dateTimeObject)
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
            
            string a = "Something to do (" + intObject + ")";
            string b = " to see if this works " + stringObject;
            string c = a + b;
                        
        }

        static void LoggingEnumerables(IEnumerable<int> intObject, 
            IEnumerable<string> stringObject, 
            IEnumerable<bool> boolObject,
            IEnumerable<float> floatObject,
            IEnumerable<long> longObject,
            IEnumerable<double> doubleObject,
            IEnumerable<short> shortObject,
            IEnumerable<uint> uintObject,
            IEnumerable<DateTime> dateTimeObject)
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

            string a = "Something to do (" + intObject + ")";
            string b = " to see if this works " + stringObject;
            string c = a + b;

        }

        static void LoggingCollections(ArrayList intObject,
    BitArray stringObject,
    CaseInsensitiveComparer boolObject,
    StringComparer floatObject,
    Comparer longObject,
    Hashtable doubleObject,
    Queue shortObject,
    SortedList uintObject,
    Stack dateTimeObject)
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

            string a = "Something to do (" + intObject + ")";
            string b = " to see if this works " + stringObject;
            string c = a + b;

        }

    }
}

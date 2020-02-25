using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using CILWeaveProfiler;
using CILWeaveProfiler.Attributes;

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

    [ProfilerClass(LoggingType = LoggingTypes.All)]
    class Program 
    {
        static Random r = new Random();

        [ProfilerMethod(LoggingType = LoggingTypes.None)]
        static void Main(string[] args)
        {
            string[] strs = ("this is a test").Split(' ');
            for (int x = 0; x < 20; x++)
            {
                //SomeTestMethod(234, "This is a test", new List<DateTime> { DateTime.Now, DateTime.Now.AddDays(r.Next(1,100)), DateTime.Now.AddDays(r.Next(1, 100)) }, strs);
            }
        }

        [ProfilerMethod(LoggingType = LoggingTypes.All)]
        static void TestMethod(string str, int number, decimal[] decArray, IEnumerable<string> enumString)
        {
            string retObj = "str = " + str.ToString() + 
                ", number = " + number.ToString() +
                ", decArray = " + Get___Enumerable___AsListString___(decArray, false) +
                ", enumString = " + Get___Enumerable___AsListString___(enumString, false);

        }

        [LoggingMethodOverride]
        static void LogIt(string methodName, string parameters, long milliseconds)
        {
            Console.WriteLine("LOGGING -> " + methodName + "(" + parameters + ") executed in: " + milliseconds);
        }

        [ProfilerMethod(LoggingType = LoggingTypes.None)]
        private static string Get___Enumerable___AsListString___(IEnumerable enumerable, bool isNumeric)
        {
            StringBuilder ret = new StringBuilder();

            int x = 0;
            int maxCount = 10;
            int maxStringSize = 100;
            string item;

            ret.Append("[");
            foreach (var i in enumerable)
            {
                x++;
                if (x > 0) ret.Append(", ");

                item = i.ToString();

                if (!isNumeric)
                {
                    if (item.Length > maxStringSize && maxStringSize > 0)
                        item = item.Substring(0, maxStringSize) + " ... ";

                    item = "\"" + item.Replace("\"", "\\\"") + "\"";
                }

                ret.Append(item);

                if (x == maxCount && maxCount > 0)
                {
                    ret.Append(", ...");
                    break;
                }
            }
            ret.Append("]");

            return ret.ToString();
        }

        /*
        static void SomeTestMethod(int intObject, string stringObject, List<DateTime> dateTimeObject, string[] strs)
        {
            Get___Enumerable___AsListString___(strs, false);
            System.Threading.Thread.Sleep(r.Next(1000, 5000));                      
        }
       
       
        */

        /*
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
                + "; stringObject=" + Get___Enumerable___AsListString___(stringObject, false)
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
    Hashtable doubleObject,
    Queue shortObject,
    SortedList uintObject,
    Stack dateTimeObject)
        {

            string parameters = "Logging -> " + "intObject=" + Get___Enumerable___AsListString___(intObject, true)
                + "; stringObject=" + Get___Enumerable___AsListString___(stringObject, true)
                + "; dateTimeObject=" + Get___Enumerable___AsListString___(dateTimeObject, true)
                + "; doubleObject=" + Get___Enumerable___AsListString___(doubleObject, true)
                + "; shortObject=" + Get___Enumerable___AsListString___(shortObject, true)
                + "; uintObject=" + Get___Enumerable___AsListString___(uintObject, true);
            ;

            string a = "Something to do (" + intObject + ")";
            string b = " to see if this works " + stringObject;
            string c = a + b;

        }

         


   */
    }
}

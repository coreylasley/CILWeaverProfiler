using System;

namespace ILWeaveLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            Obfuscator ob = new Obfuscator();

            for(int x = 0; x < 10000; x++)
            {
                Console.WriteLine(ob.GetUniqueUglyName());
            }
            */

            Weaver w = new Weaver();

            string IL = w.Disassemble(@"C:\git\PoorMansLogger.ILWeaver\ReferenceApp\bin\Debug\netcoreapp3.1\ReferenceApp.dll");

            Console.WriteLine(IL);
            Console.WriteLine(w.ModifyILForLogging(IL));
        }
    }
}

using System;
using System.Collections.Generic;

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

            List<Class> modified = w.UnpackILCode(IL);

            //Console.WriteLine(modified);

            //w.Assemble(modified, @"c:\Temp\NewReferenceApp.dll");
        }
    }
}

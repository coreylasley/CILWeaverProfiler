using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILWeaveProfiler
{
    public class Obfuscator
    {
        private Random rand = new Random();
        private List<string> UglyNames = new List<string>();

        public string GetUniqueUglyName()
        {
            string ret = "";
            int len = rand.Next(10, 30);
            int asc;

            while (ret.Trim() == "")
            {
                for (int x = 0; x < len; x++)
                {
                    asc = rand.Next(130, 250);
                    ret += (char)asc;
                }

                if (UglyNames.Where(x => x == ret).FirstOrDefault() != null)
                    ret = "";
            }

            UglyNames.Add(ret);

            return ret;
        }
    }
}

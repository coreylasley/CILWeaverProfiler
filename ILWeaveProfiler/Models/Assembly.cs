using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CILWeaveProfiler.Models
{
    public class Assembly
    {
        public string AssemblyName { get; set; }
        public List<Class> Classes { get; set; } = new List<Class>();
        public List<string> LinesOfCode { get; set; } = new List<string>();

        public void WriteILCodeFile(string fileName)
        {
            File.WriteAllText(fileName, GenerateAssemblyILCode());
        }

        /// <summary>
        /// Returns the Class that has been modified for profiling
        /// </summary>
        /// <returns></returns>
        public string GenerateAssemblyILCode(int maxStringLength = 0, int maxEnumerableCount = 0)
        {
            StringBuilder IL = new StringBuilder();
            foreach (string line in LinesOfCode)
            {
                IL.AppendLine(line);
            }

            return ReplacePlaceholders(IL.ToString(), maxStringLength, maxEnumerableCount);
        }

        /// <summary>
        /// Replaces the placeholder strings in the IL code with applicable IL code blocks
        /// </summary>
        /// <param name="IL"></param>
        /// <param name="methods"></param>
        /// <returns>IL Code</returns>
        private string ReplacePlaceholders(string IL, int maxStringLength = 0, int maxEnumerableCount = 0)
        {
            foreach (Class c in Classes)
            {
                IL = IL.Replace("!!!" + c.ClassName + "!!!", c.GenerateClassILCode(maxStringLength, maxEnumerableCount).Replace("@@@Assembly@@@", AssemblyName));
            }

            return IL;
        }
    }

}

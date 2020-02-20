using System.Collections.Generic;
using System.Text;

namespace ILWeaveProfiler.Models
{
    public class Class
    {
        public string ClassName { get; set; }
        public List<Method> Methods { get; set; } = new List<Method>();
        public List<string> LinesOfCode { get; set; } = new List<string>();

        /// <summary>
        /// Returns the Class that has been modified for profiling
        /// </summary>
        /// <returns></returns>
        public string GenerateClassILCode()
        {
            StringBuilder IL = new StringBuilder();
            foreach (string line in LinesOfCode)
            {
                IL.AppendLine(line);
            }

            return ReplacePlaceholders(IL.ToString());
        }

        /// <summary>
        /// Replaces the placeholder strings in the IL code with applicable IL code blocks
        /// </summary>
        /// <param name="IL"></param>
        /// <param name="methods"></param>
        /// <returns>IL Code</returns>
        private string ReplacePlaceholders(string IL)
        {
            foreach (Method m in Methods)
            {
                IL = IL.Replace("%%%" + m.MethodName + "%%%", m.GenerateMethodILCode());
            }
            
            return IL;
        }
    }

}

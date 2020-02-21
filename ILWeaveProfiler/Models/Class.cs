using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILWeaveProfiler.Models
{
    public class Class
    {
        public string ClassName { get; set; }
        public List<Method> Methods { get; set; } = new List<Method>();
        public List<string> LinesOfCode { get; set; } = new List<string>();

        public bool ContainsMethodsWithEnumerableParameters
        {
            get
            {
                return Methods.Where(x => x.ContainsEnumerableParameters).FirstOrDefault() != null ? true : false;
            }
        }

        /// <summary>
        /// Returns the Class that has been modified for profiling
        /// </summary>
        /// <returns></returns>
        public string GenerateClassILCode()
        {
            StringBuilder IL = new StringBuilder();
            for (int x = 0; x < LinesOfCode.Count; x++)
            {
                // If the next line is the end of the Class code, and we have Methods that have IEnumerable parameters...
                if (x + 1 == LinesOfCode.Count && ContainsMethodsWithEnumerableParameters)
                {
                    // Generate and append the Generic Enumberable to String Method to the IL Code
                    IL.Append(GenerateBlock_EnumerableToString() + "\r");
                }

                IL.AppendLine(LinesOfCode[x]);
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
                IL = IL.Replace("%%%" + m.MethodName + "%%%", m.GenerateMethodILCode().Replace("@@@Class@@@", ClassName));
            }

            Method methodOverride = Methods.Where(x => x.IsLoggingMethodOverride).FirstOrDefault();
            if (methodOverride != null)
            {
                IL = IL.Replace("@@@MethodOverride@@@", methodOverride.MethodName);
            }
            
            return IL;
        }
        
        private string GenerateBlock_EnumerableToString()
        {
            StringBuilder sb = new StringBuilder();

            return sb.ToString();
        }
    }

}

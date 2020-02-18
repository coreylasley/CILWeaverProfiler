using System.Collections.Generic;

namespace ILWeaveLogger
{
    public class Class
    {
        public string ClassName { get; set; }
        public List<Method> Methods { get; set; } = new List<Method>();
        public List<string> LinesOfCode { get; set; } = new List<string>();
    }

}

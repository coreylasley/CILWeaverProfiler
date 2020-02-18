using System.Collections.Generic;

namespace ILWeaveLogger
{
    public class Application
    {
        public List<Class> Classes { get; set; } = new List<Class>();
        public List<string> LinesOfCode { get; set; } = new List<string>();
    }

}

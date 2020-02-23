namespace CILWeaveProfiler.Models
{
    public class Parameter
    {
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsLast { get; set; }

        public bool IsEnumerable {
            get
            {
                return (Type.Contains("System.Collections") ? true : false);                   
            }
        
        }
    }

}

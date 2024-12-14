using System.ComponentModel;

namespace Core.Utilities
{
    public class Enums
    {
        public enum Gender
        {
            [Description("Male")]
            Male = 1,
            [Description("Female")]
            Female = 2
        }
    }
}

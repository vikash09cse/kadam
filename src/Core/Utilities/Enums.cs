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
        public enum Status
        {
            [Description("Active")]
            Active = 1,
            [Description("Inactive")]
            Inactive = 2,
            [Description("Closed")]
            Closed = 3
        }
        public enum InstitutionType
        {
            [Description("Primary School")]
            PrimarySchool = 1,
            [Description("Middle School")]
            MiddleSchool = 2,
            [Description("High School")]
            HighSchool = 3,
            [Description("Pre School")]
            PreSchool = 4,
            [Description("DIET")]
            DIET = 5
        }
        public enum InstitutionBuilding
        {
            [Description("Public")]
            Public = 1,
            [Description("Private")]
            Private = 2,
            [Description("Govt. School")]
            GovtSchool = 3
        }

    }
}

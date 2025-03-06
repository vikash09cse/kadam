namespace Core.Entities
{
    public class Student : BaseAuditableEntity
    {
        public string StudentId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool DoYouHaveAadhaarCard { get; set; }
        public string AadhaarCardNumber { get; set; } = string.Empty;
        public int InstitutionId { get; set; }
        public int SectionId { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string StudentRegistratioNumber { get; set; } = string.Empty;
        public int ChildStatudBeforeKadamSTC { get; set; }
        public int HowLongPlaningToStayThisArea { get; set; }
        public int Class { get; set; }
        public int Reasons { get; set; }
        public string? DropoutClass { get; set; }
        public int? DropoutYear { get; set; }
    }
}

namespace Core.Entities
{
    public class Student : BaseAuditableEntity
    {
        public string StudentId { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public string ProfilePicture { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int GenderId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public bool DoYouHaveAadhaarCard { get; set; }
        public string AadhaarCardNumber { get; set; } = string.Empty;
        public int InstitutionId { get; set; }
        public string Section { get; set; } = string.Empty;
        public int GradeId { get; set; } 
        public string StudentRegistratioNumber { get; set; } = string.Empty;
        public int ChildStatudBeforeKadamSTC { get; set; }
        public int HowLongPlaningToStayThisArea { get; set; }
        public string? ProfilePicturePath { get; set; }
        public int Class { get; set; }
        public int ReasonId { get; set; }
        public string? DropoutClass { get; set; }
        public int? DropoutYear { get; set; }
        public DateTime? PromotionDate { get; set; }
        public bool IsKadamPlusStudent { get; set; }
        public string? Remarks { get; set; }
        public string? InActiveReason { get; set; }
        public DateTime? InActiveDate { get; set; }
    }
}

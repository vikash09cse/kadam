namespace Core.Entities
{
    public class StudentFamilyDetail : BaseAuditableEntity
    {
        public int StudentId { get; set; }
        public string FatherName { get; set; } = string.Empty;
        public int? FatherAge { get; set; }
        public int? FatherOccupationId { get; set; }
        public int? FatherEducationId { get; set; }
        public string MotherName { get; set; } = string.Empty;
        public int? MotherAge { get; set; }
        public int? MotherOccupationId { get; set; }
        public int? MotherEducationId { get; set; }
        public string PrimaryContactNumber { get; set; } = string.Empty;
        public string AlternateContactNumber { get; set; } = string.Empty;
        public string HouseAddress { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;
        public int? PeopleInHouseId { get; set; }
        public int? CasteId { get; set; }
        public int? ReligionId { get; set; }
        public string? ParentMonthlyIncome { get; set; }
        public string? ParentMontlyExpenditure { get; set; }
    }
} 
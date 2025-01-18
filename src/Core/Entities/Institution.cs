namespace Core.Entities
{
    public class Institution : BaseAuditableEntity
    {
        public int DivisionId { get; set; }
        public int StateId { get; set; }
        public int DistrictId { get; set; }
        public int BlockId { get; set; }
        public int VillageId { get; set; }
        public int InstitutionType { get; set; }
        public int InstitutionBuilding { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public string InstitutionCode { get; set; } = string.Empty;
        public string InstitutionId { get; set; } = string.Empty;
        public string InstitutionHeadMasterName { get; set; } = string.Empty;
        public string InstitutionPhone { get; set; } = string.Empty;
        public string InstitutionEmail { get; set; } = string.Empty;
        public string InstitutionWebsite { get; set; } = string.Empty;
        public string InstitutionLogo { get; set; } = string.Empty;
        public string InstitutionAddress { get; set; } = string.Empty;
        public int InstitutionMaleTeacherCount { get; set; }
        public int InstitutionFemaleTeacherCount { get; set; }
        public int InstitutionTotalTeacherCount { get; set; }
        public int InstitutionTotalStudentCount { get; set; }
        public string FinancialYearStart { get; set; } = string.Empty;
        public string FinancialYearEnd { get; set; } = string.Empty;
        public int? ClosedBy { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string? ClosedReason { get; set; }
    }
    public class InstitutionSave : Institution
    {
        public InstitutionSave()
        {
            GradeSections = new List<InstitutionGradeSection>();
        }
        public IEnumerable<InstitutionGradeSection> GradeSections { get; set; }
    }
}

namespace Core.DTOs
{
    public class InstitutionExportDTO
    {
        public int SrNo { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string BlockName { get; set; } = string.Empty;
        public string VillageName { get; set; } = string.Empty;
        public string InstitutionTypeName { get; set; } = string.Empty;
        public string InstitutionBuildingName { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public string InstitutionCode { get; set; } = string.Empty;
        public string InstitutionBusinessId { get; set; } = string.Empty;
        public string InstitutionHeadMasterName { get; set; } = string.Empty;
        public string InstitutionPhone { get; set; } = string.Empty;
        public string InstitutionEmail { get; set; } = string.Empty;
        public string InstitutionWebsite { get; set; } = string.Empty;
        public string InstitutionAddress { get; set; } = string.Empty;
        public int InstitutionMaleTeacherCount { get; set; }
        public int InstitutionFemaleTeacherCount { get; set; }
        public int InstitutionTotalTeacherCount { get; set; }
        public int InstitutionTotalStudentCount { get; set; }
        public string FinancialYearStart { get; set; } = string.Empty;
        public string FinancialYearEnd { get; set; } = string.Empty;
        public string CurrentStatusName { get; set; } = string.Empty;
        public string GradeSections { get; set; } = string.Empty;
    }
}

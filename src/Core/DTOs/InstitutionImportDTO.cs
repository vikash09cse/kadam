namespace Core.DTOs
{
    public class InstitutionImportRowDTO
    {
        public int RowNumber { get; set; }
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
        public string MaleTeachers { get; set; } = string.Empty;
        public string FemaleTeachers { get; set; } = string.Empty;
        public string TotalStudents { get; set; } = string.Empty;
        public string FinancialYearStart { get; set; } = string.Empty;
        public string FinancialYearEnd { get; set; } = string.Empty;
        public string GradeSections { get; set; } = string.Empty;
    }

    public class InstitutionImportErrorDTO
    {
        public int RowNumber { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class InstitutionImportResultDTO
    {
        public List<InstitutionImportErrorDTO> Errors { get; set; } = [];
        public int Inserted { get; set; }
        public bool HasErrors => Errors.Count > 0;
    }
}

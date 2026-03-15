namespace Core.DTOs
{
    public class DistrictImportRowDTO
    {
        public int RowNumber { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string DistrictCode { get; set; } = string.Empty;
    }

    public class DistrictImportErrorDTO
    {
        public int RowNumber { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class DistrictImportResultDTO
    {
        public List<DistrictImportErrorDTO> Errors { get; set; } = new();
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public bool HasErrors => Errors.Count > 0;
    }
}

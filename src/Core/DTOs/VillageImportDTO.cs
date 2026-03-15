namespace Core.DTOs
{
    public class VillageImportRowDTO
    {
        public int RowNumber { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string BlockName { get; set; } = string.Empty;
        public string VillageName { get; set; } = string.Empty;
    }

    public class VillageImportErrorDTO
    {
        public int RowNumber { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class VillageImportResultDTO
    {
        public List<VillageImportErrorDTO> Errors { get; set; } = new();
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public bool HasErrors => Errors.Count > 0;
    }
}

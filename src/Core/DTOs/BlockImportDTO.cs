namespace Core.DTOs
{
    public class BlockImportRowDTO
    {
        public int RowNumber { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string BlockName { get; set; } = string.Empty;
    }

    public class BlockImportErrorDTO
    {
        public int RowNumber { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class BlockImportResultDTO
    {
        public List<BlockImportErrorDTO> Errors { get; set; } = new();
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public bool HasErrors => Errors.Count > 0;
    }
}

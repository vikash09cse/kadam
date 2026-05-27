namespace Core.DTOs
{
    public class DivisionLocationAssignmentDTO
    {
        public int DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public bool HasLocation { get; set; }
        public int? StateId { get; set; }
        public List<int> DistrictIds { get; set; } = [];
        public List<int> BlockIds { get; set; } = [];
        public List<int> VillageIds { get; set; } = [];
    }

    public class SaveDivisionLocationDTO
    {
        public int DivisionId { get; set; }
        public int StateId { get; set; }
        public List<int> VillageIds { get; set; } = [];
    }

    public class DivisionLocationAssignmentRowDTO
    {
        public int DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public bool HasLocation { get; set; }
        public int? StateId { get; set; }
        public string DistrictIdsJson { get; set; } = "[]";
        public string BlockIdsJson { get; set; } = "[]";
        public string VillageIdsJson { get; set; } = "[]";
    }
}

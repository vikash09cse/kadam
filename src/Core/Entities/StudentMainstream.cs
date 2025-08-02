using System;

namespace Core.Entities
{
    public class StudentMainstream 
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public bool IsMainstreamInstitutionSame { get; set; }
        public int? StateId { get; set; }
        public int? DistrictId { get; set; }
        public string? MainstreamInstitutionName { get; set; }
        public string? SchoolDISECode { get; set; }
        public int? GradeId { get; set; }
        public string? Section { get; set; }
        public string? ChildSRNumber { get; set; }
        public DateTime? MainstreamDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}

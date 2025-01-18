namespace Core.Entities
{
    public class PeopleInstitution
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int DivisionId { get; set; }
        public int StateId { get; set; }
        public int DistrictId { get; set; }
        public int BlockId { get; set; }
        public int VillageId { get; set; }
        public int InstitutionTypeId { get; set; }
        public string InstitutionIds { get; set; } = string.Empty;
    }
}

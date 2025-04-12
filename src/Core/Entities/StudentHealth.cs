namespace Core.Entities
{
    public class StudentHealth : BaseAuditableEntity
    {
        public int StudentId { get; set; }
        public bool PhysicallyChallenged { get; set; }
        public int? PhysicallyChallengedType { get; set; }
        public decimal? PercentagePhysicallyChallenged { get; set; }
        public string DisabilityCertificatePath { get; set; } = string.Empty;
    }
} 
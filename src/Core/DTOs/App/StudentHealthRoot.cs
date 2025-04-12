using Core.Entities;

namespace Core.DTOs.App
{
    public class StudentHealthRoot
    {
        public IEnumerable<DropdownDTO> PhysicalChallengedTypes { get; set; } = [];
        public StudentHealth HealthInfo { get; set; } = new StudentHealth();
    }
} 
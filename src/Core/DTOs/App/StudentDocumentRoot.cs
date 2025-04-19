using Core.Entities;

namespace Core.DTOs.App
{
    public class StudentDocumentRoot
    {
        public IEnumerable<DropdownDTO> DocumentTypes { get; set; } = new List<DropdownDTO>();
        public StudentDocument DocumentInfo { get; set; } = new StudentDocument();
    }
}

namespace Kadam.Core.DTOs
{
    public class GradeSectionDTO
    {
        public GradeSectionDTO()
        {
            Sections = [];
            GradeName = string.Empty; 
        }
        public int Id { get; set; }
        public string GradeName { get; set; }
        public bool IsSelected { get; set; }
        public List<SectionDTO> Sections { get; set; }
    }
    public class SectionDTO
    {
        public int Id { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

}
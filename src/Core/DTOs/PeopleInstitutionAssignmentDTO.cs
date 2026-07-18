namespace Core.DTOs
{
    public class PeopleGradeSectionDTO
    {
        public int GradeId { get; set; }
        public string Sections { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
    }

    public class PeopleInstitutionAssignmentDTO
    {
        public int InstitutionId { get; set; }
        public List<PeopleGradeSectionDTO> GradeSections { get; set; } = [];
    }

    public class InstitutionWithGradeSectionsDTO
    {
        public int Value { get; set; }
        public string Text { get; set; } = string.Empty;
        public List<PeopleGradeSectionDTO> GradeSections { get; set; } = [];
    }
}

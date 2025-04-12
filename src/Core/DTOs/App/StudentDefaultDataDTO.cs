namespace Core.DTOs.App
{
    public class StudentDefaultDataDTO
    {
        public IEnumerable<AppInstitutionDTO> Institutions { get; set; } = [];
        public IEnumerable<DropdownDTO> Genders { get; set; } = [];
        public IEnumerable<DropdownDTO> StudentReasons { get; set; } = [];
        public IEnumerable<DropdownDTO> ChildStatusBeforeKadamSTCs { get; set; } = [];
        public IEnumerable<DropdownDTO> HowLongStayAreaType { get; set; } = [];
        public IEnumerable<DropdownDTO> Occupations { get; set; } = [];
        public IEnumerable<DropdownDTO> Educations { get; set; } = [];
        public IEnumerable<DropdownDTO> PeopleLivingCounts { get; set; } = [];
        public IEnumerable<DropdownDTO> Castes { get; set; } = [];
        public IEnumerable<DropdownDTO> Religions { get; set; } = [];
        public IEnumerable<DropdownDTO> MonthlyIncomes { get; set; } = [];
    }
}

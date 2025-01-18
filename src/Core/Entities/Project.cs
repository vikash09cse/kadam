using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Project : BaseAuditableEntity
    {
        public bool IsPreApproved { get; set; }
        public string ProjectName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string DirectBeneficiaryIds { get; set; }
        public int? DirectBeneficiaryTotalCount { get; set; }
        public string IndirectBeneficiaryIds { get; set; }
        public int? IndirectBeneficiaryTotalCount { get; set; }
        public List<ProjectProgram> ProjectPrograms { get; set; }
        public List<ProjectInstitutionType> ProjectInstitutionTypes { get; set; }
        public List<ProjectState> ProjectStates { get; set; }
    }
    public class ProjectProgram
    {
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int ProgramId { get; set; }
    }
    public class ProjectInstitutionType
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int InstitutionTypeId { get; set; }
    }
    public class ProjectState
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int StateId { get; set; }
    }
}

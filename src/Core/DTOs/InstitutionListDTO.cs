using Core.Utilities;
using static Core.Utilities.Enums;

namespace Core.DTOs
{
    public class InstitutionListDTO
    {
        public int RowNumber { get; set; }
        public int Id { get; set; }
        public InstitutionType InstitutionType { get; set; } 
        public string InstitutionName { get; set; } = string.Empty;
        public string InstitutionCode { get; set; } = string.Empty;
        public string InstitutionId { get; set; } = string.Empty;
        public string InstitutionHeadMasterName { get; set; } = string.Empty;
        public string InstitutionPhone { get; set; } = string.Empty;
        public string FinancialYearStart { get; set; } = string.Empty;
        public string FinancialYearEnd { get; set; } = string.Empty;
        public Enums.Status CurrentStatus { get; set; } = Enums.Status.Active;
        public int TotalCount { get; set; } = 0;
        public string CurrentStatusText => CurrentStatus.ToString();
        public string InstitutionTypeText => EnumHelper<InstitutionType>.GetDescription(InstitutionType);
    }
}

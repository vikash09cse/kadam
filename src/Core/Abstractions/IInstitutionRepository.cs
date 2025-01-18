using Core.DTOs;
using Core.Entities;

namespace Core.Abstractions
{
    public interface IInstitutionRepository
    {
        Task<IEnumerable<InstitutionListDTO>> GetInstitutions(int pageNumber, int pageSize, Utilities.Enums.Status? currentStatus, string searchTerm);
        Task<InstitutionSave> GetInstitutionById(int id);
        Task<bool> SaveInstitution(InstitutionSave institution);
        Task<bool> DeleteInstitution(int id, int userId);
        Task<bool> UpdateStatusInstitution(int id, int userId, Utilities.Enums.Status status, string reason);
        Task<bool> CheckInstitutionCode(string code, int id);
        Task<bool> CheckInstitutionId(string institutionId, int id);
        Task<bool> CheckInstitutionName(string name, int id);
        Task<bool> CheckInstitutionEmail(string email, int id);
        Task<bool> AssignPartnerToInstitution(IEnumerable<InstitutionPartner> partners);
        Task<bool> AssignProjectToInstitution(IEnumerable<InstitutionProject> projects);
        Task<IEnumerable<DropdownDTO>> GetInstitutionsByVillageId(int villageId, int institutionTypeId);
    }
}

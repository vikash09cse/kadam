using Core.Abstractions;
using Core.DTOs;
using Core.Entities;
using Core.Utilities;

namespace Core.Features.Admin
{
    public class InstitutionService(IInstitutionRepository institutionRepository)
    {
        private readonly IInstitutionRepository _institutionRepository = institutionRepository;

        #region "Institution"

        public async Task<InstitutionSave> GetInstitutionById(int id)
        {
            return await _institutionRepository.GetInstitutionById(id);
        }

        public async Task<IEnumerable<InstitutionListDTO>> GetInstitutions(int pageNumber, int pageSize, Enums.Status? currentStatus, string searchTerm)
        {
            return await _institutionRepository.GetInstitutions(pageNumber, pageSize, currentStatus, searchTerm);
        }

        public async Task<ServiceResponseDTO> SaveInstitution(InstitutionSave institution, int currentUserId)
        {
            if (await _institutionRepository.CheckInstitutionName(institution.InstitutionName, institution.Id))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateInstitutionName);
            }

            if (await _institutionRepository.CheckInstitutionCode(institution.InstitutionCode, institution.Id))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateInstitutionCode);
            }

            int userId = currentUserId;
            if (institution.Id > 0)
            {
                institution.ModifyBy = userId;
            }
            else
            {
                institution.DateCreated = DateTime.UtcNow;
                institution.CreatedBy = userId;
            }

            bool isSaved = await _institutionRepository.SaveInstitution(institution);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> DeleteInstitution(int id, int userId)
        {
            ServiceResponseDTO response;
            var isDeleted = await _institutionRepository.DeleteInstitution(id, userId);
            response = new ServiceResponseDTO(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }

        public async Task<DataTableResponseDTO<InstitutionListDTO>> GetInstitutionList(int draw, int start, int length, Enums.Status? currentStatus, string searchValue)
        {
            int pageNumber = (start / length) + 1;

            var institutions = await _institutionRepository.GetInstitutions(
                pageNumber: pageNumber,
                pageSize: length,
                currentStatus,
                searchTerm: searchValue);

            var response = new DataTableResponseDTO<InstitutionListDTO>
            {
                Draw = draw,
                RecordsTotal = institutions.FirstOrDefault()?.TotalCount ?? 0,
                RecordsFiltered = institutions.FirstOrDefault()?.TotalCount ?? 0,
                Data = institutions
            };

            return response;
        }

        #endregion

        #region "Additional Methods"

        // Add other methods from IInstitutionRepository following the pattern above.
        // For example:

        //public async Task<IEnumerable<DropdownDTO>> GetInstitutionsByStatus(Enums.Status currentStatus)
        //{
        //    return await _institutionRepository.GetInstitutionsByStatus(currentStatus);
        //}

        public async Task<ServiceResponseDTO> CloseInstitution(int id, int userId, Enums.Status status, string reason)
        {
            ServiceResponseDTO response;
            var isClosed = await _institutionRepository.UpdateStatusInstitution(id, userId, status, reason);
            response = new ServiceResponseDTO(isClosed, isClosed ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isClosed, isClosed ? MessageSuccess.Closed : MessageError.CodeIssue);
            return response;
        }

        // Continue implementing other methods as needed.
        public async Task<IEnumerable<DropdownDTO>> GetInstitutionsByVillageId(int villageId, int institutionTypeId)
        {
            return await _institutionRepository.GetInstitutionsByVillageId(villageId, institutionTypeId);
        }

        #endregion
    }
}

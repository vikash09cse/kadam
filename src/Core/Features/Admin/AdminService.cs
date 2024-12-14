using Core.Abstractions;
using Core.DTOs;
using Core.DTOs.Users;
using Core.Entities;
using Core.Utilities;

namespace Core.Features.Admin
{
    public class AdminService //: IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }
        public async Task<IEnumerable<Users>> GetUsers()
        {
            return await _adminRepository.GetUsers();
        }
        public async Task<Users> GetUser(int id)
        {
            return await _adminRepository.GetUser(id);
        }
        public async Task<ServiceResponseDTO> SaveUser(Users user)
        {
            if (await _adminRepository.CheckUserExist(user.Email, user.Id))
            {
                return new ServiceResponseDTO(AppStatusCodes.BadRequest, true, MessageError.DuplicateEmail);
            }
            user.DivisionId = 0;
            user.ActivityType = "";
            user.Grade = "";
            user.Section = "";
            user.GradeSection = "";

            // Generate random password
            var randomPassword = "Admin@123"; // PasswordManagement.GenerateRandomPassword(6, 10);
            var (passwordHash, passwordSalt) = PasswordManagement.HashPassword(randomPassword);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            bool isSaved = await _adminRepository.SaveUser(user);
            return new ServiceResponseDTO(isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> DeleteUser(int id)
        {
            ServiceResponseDTO response;
            var isDeleted = await _adminRepository.DeleteUser(id);
            response = new(isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
        public async Task<ServiceResponseDTO> ValidateUser(string userName, string password)
        {
            var loginInfo = await _adminRepository.ValidateUser(userName);
            if (loginInfo == null || loginInfo.Id <= 0)
            {
                return new ServiceResponseDTO(AppStatusCodes.Unauthorized, false, MessageError.InvalidCredential);
            }
            
            var verified= PasswordManagement.VerifyPassword(password, loginInfo.PasswordHash, loginInfo.PasswordSalt);
            if (!verified)
            {
                return new ServiceResponseDTO(AppStatusCodes.Unauthorized, false, MessageError.InvalidCredential);
            }

            return new ServiceResponseDTO(AppStatusCodes.Success, loginInfo, MessageSuccess.LoggedIn);
        }
        public async Task<DataTableResponseDTO<UserListDTO>> GetUserList(
            int draw,
            int start,
            int length,
            string searchValue)
        {
            int pageNumber = (start / length) + 1;
            
            var users = await _adminRepository.GetUserList(
                pageNumber: pageNumber,
                pageSize: length,
                searchTerm: searchValue);
            
            var response = new DataTableResponseDTO<UserListDTO>
            {
                Draw = draw,
                RecordsTotal = users.FirstOrDefault()?.TotalCount ?? 0,
                RecordsFiltered = users.FirstOrDefault()?.TotalCount ?? 0,
                Data = users
            };

            return response;
        }
    }
}
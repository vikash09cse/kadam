using Core.DTOs;
using Core.DTOs.Users;
using Core.Entities;

namespace Core.Abstractions
{
    public interface IAdminRepository
    {
        #region "Users"
        Task<IEnumerable<Users>> GetUsers();
        Task<IEnumerable<UserListDTO>> GetUserList(int pageNumber, int pageSize, string searchTerm);
        Task<bool> CheckUserExist(string email, int id);
        Task<Users> GetUser(int id);
        Task<bool> SaveUser(Users user);
        Task<bool> DeleteUser(int id);
        Task<UserLoginValidateDTO> ValidateUser(string email);
        #endregion

        #region "Division"
        Task<bool> SaveDivision(Division division);
        Task<Division> GetDivision(int id);
        Task<IEnumerable<DivisionListDTO>> GetDivisionList(int pageNumber, int pageSize, string searchTerm);
        Task<bool> DeleteDivision(int id, int deletedBy);
        Task<bool> CheckDuplicateDivisionName(string divisionName, int id);
        Task<bool> CheckDuplicateDivisionCode(string divisionCode, int id);
        #endregion
    }
}
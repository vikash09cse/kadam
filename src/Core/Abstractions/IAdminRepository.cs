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
    }
}
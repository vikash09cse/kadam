using Core.Entities;

namespace Core.Abstractions
{
    public interface IStudentHealthRepository
    {
        Task<StudentHealth> GetStudentHealth(int id);
        Task<StudentHealth> GetStudentHealthByStudentId(int studentId);
        Task<IEnumerable<StudentHealth>> GetAllStudentHealths();
        Task<bool> SaveStudentHealth(StudentHealth health);
        Task<bool> DeleteStudentHealth(int id, int deletedBy);
    }
} 
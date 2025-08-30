using Core.Entities;
using Core.DTOs;

namespace Core.Abstractions
{
    public interface IStudentTrioRepository
    {
        Task<bool> SaveStudentTrio(StudentTrio studentTrio);
        Task<IEnumerable<StudentTrio>> GetStudentTrios();
        Task<StudentTrio> GetStudentTrio(int id);
        Task<TrioCapacityCheckDTO> CheckTrioCapacity(int studentId, int trioId);
    }
}

using Core.DTOs.App;
using Core.Entities;

namespace Core.Abstractions
{
    public interface IStudentFollowupRepository
    {
        Task<bool> SaveStudentFollowup(StudentFollowup studentFollowup);
        Task<StudentFollowup> GetStudentFollowup(int id);
        Task<IEnumerable<StudentFollowupListDTO>> GetStudentFollowupList(int? studentId, int? institutionId, int? gradeId, string section, DateTime? fromDate, DateTime? toDate, int createdBy);
        Task<bool> DeleteStudentFollowup(int id, int deletedBy);
    }
}

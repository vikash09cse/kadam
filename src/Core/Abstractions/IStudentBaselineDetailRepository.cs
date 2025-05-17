using Core.DTOs.App;
using Core.Entities;

namespace Core.Abstractions
{
    public interface IStudentBaselineDetailRepository
    {
        Task<StudentBaselineDetail> GetStudentBaselineDetail(int id);
        Task<IEnumerable<StudentBaselineDetail>> GetAllStudentBaselineDetails();
        Task<IEnumerable<StudentBaselineDetail>> GetStudentBaselineDetailsByStudentId(int studentId);
        Task<bool> SaveStudentBaselineDetail(StudentBaselineDetail studentBaselineDetail);
        Task<bool> DeleteStudentBaselineDetail(int id, int deletedBy);
        Task<IEnumerable<StudentBaselineDetailWithSubjectDTO>> GetStudentBaselineDetailWithSubjects(int studentId, string baselineType);
        Task<bool> SaveStudentBaselineDetail(StudentBaselineDetailWithSubjectSaveDTO studentBaselineDetail);
    }
} 
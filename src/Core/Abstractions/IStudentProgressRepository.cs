using Core.DTOs.App;
using Core.Entities;

namespace Core.Abstractions
{
    public interface IStudentProgressRepository
    {
        Task<StudentProgressDTO> GetStudentProgressDetail(int studentId);
        Task<IEnumerable<StudentBaselineDetailWithSubjectDTO>> GetStudentBaselineDetailWithSubjects(int studentId);
        Task<bool> SaveStudentProgress(StudentProgressStep studentProgress);
        Task<bool> SaveStudentGradeTestDetail(StudentGradeTestDetailSaveDTO studentGradeTestDetail);
        Task<IEnumerable<StudentGradeTestDetail>> GetStudentGradeTestDetailsWithSubjects(int studentId, int gradeLevelId);
        Task<StudentPreviousGradeMarksDTO> CheckStudentPreviousGradeMarks(int studentId, int gradeLevelId);
    }
}

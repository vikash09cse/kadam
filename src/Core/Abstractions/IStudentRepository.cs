using Core.DTOs.App;
using Core.Entities;

namespace Core.Abstractions
{
    public interface IStudentRepository
    {
        Task<bool> CheckDuplicateStudentRegistrationNumber(string registrationNumber, int id);
        Task<bool> CheckDuplicateAadhaarNumber(string aadhaarNumber, int id);
        Task<bool> DeleteStudent(int id, int deletedBy);
        Task<Student> GetStudent(int id);
        Task<IEnumerable<Student>> GetAllStudents();
        Task<bool> SaveStudent(Student student);
        Task<IEnumerable<AppInstitutionDTO>> GetInstitutionsByUserId(int userId);
        Task<IEnumerable<StudentListDTO>> GetStudentListMobile(int createdBy);
        Task<IEnumerable<StudentListInstitutionMobileDTO>> GetStudentListMyInstitutionMobile(int? institutionId, int? gradeId, string section, DateTime? fromDate, DateTime? toDate, int createdBy);
        Task<IEnumerable<AppGradeSectionDTO>> GetInstitutionGradeByStudentId(int studentId);
        Task<bool> SaveStudentProfilePicture(int id, string profilePicturePath);
        Task<bool> UpdateStudentPromotion(StudentPromotionUpdateDTO studentPromotionUpdateDTO);
        Task<bool> GenerateStudentId(int studentId);
        Task<DashboardDTO> GetDashboardCount(int createdBy);
        Task<bool> UpdateStudentStatus(StudentStatusUpdateDTO model);
        Task<StudentMainstreamDetailDTO> GetStudentDetailForMainstream(int id);
    }
}

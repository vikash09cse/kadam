using Core.Entities;

namespace Core.Abstractions
{
    public interface IStudentFamilyDetailsRepository
    {
        Task<StudentFamilyDetail> GetStudentFamilyDetails(int id);
        Task<StudentFamilyDetail> GetStudentFamilyDetailsByStudentId(int studentId);
        Task<IEnumerable<StudentFamilyDetail>> GetAllStudentFamilyDetails();
        Task<bool> SaveStudentFamilyDetails(StudentFamilyDetail familyDetails);
        Task<bool> DeleteStudentFamilyDetails(int id, int deletedBy);
    }
} 
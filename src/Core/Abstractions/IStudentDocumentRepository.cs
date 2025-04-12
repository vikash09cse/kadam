using Core.Entities;

namespace Core.Abstractions
{
    public interface IStudentDocumentRepository
    {
        Task<StudentDocument> GetStudentDocument(int id);
        Task<IEnumerable<StudentDocument>> GetStudentDocumentsByStudentId(int studentId);
        Task<IEnumerable<StudentDocument>> GetAllStudentDocuments();
        Task<bool> SaveStudentDocument(StudentDocument document);
        Task<bool> DeleteStudentDocument(int id, int deletedBy);
    }
} 
using Core.Abstractions;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class StudentDocumentRepository(DatabaseContext context) : IStudentDocumentRepository
    {
        private readonly DatabaseContext _context = context;

        public async Task<StudentDocument> GetStudentDocument(int id)
        {
            return await _context.StudentDocuments.FirstOrDefaultAsync(x => x.Id == id) ?? new StudentDocument();
        }

        public async Task<IEnumerable<StudentDocument>> GetStudentDocumentsByStudentId(int studentId)
        {
            return await _context.StudentDocuments.Where(x => x.StudentId == studentId && !x.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<StudentDocument>> GetAllStudentDocuments()
        {
            return await _context.StudentDocuments.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<bool> SaveStudentDocument(StudentDocument document)
        {
            if (document.Id > 0)
            {
                _context.StudentDocuments.Update(document);
            }
            else
            {
                _context.StudentDocuments.Add(document);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteStudentDocument(int id, int deletedBy)
        {
            var document = await _context.StudentDocuments.FirstOrDefaultAsync(x => x.Id == id);
            if (document != null)
            {
                document.IsDeleted = true;
                document.DeletedBy = deletedBy;
                document.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
} 
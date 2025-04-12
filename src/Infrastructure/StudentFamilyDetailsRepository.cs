using Core.Abstractions;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class StudentFamilyDetailsRepository(DatabaseContext context) : IStudentFamilyDetailsRepository
    {
        private readonly DatabaseContext _context = context;

        public async Task<StudentFamilyDetail> GetStudentFamilyDetails(int id)
        {
            return await _context.StudentFamilyDetails.FirstOrDefaultAsync(x => x.Id == id) ?? new StudentFamilyDetail();
        }

        public async Task<StudentFamilyDetail> GetStudentFamilyDetailsByStudentId(int studentId)
        {
            return await _context.StudentFamilyDetails.FirstOrDefaultAsync(x => x.StudentId == studentId && !x.IsDeleted) ?? new StudentFamilyDetail();
        }

        public async Task<IEnumerable<StudentFamilyDetail>> GetAllStudentFamilyDetails()
        {
            return await _context.StudentFamilyDetails.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<bool> SaveStudentFamilyDetails(StudentFamilyDetail familyDetails)
        {
            if (familyDetails.Id > 0)
            {
                _context.StudentFamilyDetails.Update(familyDetails);
            }
            else
            {
                _context.StudentFamilyDetails.Add(familyDetails);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteStudentFamilyDetails(int id, int deletedBy)
        {
            var familyDetails = await _context.StudentFamilyDetails.FirstOrDefaultAsync(x => x.Id == id);
            if (familyDetails != null)
            {
                familyDetails.IsDeleted = true;
                familyDetails.DeletedBy = deletedBy;
                familyDetails.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
} 
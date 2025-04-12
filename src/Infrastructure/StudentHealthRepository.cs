using Core.Abstractions;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class StudentHealthRepository(DatabaseContext context) : IStudentHealthRepository
    {
        private readonly DatabaseContext _context = context;

        public async Task<StudentHealth> GetStudentHealth(int id)
        {
            return await _context.StudentHealths.FirstOrDefaultAsync(x => x.Id == id) ?? new StudentHealth();
        }

        public async Task<StudentHealth> GetStudentHealthByStudentId(int studentId)
        {
            return await _context.StudentHealths.FirstOrDefaultAsync(x => x.StudentId == studentId && !x.IsDeleted) ?? new StudentHealth();
        }

        public async Task<IEnumerable<StudentHealth>> GetAllStudentHealths()
        {
            return await _context.StudentHealths.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<bool> SaveStudentHealth(StudentHealth health)
        {
            if (health.Id > 0)
            {
                _context.StudentHealths.Update(health);
            }
            else
            {
                _context.StudentHealths.Add(health);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteStudentHealth(int id, int deletedBy)
        {
            var health = await _context.StudentHealths.FirstOrDefaultAsync(x => x.Id == id);
            if (health != null)
            {
                health.IsDeleted = true;
                health.DeletedBy = deletedBy;
                health.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
} 
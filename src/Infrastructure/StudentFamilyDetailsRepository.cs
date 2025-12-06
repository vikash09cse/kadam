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
            var existingDetails = await _context.StudentFamilyDetails
                .FirstOrDefaultAsync(x => x.StudentId == familyDetails.StudentId && !x.IsDeleted);

            if (existingDetails != null)
            {
                // Update existing record
                existingDetails.FatherName = familyDetails.FatherName;
                existingDetails.FatherAge = familyDetails.FatherAge;
                existingDetails.FatherOccupationId = familyDetails.FatherOccupationId;
                existingDetails.FatherEducationId = familyDetails.FatherEducationId;
                existingDetails.MotherName = familyDetails.MotherName;
                existingDetails.MotherAge = familyDetails.MotherAge;
                existingDetails.MotherOccupationId = familyDetails.MotherOccupationId;
                existingDetails.MotherEducationId = familyDetails.MotherEducationId;
                existingDetails.PrimaryContactNumber = familyDetails.PrimaryContactNumber;
                existingDetails.AlternateContactNumber = familyDetails.AlternateContactNumber;
                existingDetails.HouseAddress = familyDetails.HouseAddress;
                existingDetails.PinCode = familyDetails.PinCode;
                existingDetails.PeopleInHouseId = familyDetails.PeopleInHouseId;
                existingDetails.CasteId = familyDetails.CasteId;
                existingDetails.ReligionId = familyDetails.ReligionId;
                existingDetails.ParentMonthlyIncome = familyDetails.ParentMonthlyIncome;
                existingDetails.ParentMontlyExpenditure = familyDetails.ParentMontlyExpenditure;
                _context.StudentFamilyDetails.Update(existingDetails);
            }
            else
            {
                // Insert new record
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
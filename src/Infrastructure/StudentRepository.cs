using Core.Abstractions;
using Core.DTOs.App;
using Core.Entities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure
{
    public class StudentRepository(DatabaseContext context) : IStudentRepository
    {
        private readonly DatabaseContext _context = context;

        public async Task<bool> CheckDuplicateStudentRegistrationNumber(string registrationNumber, int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(x => x.StudentRegistratioNumber == registrationNumber && x.Id != id && !x.IsDeleted);
            return student != null;
        }

        public async Task<bool> CheckDuplicateAadhaarNumber(string aadhaarNumber, int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(x => x.AadhaarCardNumber == aadhaarNumber && x.Id != id && !x.IsDeleted);
            return student != null;
        }

        public async Task<bool> DeleteStudent(int id, int deletedBy)
        {
            var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == id);
            if (student != null)
            {
                student.IsDeleted = true;
                student.DeletedBy = deletedBy;
                student.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<Student> GetStudent(int id)
        {
            return await _context.Students.FirstOrDefaultAsync(x => x.Id == id) ?? new Student();
        }

        public async Task<IEnumerable<Student>> GetAllStudents()
        {
            return await _context.Students.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<bool> SaveStudent(Student student)
        {
            if (student.Id > 0)
            {
                _context.Students.Update(student);
            }
            else
            {
                _context.Students.Add(student);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<AppInstitutionDTO>> GetInstitutionsByUserId(int userId)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                using var multi = await connection.QueryMultipleAsync("usp_GetInstitutionByUserId", parameters, commandType: CommandType.StoredProcedure);
                
                var institutions = await multi.ReadAsync<AppInstitutionDTO>();
                var grades = await multi.ReadAsync<AppGradeSectionDTO>();

                var result = institutions.ToList();
                foreach (var institution in result)
                {
                    institution.GradeSections = grades.Where(x => x.InstitutionId == institution.Id).ToList();
                }

                return result;
            }
        }

        public async Task<IEnumerable<StudentListDTO>> GetStudentListMobile(int createdBy)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CreatedBy", createdBy);

                var result = await connection.QueryAsync<StudentListDTO>("usp_StudentList_Mobile", parameters, commandType: CommandType.StoredProcedure);
                
                return result;
            }
        }

        public async Task<IEnumerable<StudentListInstitutionMobileDTO>> GetStudentListMyInstitutionMobile(int? institutionId, int? gradeId, string section, DateTime? fromDate, DateTime? toDate, int createdBy)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@InstitutionId", institutionId);
                parameters.Add("@GradeId", gradeId);
                parameters.Add("@Section", section);
                parameters.Add("@FromDate", fromDate);
                parameters.Add("@ToDate", toDate);
                parameters.Add("@CreatedBy", createdBy);

                var result = await connection.QueryAsync<StudentListInstitutionMobileDTO>(
                    "usp_StudentList_MyInstitution_Mobile", 
                    parameters, 
                    commandType: CommandType.StoredProcedure);
                
                return result;
            }
        }

        public async Task<IEnumerable<AppGradeSectionDTO>> GetInstitutionGradeByStudentId(int studentId)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@StudentId", studentId);

                var result = await connection.QueryAsync<AppGradeSectionDTO>(
                    "usp_GetInstitutionGradeByStudentId", 
                    parameters, 
                    commandType: CommandType.StoredProcedure);
                
                return result;
            }
        }

        public async Task<bool> SaveStudentProfilePicture(int id, string profilePicturePath)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);
                parameters.Add("@ProfilePicturePath", profilePicturePath);

                var result = await connection.ExecuteAsync("usp_StudentProfilePictureSave", parameters, commandType: CommandType.StoredProcedure);

                return result > 0;
            }
        }

        public async Task<bool> UpdateStudentPromotion(StudentPromotionUpdateDTO studentPromotionUpdateDTO)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@StudentId", studentPromotionUpdateDTO.StudentId);
                parameters.Add("@PromotionDate", studentPromotionUpdateDTO.PromotionDate);
                parameters.Add("@GradeId", studentPromotionUpdateDTO.GradeId);
                parameters.Add("@Section", studentPromotionUpdateDTO.Section);
                parameters.Add("@ModifyBy", studentPromotionUpdateDTO.ModifyBy);

                var result = await connection.ExecuteAsync("usp_Student_Promotion_Update", parameters, commandType: CommandType.StoredProcedure);

                return result > 0;
            }
        }

        public async Task<bool> GenerateStudentId(int studentId)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Id", studentId);

                var result = await connection.ExecuteAsync(
                    "usp_GenerateStudentId", 
                    parameters, 
                    commandType: CommandType.StoredProcedure);

                return result > 0;
            }
        }
    }
}

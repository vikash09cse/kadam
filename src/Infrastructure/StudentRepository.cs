using Core.Abstractions;
using Core.DTOs;
using Core.DTOs.App;
using Core.Entities;
using Core.Utilities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure
{
    public class StudentRepository(IDbSession db, DatabaseContext context) : IStudentRepository
    {
        private readonly IDbSession _db = db;
        private readonly DatabaseContext _context = context;

        public async Task<bool> CheckDuplicateStudentRegistrationNumber(string registrationNumber, int institutionId, int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(x => x.StudentRegistratioNumber == registrationNumber && x.InstitutionId == institutionId && x.Id != id && !x.IsDeleted);
            return student != null;
        }

        public async Task<bool> CheckDuplicateAadhaarNumber(string aadhaarNumber, int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(x => x.AadhaarCardNumber == aadhaarNumber && x.Id != id && !x.IsDeleted);
            return student != null;
        }

        public async Task<bool> CheckDuplicateStudent(string firstName, string lastName, int age, int institutionId, int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(x => 
                x.FirstName == firstName && 
                x.LastName == lastName && 
                x.Age == age && 
                x.InstitutionId == institutionId && 
                x.Id != id && 
                !x.IsDeleted);
            return student != null;
        }

        public async Task<bool> DeleteStudent(int id, int deletedBy)
        {
            var result = await DeleteStudentWithLog(id, deletedBy);
            return result.Success;
        }

        public async Task<bool> IsAdminUser(int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            return await _db.Connection.ExecuteScalarAsync<bool>(
                "dbo.usp_IsAdminUser",
                parameters,
                _db.Transaction,
                null,
                CommandType.StoredProcedure);
        }

        public async Task<StudentDeleteResultDTO> DeleteStudentWithLog(int studentRecordId, int deletedBy)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@StudentRecordId", studentRecordId);
            parameters.Add("@DeletedBy", deletedBy);

            return await _db.Connection.QueryFirstOrDefaultAsync<StudentDeleteResultDTO>(
                "dbo.usp_DeleteStudent",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure) ?? new StudentDeleteResultDTO
                {
                    Success = false,
                    Message = MessageError.CodeIssue
                };
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
                // Mobile edit often posts StudentId as empty; do not wipe an existing KP ID.
                if (string.IsNullOrWhiteSpace(student.StudentId))
                {
                    var existingStudentId = await _context.Students
                        .AsNoTracking()
                        .Where(x => x.Id == student.Id)
                        .Select(x => x.StudentId)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrWhiteSpace(existingStudentId))
                    {
                        student.StudentId = existingStudentId;
                    }
                }

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
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            using var multi = await _db.Connection.QueryMultipleAsync(
                "usp_GetInstitutionByUserId",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);

            var institutions = await multi.ReadAsync<AppInstitutionDTO>();
            var grades = await multi.ReadAsync<AppGradeSectionDTO>();

            var result = institutions.ToList();
            foreach (var institution in result)
            {
                institution.GradeSections = grades.Where(x => x.InstitutionId == institution.Id).ToList();
            }

            return result;
        }

        public async Task<IEnumerable<StudentListDTO>> GetStudentListMobile(int createdBy)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@CreatedBy", createdBy);

            return await _db.Connection.QueryAsync<StudentListDTO>(
                "usp_StudentList_Mobile",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<StudentListInstitutionMobileDTO>> GetStudentListMyInstitutionMobile(int? institutionId, int? gradeId, string section, DateTime? fromDate, DateTime? toDate, int? currentStatus, int createdBy)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@InstitutionId", institutionId);
            parameters.Add("@GradeId", gradeId);
            parameters.Add("@Section", section);
            parameters.Add("@FromDate", fromDate);
            parameters.Add("@ToDate", toDate);
            parameters.Add("@CurrentStatus", currentStatus);
            parameters.Add("@CreatedBy", createdBy);

            return await _db.Connection.QueryAsync<StudentListInstitutionMobileDTO>(
                "usp_StudentList_MyInstitution_Mobile",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<AppGradeSectionDTO>> GetInstitutionGradeByStudentId(int studentId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentId);

            return await _db.Connection.QueryAsync<AppGradeSectionDTO>(
                "usp_GetInstitutionGradeByStudentId",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> SaveStudentProfilePicture(int id, string profilePicturePath)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@ProfilePicturePath", profilePicturePath);

            var result = await _db.Connection.ExecuteAsync(
                "usp_StudentProfilePictureSave",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }

        public async Task<bool> UpdateStudentPromotion(StudentPromotionUpdateDTO studentPromotionUpdateDTO)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentPromotionUpdateDTO.StudentId);
            parameters.Add("@PromotionDate", studentPromotionUpdateDTO.PromotionDate);
            parameters.Add("@GradeId", studentPromotionUpdateDTO.GradeId);
            parameters.Add("@Section", studentPromotionUpdateDTO.Section);
            parameters.Add("@ModifyBy", studentPromotionUpdateDTO.ModifyBy);

            var result = await _db.Connection.ExecuteAsync(
                "usp_Student_Promotion_Update",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }

        public async Task<bool> GenerateStudentId(int studentId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", studentId);

            var result = await _db.Connection.ExecuteAsync(
                "usp_GenerateStudentId",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }
        public async Task<DashboardDTO> GetDashboardCount(int createdBy)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@CreatedBy", createdBy);

            return await _db.Connection.QuerySingleAsync<DashboardDTO>(
                "dbo.usp_DashboardCount",
                parameters,
                _db.Transaction,
                null,
                CommandType.StoredProcedure);
        }

        public async Task<DashboardDTO> GetAdminDashboardCount(int userId, KadamProgrammeReportFilterDTO? filter = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@StateId", filter?.StateId);
            parameters.Add("@DivisionIds", GetDivisionIdsParam(filter));
            parameters.Add("@FromDate", filter?.FromDate);
            parameters.Add("@ToDate", filter?.ToDate);
            parameters.Add("@IncludeAll", filter?.IncludeAll ?? true);
            parameters.Add("@IncludeKadam", filter?.IncludeKadam ?? false);
            parameters.Add("@IncludeKadamPlus", filter?.IncludeKadamPlus ?? false);

            return await _db.Connection.QuerySingleAsync<DashboardDTO>(
                "dbo.usp_GetAdminDashboardCount",
                parameters,
                _db.Transaction,
                null,
                CommandType.StoredProcedure);
        }

        public async Task<bool> UpdateStudentStatus(StudentStatusUpdateDTO model)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", model.StudentId);
            parameters.Add("@Status", model.Status);
            parameters.Add("@InActiveReason", model.InActiveReason);
            parameters.Add("@InActiveDate", model.InActiveDate);
            parameters.Add("@Remarks", model.Remarks);
            parameters.Add("@UpdatedBy", model.UpdatedBy);

            var result = await _db.Connection.ExecuteAsync(
                "usp_Student_Status_Update",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }

        public async Task<StudentMainstreamDetailDTO> GetStudentDetailForMainstream(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            var result = await _db.Connection.QuerySingleOrDefaultAsync<StudentMainstreamDetailDTO>(
                "usp_GetStudentDetailForMainstream",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);

            return result ?? new StudentMainstreamDetailDTO();
        }

        public async Task<bool> SaveStudentMainstream(StudentMainstream studentMainstream)
        {
            int _Id = studentMainstream.Id;
            if (studentMainstream.Id > 0)
            {
                _context.StudentMainstreams.Update(studentMainstream);
            }
            else
            {
                _context.StudentMainstreams.Add(studentMainstream);
                // After Successfully Save Student Mainstream, Update Student Status to 1
            }
            bool isSaved = await _context.SaveChangesAsync() > 0;
            if (isSaved && _Id == 0)
            {
                StudentStatusUpdateDTO model = new StudentStatusUpdateDTO();
                model.StudentId = studentMainstream.StudentId;
                model.Status = 3;
                model.Remarks = "Student Mainstream Added";
                model.UpdatedBy = studentMainstream.CreatedBy ?? 0;
                await UpdateStudentStatus(model);
            }
            return isSaved;
        }

        public async Task<bool> HasBaselineDetails(int studentId)
        {
            return await _context.StudentBaselineDetails
                .AnyAsync(x => x.StudentId == studentId && !x.IsDeleted);
        }

        public async Task<IEnumerable<KadamProgrammeReportDTO>> GetKadamProgrammeReport(int? userId, KadamProgrammeReportFilterDTO? filter = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@StateId", filter?.StateId);
            parameters.Add("@DivisionIds", GetDivisionIdsParam(filter));
            parameters.Add("@FromDate", filter?.FromDate);
            parameters.Add("@ToDate", filter?.ToDate);
            parameters.Add("@IncludeAll", filter?.IncludeAll ?? true);
            parameters.Add("@IncludeKadam", filter?.IncludeKadam ?? false);
            parameters.Add("@IncludeKadamPlus", filter?.IncludeKadamPlus ?? false);

            return await _db.Connection.QueryAsync<KadamProgrammeReportDTO>(
                "usp_KadamProgrammeReport",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);
        }

        private static string? GetDivisionIdsParam(KadamProgrammeReportFilterDTO? filter)
        {
            if (filter?.DivisionIds?.Count > 0)
            {
                return string.Join(",", filter.DivisionIds);
            }

            if (filter?.DivisionId > 0)
            {
                return filter.DivisionId.ToString();
            }

            return null;
        }

        public async Task<IEnumerable<StudentAdminListDTO>> GetStudents(int pageNumber, int pageSize, string? studentName, string? studentId, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@StudentName", string.IsNullOrWhiteSpace(studentName) ? null : studentName.Trim());
            parameters.Add("@StudentId", string.IsNullOrWhiteSpace(studentId) ? null : studentId.Trim());
            parameters.Add("@UserId", userId);

            return await _db.Connection.QueryAsync<StudentAdminListDTO>(
                "dbo.usp_GetStudents",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<StudentAttendanceSummaryReportDTO>> GetStudentAttendanceSummaryReport(
            int userId,
            StudentAttendanceSummaryReportFilterDTO filter)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@InstitutionId", filter.InstitutionId);
            parameters.Add("@GradeId", filter.GradeId);
            parameters.Add("@Section", string.IsNullOrWhiteSpace(filter.Section) ? null : filter.Section.Trim());
            parameters.Add("@FromDate", filter.FromDate.Date);
            parameters.Add("@ToDate", filter.ToDate.Date);

            return await _db.Connection.QueryAsync<StudentAttendanceSummaryReportDTO>(
                "dbo.usp_StudentAttendanceSummaryReport",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<AppGradeSectionDTO>> GetGradeSectionsByInstitutionId(int institutionId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@InstitutionId", institutionId);

            return await _db.Connection.QueryAsync<AppGradeSectionDTO>(
                "dbo.usp_GetGradeSectionsByInstitutionId",
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure);
        }
    }
}

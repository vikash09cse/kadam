using Core.Abstractions;
using Core.DTOs.App;
using Core.Entities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;

namespace Infrastructure
{
    public class StudentBaselineDetailRepository(DatabaseContext context) : IStudentBaselineDetailRepository
    {
        private readonly DatabaseContext _context = context;

        public async Task<StudentBaselineDetail> GetStudentBaselineDetail(int id)
        {
            return await _context.StudentBaselineDetails.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted) ?? new StudentBaselineDetail();
        }

        public async Task<IEnumerable<StudentBaselineDetail>> GetAllStudentBaselineDetails()
        {
            return await _context.StudentBaselineDetails.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<StudentBaselineDetail>> GetStudentBaselineDetailsByStudentId(int studentId)
        {
            return await _context.StudentBaselineDetails.Where(x => x.StudentId == studentId && !x.IsDeleted).ToListAsync();
        }

        public async Task<bool> SaveStudentBaselineDetail(StudentBaselineDetail studentBaselineDetail)
        {
            if (studentBaselineDetail.Id>0)
            {
                _context.StudentBaselineDetails.Update(studentBaselineDetail);
            }
            else
            {
                _context.StudentBaselineDetails.Add(studentBaselineDetail);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteStudentBaselineDetail(int id, int deletedBy)
        {
            var studentBaselineDetail = await _context.StudentBaselineDetails.FirstOrDefaultAsync(x => x.Id == id);
            if (studentBaselineDetail != null)
            {
                studentBaselineDetail.IsDeleted = true;
                studentBaselineDetail.DeletedBy = deletedBy;
                studentBaselineDetail.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<IEnumerable<StudentBaselineDetailWithSubjectDTO>> GetStudentBaselineDetailWithSubjects(int studentId, string baselineType)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@StudentId", studentId);
                parameters.Add("@BaselineType", baselineType);

                var result = await connection.QueryAsync<StudentBaselineDetailWithSubjectDTO>(
                    "usp_GetStudentBaselineDetailWithSubjects",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }

        public async Task<bool> SaveStudentBaselineDetail(StudentBaselineDetailWithSubjectSaveDTO studentBaselineDetail)
        {
            using (var connection = _context.Database.GetDbConnection())
            {

                var studentBaselineDetails = JsonConvert.SerializeObject(studentBaselineDetail.StudentBaselineDetails);

                var parameters = new DynamicParameters();
                parameters.Add("@StudentId", studentBaselineDetail.StudentId);
                parameters.Add("@CreatedBy", studentBaselineDetail.CreatedBy);
                parameters.Add("@BaselineType", studentBaselineDetail.BaselineType);
                parameters.Add("@BaselineDetails", studentBaselineDetails);

                var result = await connection.QueryAsync<int>(
                    "usp_SaveStudentBaseline",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result.FirstOrDefault()==1;
            }
        }

        public async Task<(bool Success, string Message)> UpdateBaselineCompletedDate(UpdateBaselineCompletedDateDTO model)
        {
            var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == model.StudentId && !x.IsDeleted);
            if (student is null)
                return (false, "Student was not found.");
            if (student.CurrentStatus == Core.Utilities.Enums.Status.Closed)
                return (false, "Completed students are read-only.");

            var date = model.CompletedDate.Date;
            if (date < student.EnrollmentDate.Date)
                return (false, "Baseline date cannot be before enrollment.");
            if (date > DateTime.Today)
                return (false, "Baseline date cannot be in the future.");

            var details = await _context.StudentBaselineDetails
                .Where(x => x.StudentId == model.StudentId && !x.IsDeleted &&
                            x.BaselineType == "baselinepreAssessment")
                .ToListAsync();
            if (details.Count == 0)
                return (false, "Baseline has not been saved yet.");

            var endlineDates = await _context.StudentBaselineDetails
                .Where(x => x.StudentId == model.StudentId && !x.IsDeleted &&
                            x.BaselineType == "endlinepreAssessment" && x.CompletedDate.HasValue)
                .Select(x => x.CompletedDate!.Value)
                .ToListAsync();
            if (endlineDates.Count > 0 && date > endlineDates.Max().Date)
                return (false, "Baseline date cannot be after the endline date.");

            foreach (var detail in details)
            {
                detail.CompletedDate = date;
                detail.ModifyBy = model.CreatedBy;
                detail.ModifyDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return (true, "Baseline date updated successfully.");
        }

    }
} 
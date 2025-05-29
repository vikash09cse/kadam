using Core.Abstractions;
using Core.DTOs.App;
using Core.Entities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;

namespace Infrastructure
{
    public class StudentProgressRepository(IDbSession db) : IStudentProgressRepository
    {
        private readonly IDbSession _db = db;

        public async Task<StudentProgressDTO> GetStudentProgressDetail(int studentId)
        {

            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentId);

            using var multi = await _db.Connection.QueryMultipleAsync("usp_GetStudentProgressDetail", parameters, commandType: CommandType.StoredProcedure);

            var studentProgress = await multi.ReadFirstOrDefaultAsync<StudentProgressDTO>();
            var gradeCompletedLevels = await multi.ReadAsync<int>();
            var steps = await multi.ReadAsync<StudentGradeStepProgressDTO>();



            IList<StudentGradeLevelProgressDTO> gradeLevels = [];
            if (studentProgress != null)
            {
                foreach (var step in steps)
                {
                    if (step.StepId == studentProgress.EntryStepId)
                    {
                        step.BackgroundColor = "#6e33bb";
                    }
                    if (step.StepId == studentProgress.ExitStepId)
                    {
                        step.BackgroundColor = "#b1a40e";
                    }

                    if (step.StepId > studentProgress.EntryStepId && step.StepId < studentProgress.ExitStepId && step.IsCompleted)
                    {
                        step.BackgroundColor = "#11a015";
                    }
                }

                gradeLevels.Add(new StudentGradeLevelProgressDTO
                {
                    GradeLevelId = 1,
                    GradeLevelText = "Grade Test 1",
                    IsCompleted = gradeCompletedLevels.Contains(1),
                    IsEnabled = studentProgress.EntryStepId == 1 || studentProgress.ExitStepId == 1,
                    Steps = steps.Where(x => x.StepId == 1 || x.StepId == 2)
                });
                gradeLevels.Add(new StudentGradeLevelProgressDTO
                {
                    GradeLevelId = 2,
                    GradeLevelText = "Grade Test 2",
                    IsCompleted = gradeCompletedLevels.Contains(2),
                    IsEnabled = studentProgress.EntryStepId == 2 || studentProgress.ExitStepId == 2,
                    Steps = steps.Where(x => x.StepId == 3 || x.StepId == 4)
                });
                gradeLevels.Add(new StudentGradeLevelProgressDTO
                {
                    GradeLevelId = 3,
                    GradeLevelText = "Grade Test 3",
                    IsCompleted = gradeCompletedLevels.Contains(3),
                    IsEnabled = studentProgress.EntryStepId == 3 || studentProgress.ExitStepId == 3,
                    Steps = steps.Where(x => x.StepId == 5 || x.StepId == 6)
                });
                gradeLevels.Add(new StudentGradeLevelProgressDTO
                {
                    GradeLevelId = 4,
                    GradeLevelText = "Grade Test 4",
                    IsCompleted = gradeCompletedLevels.Contains(4),
                    IsEnabled = studentProgress.EntryStepId == 4 || studentProgress.ExitStepId == 4,
                    Steps = steps.Where(x => x.StepId == 7 || x.StepId == 8)
                });
                gradeLevels.Add(new StudentGradeLevelProgressDTO
                {
                    GradeLevelId = 5,
                    GradeLevelText = "Grade Test 5",
                    IsCompleted = gradeCompletedLevels.Contains(5),
                    IsEnabled = studentProgress.EntryStepId == 5 || studentProgress.ExitStepId == 5,
                    Steps = steps.Where(x => x.StepId == 9 || x.StepId == 10)
                });

                foreach (var gradeLevel in gradeLevels)
                {
                    if (gradeLevel.GradeLevelId == studentProgress.GradeEntryLevelId && !gradeLevel.IsCompleted)
                    {
                        gradeLevel.BackgroundColor = "#77021d";
                    }
                    else if (gradeLevel.IsCompleted)
                    {

                        gradeLevel.BackgroundColor = "#11a015";
                    }
                }

                studentProgress.GradeLevelProgress = gradeLevels;
            }
            return studentProgress;

        }

        public async Task<IEnumerable<StudentBaselineDetailWithSubjectDTO>> GetStudentBaselineDetailWithSubjects(int studentId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentId);

            var result = await _db.Connection.QueryAsync<StudentBaselineDetailWithSubjectDTO>(
                "usp_GetStudentBaselineDetailWithSubjects",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result;
        }
        public async Task<bool> SaveStudentProgress(StudentProgressStep studentProgress)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentProgress.StudentId);
            parameters.Add("@StepId", studentProgress.StepId);
            parameters.Add("@IsCompleted", studentProgress.IsCompleted);
            parameters.Add("@CreatedBy", studentProgress.CreatedBy);

            try
            {
                await _db.Connection.ExecuteAsync("usp_StudentProgressStep_Save", parameters, commandType: CommandType.StoredProcedure);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving student progress: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SaveStudentGradeTestDetail(StudentGradeTestDetailSaveDTO studentGradeTestDetail)
        {
            try
            {
                var testDetails = JsonConvert.SerializeObject(studentGradeTestDetail.StudentGradeTestDetails);

                var parameters = new DynamicParameters();
                parameters.Add("@StudentId", studentGradeTestDetail.StudentId);
                parameters.Add("@GradeLevelId", studentGradeTestDetail.GradeLevelId);
                parameters.Add("@CreatedBy", studentGradeTestDetail.CreatedBy);
                parameters.Add("@TestDetails", testDetails);

                var result = await _db.Connection.QueryAsync<int>(
                    "usp_SaveStudentGRadeTestDetail",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result.FirstOrDefault() == 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving student grade test detail: {ex.Message}");
                return false;
            }
        }
    }
}

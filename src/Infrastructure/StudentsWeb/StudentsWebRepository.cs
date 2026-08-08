using Core.Abstractions;
using Core.Entities;
using Core.Features.StudentsWeb;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.StudentsWeb;

public sealed class StudentsWebRepository(IDbSession db, DatabaseContext context) : IStudentsWebRepository
{
    public async Task<bool> HasPageAccess(int userId, string pageUrl)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@PageUrl", pageUrl);
        return await db.Connection.ExecuteScalarAsync<bool>(
            "dbo.usp_WebStudents_HasPageAccess", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<StudentsWebDashboardDTO> GetDashboard(int userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        return await db.Connection.QuerySingleAsync<StudentsWebDashboardDTO>(
            "dbo.usp_WebStudents_GetDashboard", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IReadOnlyList<StudentsWebListItemDTO>> GetStudents(
        int userId, int pageNumber, int pageSize, string? studentName, string? studentId, string? aadhaarNumber, int? status)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@PageNumber", pageNumber);
        parameters.Add("@PageSize", pageSize);
        parameters.Add("@StudentName", NullIfWhiteSpace(studentName));
        parameters.Add("@StudentId", NullIfWhiteSpace(studentId));
        parameters.Add("@AadhaarNumber", NullIfWhiteSpace(aadhaarNumber));
        parameters.Add("@Status", status);
        var result = await db.Connection.QueryAsync<StudentsWebListItemDTO>(
            "dbo.usp_WebStudents_GetStudents", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
        return result.AsList();
    }

    public async Task<IReadOnlyList<StudentsWebInstitutionStudentDTO>> GetInstitutionStudents(
        int userId, int pageNumber, int pageSize, string? searchText, int? institutionId,
        int? gradeId, string? section, DateTime? fromDate, DateTime? toDate, int? status)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@PageNumber", pageNumber);
        parameters.Add("@PageSize", pageSize);
        parameters.Add("@SearchText", NullIfWhiteSpace(searchText));
        parameters.Add("@InstitutionId", institutionId);
        parameters.Add("@GradeId", gradeId);
        parameters.Add("@Section", NullIfWhiteSpace(section));
        parameters.Add("@FromDate", fromDate?.Date);
        parameters.Add("@ToDate", toDate?.Date);
        parameters.Add("@Status", status);
        var result = await db.Connection.QueryAsync<StudentsWebInstitutionStudentDTO>(
            "dbo.usp_WebStudents_GetInstitutionStudents", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
        return result.AsList();
    }

    public async Task<StudentsWebEditDTO?> GetStudent(int id, int userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        parameters.Add("@UserId", userId);
        return await db.Connection.QuerySingleOrDefaultAsync<StudentsWebEditDTO>(
            "dbo.usp_WebStudents_GetStudent", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IReadOnlyList<StudentsWebLookupDTO>> GetInstitutions(int userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        var result = await db.Connection.QueryAsync<StudentsWebLookupDTO>(
            "dbo.usp_WebStudents_GetInstitutions", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
        return result.AsList();
    }

    public async Task<IReadOnlyList<StudentsWebLookupDTO>> GetGradeSections(int institutionId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@InstitutionId", institutionId);
        var result = await db.Connection.QueryAsync<StudentsWebLookupDTO>(
            "dbo.usp_WebStudents_GetGradeSections", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
        return result.AsList();
    }

    public async Task<StudentsWebAssessmentDTO?> GetAssessment(
        int studentId, StudentsWebAssessmentKind kind, int userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@StudentId", studentId);
        parameters.Add("@UserId", userId);
        parameters.Add("@AssessmentType", kind == StudentsWebAssessmentKind.Baseline
            ? "baselinepreAssessment"
            : "endlinepreAssessment");

        using var result = await db.Connection.QueryMultipleAsync(
            "dbo.usp_WebStudents_GetAssessment", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
        var assessment = await result.ReadSingleOrDefaultAsync<StudentsWebAssessmentDTO>();
        if (assessment is null) return null;
        assessment.Subjects = (await result.ReadAsync<StudentsWebAssessmentSubjectDTO>()).AsList();
        return assessment;
    }

    public async Task<StudentsWebAssessmentSaveStatus> SaveBaseline(
        StudentsWebAssessmentSaveDTO model, StudentsWebAssessmentDTO assessment, int userId)
    {
        if (!await CanAccessStudent(model.StudentId, userId))
            return StudentsWebAssessmentSaveStatus.NotAuthorizedOrNotFound;

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var student = await context.Students.FirstOrDefaultAsync(x => x.Id == model.StudentId && !x.IsDeleted);
            if (student is null)
                return StudentsWebAssessmentSaveStatus.NotAuthorizedOrNotFound;
            if (student.CurrentStatus == Core.Utilities.Enums.Status.Closed ||
                await context.StudentProgressSteps.AnyAsync(x => x.StudentId == model.StudentId && x.IsCompleted))
                return StudentsWebAssessmentSaveStatus.Locked;
            if (student.GradeId != assessment.GradeId || student.Age != assessment.Age ||
                student.IsKadamPlusStudent != assessment.IsKadamPlusStudent)
                return StudentsWebAssessmentSaveStatus.InvalidSubjects;

            var subjects = await context.Subjects
                .Where(x => !x.IsDeleted && x.CurrentStatus == Core.Utilities.Enums.Status.Active)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
            var submittedIds = model.Scores.Select(x => x.SubjectId).OrderBy(x => x).ToArray();
            var subjectIds = subjects.Select(x => x.Id).OrderBy(x => x).ToArray();
            if (submittedIds.Length != submittedIds.Distinct().Count() ||
                !submittedIds.SequenceEqual(subjectIds))
                return StudentsWebAssessmentSaveStatus.InvalidSubjects;

            var existing = await context.StudentBaselineDetails
                .Where(x => x.StudentId == model.StudentId && !x.IsDeleted &&
                            x.BaselineType == "baselinepreAssessment")
                .ToListAsync();
            foreach (var obsolete in existing.Where(x => !subjectIds.Contains(x.SubjectId)))
            {
                obsolete.IsDeleted = true;
                obsolete.DeletedBy = userId;
                obsolete.DeletedDate = DateTime.UtcNow;
                obsolete.DateEntryPoint = StudentsWebEntryPoint.Web;
            }
            var totalMarks = StudentsWebAssessmentCalculator.GetTotalMarks(
                assessment.IsKadamPlusStudent, assessment.GradeName, assessment.Age);

            foreach (var score in model.Scores)
            {
                var detail = existing.FirstOrDefault(x => x.SubjectId == score.SubjectId);
                if (detail is null)
                {
                    detail = new StudentBaselineDetail
                    {
                        StudentId = model.StudentId,
                        SubjectId = score.SubjectId,
                        BaselineType = "baselinepreAssessment",
                        CreatedBy = userId,
                        DateCreated = DateTime.UtcNow
                    };
                    context.StudentBaselineDetails.Add(detail);
                }
                else
                {
                    detail.ModifyBy = userId;
                    detail.ModifyDate = DateTime.UtcNow;
                }

                detail.StudentAge = assessment.Age;
                detail.ObtainedMarks = score.ObtainedMarks;
                detail.TotalMarks = totalMarks;
                detail.PercentageMarks = score.ObtainedMarks.HasValue
                    ? StudentsWebAssessmentCalculator.GetPercentage(score.ObtainedMarks.Value, totalMarks)
                    : null;
                detail.CompletedDate = model.CompletedDate!.Value.Date;
                detail.CurrentStatus = Core.Utilities.Enums.Status.Active;
                detail.DateEntryPoint = StudentsWebEntryPoint.Web;
            }

            var placementValue = StudentsWebAssessmentCalculator.GetPlacement(
                assessment.IsKadamPlusStudent,
                assessment.GradeName,
                assessment.Age,
                model.Scores.Select(x => x.ObtainedMarks ?? 0m));
            var placement = await context.StudentGradeStartAndEndDetails
                .FirstOrDefaultAsync(x => x.StudentId == model.StudentId);
            if (placement is null)
            {
                placement = new StudentGradeStartAndEndDetail
                {
                    StudentId = model.StudentId,
                    CreatedBy = userId,
                    DateCreated = DateTime.UtcNow
                };
                context.StudentGradeStartAndEndDetails.Add(placement);
            }
            else
            {
                placement.ModifyBy = userId;
                placement.ModifyDate = DateTime.UtcNow;
            }
            placement.GradeEntryLevelId = placementValue.GradeEntryLevelId;
            placement.GradeExitLevelId = placementValue.GradeExitLevelId;
            placement.EntryStepId = placementValue.EntryStepId;
            placement.ExitStepId = placementValue.ExitStepId;
            placement.DateEntryPoint = StudentsWebEntryPoint.Web;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return StudentsWebAssessmentSaveStatus.Saved;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<StudentsWebAssessmentSaveStatus> SaveEndline(
        StudentsWebAssessmentSaveDTO model, StudentsWebAssessmentDTO assessment, int userId)
    {
        if (!await CanAccessStudent(model.StudentId, userId))
            return StudentsWebAssessmentSaveStatus.NotAuthorizedOrNotFound;

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var student = await context.Students.FirstOrDefaultAsync(x => x.Id == model.StudentId && !x.IsDeleted);
            if (student is null)
                return StudentsWebAssessmentSaveStatus.NotAuthorizedOrNotFound;
            if (student.CurrentStatus == Core.Utilities.Enums.Status.Closed)
                return StudentsWebAssessmentSaveStatus.Locked;

            var baselineDate = await context.StudentBaselineDetails
                .Where(x => x.StudentId == model.StudentId && !x.IsDeleted &&
                            x.BaselineType == "baselinepreAssessment")
                .MaxAsync(x => x.CompletedDate);
            if (!baselineDate.HasValue ||
                await context.StudentBaselineDetails.AnyAsync(x =>
                    x.StudentId == model.StudentId && !x.IsDeleted &&
                    x.BaselineType == "endlinepreAssessment"))
                return StudentsWebAssessmentSaveStatus.Locked;
            if (model.CompletedDate!.Value.Date < baselineDate.Value.Date)
                return StudentsWebAssessmentSaveStatus.Locked;
            if (student.GradeId != assessment.GradeId || student.Age != assessment.Age ||
                student.IsKadamPlusStudent != assessment.IsKadamPlusStudent)
                return StudentsWebAssessmentSaveStatus.InvalidSubjects;

            var subjectIds = await context.Subjects
                .Where(x => !x.IsDeleted && x.CurrentStatus == Core.Utilities.Enums.Status.Active)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.Id)
                .ToArrayAsync();
            var submittedIds = model.Scores.Select(x => x.SubjectId).OrderBy(x => x).ToArray();
            if (submittedIds.Length != submittedIds.Distinct().Count() ||
                !submittedIds.SequenceEqual(subjectIds.OrderBy(x => x)))
                return StudentsWebAssessmentSaveStatus.InvalidSubjects;

            var totalMarks = StudentsWebAssessmentCalculator.GetTotalMarks(
                assessment.IsKadamPlusStudent, assessment.GradeName, assessment.Age);
            foreach (var score in model.Scores)
            {
                context.StudentBaselineDetails.Add(new StudentBaselineDetail
                {
                    StudentId = model.StudentId,
                    SubjectId = score.SubjectId,
                    StudentAge = assessment.Age,
                    BaselineType = "endlinepreAssessment",
                    ObtainedMarks = score.ObtainedMarks,
                    TotalMarks = totalMarks,
                    PercentageMarks = score.ObtainedMarks.HasValue
                        ? StudentsWebAssessmentCalculator.GetPercentage(score.ObtainedMarks.Value, totalMarks)
                        : null,
                    CompletedDate = model.CompletedDate.Value.Date,
                    CurrentStatus = Core.Utilities.Enums.Status.Active,
                    CreatedBy = userId,
                    DateCreated = DateTime.UtcNow,
                    DateEntryPoint = StudentsWebEntryPoint.Web
                });
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return StudentsWebAssessmentSaveStatus.Saved;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<StudentsWebProgressDTO?> GetProgress(int studentId, int userId)
    {
        var baseline = await GetAssessment(studentId, StudentsWebAssessmentKind.Baseline, userId);
        if (baseline is null || !baseline.CompletedDate.HasValue) return null;

        var parameters = new DynamicParameters();
        parameters.Add("@StudentId", studentId);
        using var result = await db.Connection.QueryMultipleAsync(
            "dbo.usp_WebStudents_GetProgress", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
        var progress = await result.ReadSingleOrDefaultAsync<StudentsWebProgressDTO>();
        if (progress is null) return null;
        progress.Steps = (await result.ReadAsync<StudentsWebProgressStepDTO>()).AsList();
        progress.CompletedGradeLevels = (await result.ReadAsync<int>()).AsList();
        return progress;
    }

    public async Task<StudentsWebProgressSaveStatus> CompleteProgressStep(
        int studentId, int stepId, int userId)
    {
        if (!await CanAccessStudent(studentId, userId))
            return StudentsWebProgressSaveStatus.NotAuthorizedOrNotFound;

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var student = await context.Students.FirstOrDefaultAsync(x => x.Id == studentId && !x.IsDeleted);
            var level = await context.StudentGradeStartAndEndDetails.FirstOrDefaultAsync(x => x.StudentId == studentId);
            var hasBaseline = await context.StudentBaselineDetails.AnyAsync(x =>
                x.StudentId == studentId && !x.IsDeleted &&
                x.BaselineType == "baselinepreAssessment" && x.CompletedDate.HasValue);
            if (student is null)
                return StudentsWebProgressSaveStatus.NotAuthorizedOrNotFound;
            if (!hasBaseline || level is null)
                return StudentsWebProgressSaveStatus.BaselineRequired;
            if (student.CurrentStatus == Core.Utilities.Enums.Status.Closed ||
                stepId < level.EntryStepId || stepId > level.ExitStepId)
                return StudentsWebProgressSaveStatus.OutOfRange;

            var existing = await context.StudentProgressSteps
                .FirstOrDefaultAsync(x => x.StudentId == studentId && x.StepId == stepId);
            if (existing?.IsCompleted == true)
                return StudentsWebProgressSaveStatus.Saved;
            if (stepId > level.EntryStepId &&
                !await context.StudentProgressSteps.AnyAsync(x =>
                    x.StudentId == studentId && x.StepId == stepId - 1 && x.IsCompleted))
                return StudentsWebProgressSaveStatus.PreviousStepRequired;

            if (stepId > level.EntryStepId && stepId % 2 == 1)
            {
                var previousGradeLevel = (stepId - 1) / 2;
                if (!await context.StudentGradeTestDetails.AnyAsync(x =>
                    x.StudentId == studentId && x.GradeLevelId == previousGradeLevel && !x.IsDeleted))
                    return StudentsWebProgressSaveStatus.PreviousGradeTestRequired;
            }

            if (existing is null)
            {
                context.StudentProgressSteps.Add(new StudentProgressStep
                {
                    StudentId = studentId,
                    StepId = stepId,
                    IsCompleted = true,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                    DateEntryPoint = StudentsWebEntryPoint.Web
                });
            }
            else
            {
                existing.IsCompleted = true;
                existing.DateEntryPoint = StudentsWebEntryPoint.Web;
            }
            level.LastCompletedStepId = Math.Max(level.LastCompletedStepId ?? 0, stepId);
            level.ModifyBy = userId;
            level.ModifyDate = DateTime.UtcNow;
            level.DateEntryPoint = StudentsWebEntryPoint.Web;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return StudentsWebProgressSaveStatus.Saved;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<StudentsWebGradeTestDTO?> GetGradeTest(int studentId, int gradeLevelId, int userId)
    {
        if (await GetProgress(studentId, userId) is null) return null;
        var parameters = new DynamicParameters();
        parameters.Add("@StudentId", studentId);
        parameters.Add("@GradeLevelId", gradeLevelId);
        using var result = await db.Connection.QueryMultipleAsync(
            "dbo.usp_WebStudents_GetGradeTest", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
        var gradeTest = await result.ReadSingleOrDefaultAsync<StudentsWebGradeTestDTO>();
        if (gradeTest is null) return null;
        gradeTest.Subjects = (await result.ReadAsync<StudentsWebAssessmentSubjectDTO>()).AsList();
        return gradeTest;
    }

    public async Task<StudentsWebGradeTestSaveStatus> SaveGradeTest(
        StudentsWebGradeTestSaveDTO model, StudentsWebGradeTestDTO gradeTest, int userId)
    {
        if (!await CanAccessStudent(model.StudentId, userId))
            return StudentsWebGradeTestSaveStatus.NotAuthorizedOrNotFound;

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var level = await context.StudentGradeStartAndEndDetails
                .FirstOrDefaultAsync(x => x.StudentId == model.StudentId);
            if (level is null || model.GradeLevelId < level.GradeEntryLevelId ||
                model.GradeLevelId > level.GradeExitLevelId)
                return StudentsWebGradeTestSaveStatus.StepsIncomplete;

            var firstStep = (model.GradeLevelId * 2) - 1;
            var completedSteps = await context.StudentProgressSteps.CountAsync(x =>
                x.StudentId == model.StudentId && x.IsCompleted &&
                (x.StepId == firstStep || x.StepId == firstStep + 1));
            if (completedSteps < 2)
                return StudentsWebGradeTestSaveStatus.StepsIncomplete;
            if (model.GradeLevelId > level.GradeEntryLevelId &&
                (!gradeTest.PreviousGradePercentage.HasValue || gradeTest.PreviousGradePercentage < 80))
                return StudentsWebGradeTestSaveStatus.PreviousGradeScoreRequired;

            var subjectIds = await context.Subjects
                .Where(x => !x.IsDeleted && x.CurrentStatus == Core.Utilities.Enums.Status.Active)
                .Select(x => x.Id).OrderBy(x => x).ToArrayAsync();
            var submittedIds = model.Scores.Select(x => x.SubjectId).OrderBy(x => x).ToArray();
            if (!submittedIds.SequenceEqual(subjectIds) ||
                submittedIds.Length != submittedIds.Distinct().Count())
                return StudentsWebGradeTestSaveStatus.InvalidSubjects;

            var existing = await context.StudentGradeTestDetails.Where(x =>
                x.StudentId == model.StudentId && x.GradeLevelId == model.GradeLevelId && !x.IsDeleted)
                .ToListAsync();
            var totals = gradeTest.Subjects.ToDictionary(x => x.SubjectId, x => x.TotalMarks);
            foreach (var score in model.Scores)
            {
                var detail = existing.FirstOrDefault(x => x.SubjectId == score.SubjectId);
                if (detail is null)
                {
                    detail = new StudentGradeTestDetail
                    {
                        StudentId = model.StudentId,
                        GradeLevelId = model.GradeLevelId,
                        SubjectId = score.SubjectId,
                        CreatedBy = userId,
                        DateCreated = DateTime.UtcNow
                    };
                    context.StudentGradeTestDetails.Add(detail);
                }
                else
                {
                    detail.ModifyBy = userId;
                    detail.ModifyDate = DateTime.UtcNow;
                }
                detail.ObtainedMarks = score.ObtainedMarks;
                detail.TotalMarks = totals[score.SubjectId];
                detail.PercentageMarks = StudentsWebAssessmentCalculator.GetPercentage(
                    score.ObtainedMarks!.Value, totals[score.SubjectId]);
                detail.CompletedDate = model.CompletedDate!.Value.Date;
                detail.CurrentStatus = Core.Utilities.Enums.Status.Active;
                detail.DateEntryPoint = StudentsWebEntryPoint.Web;
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return StudentsWebGradeTestSaveStatus.Saved;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<StudentsWebHealthDTO?> GetHealth(int studentId, int userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@StudentId", studentId);
        parameters.Add("@UserId", userId);
        return await db.Connection.QuerySingleOrDefaultAsync<StudentsWebHealthDTO>(
            "dbo.usp_WebStudents_GetHealth", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IReadOnlyList<StudentsWebDocumentDTO>> GetDocuments(int studentId, int userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@StudentId", studentId);
        parameters.Add("@UserId", userId);
        var result = await db.Connection.QueryAsync<StudentsWebDocumentDTO>(
            "dbo.usp_WebStudents_GetDocuments", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
        return result.AsList();
    }

    public async Task<bool> StudentRegistrationNumberExists(string registrationNumber, int institutionId, int exceptId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@RegistrationNumber", registrationNumber);
        parameters.Add("@InstitutionId", institutionId);
        parameters.Add("@ExceptId", exceptId);
        return await db.Connection.ExecuteScalarAsync<bool>(
            "dbo.usp_WebStudents_RegistrationNumberExists", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> AadhaarExists(string aadhaarNumber, int exceptId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@AadhaarNumber", aadhaarNumber);
        parameters.Add("@ExceptId", exceptId);
        return await db.Connection.ExecuteScalarAsync<bool>(
            "dbo.usp_WebStudents_AadhaarExists", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> SaveStudent(StudentsWebEditDTO model, int userId)
    {
        if (model.Id > 0 && !await CanAccessStudent(model.Id, userId))
            return 0;
        if (!await CanAccessInstitution(model.InstitutionId, userId))
            return 0;

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            Student student;
            if (model.Id > 0)
            {
                student = await context.Students.FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted)
                    ?? throw new InvalidOperationException("Student was not found.");
                student.ModifyBy = userId;
                student.ModifyDate = DateTime.UtcNow;
            }
            else
            {
                student = new Student
                {
                    CreatedBy = userId,
                    DateCreated = DateTime.UtcNow,
                    CurrentStatus = Core.Utilities.Enums.Status.Active
                };
                context.Students.Add(student);
            }

            MapStudent(model, student);
            student.DateEntryPoint = StudentsWebEntryPoint.Web;
            await context.SaveChangesAsync();

            var family = await context.StudentFamilyDetails
                .FirstOrDefaultAsync(x => x.StudentId == student.Id && !x.IsDeleted);
            if (family is null)
            {
                family = new StudentFamilyDetail
                {
                    StudentId = student.Id,
                    CreatedBy = userId,
                    DateCreated = DateTime.UtcNow,
                    CurrentStatus = Core.Utilities.Enums.Status.Active
                };
                context.StudentFamilyDetails.Add(family);
            }
            else
            {
                family.ModifyBy = userId;
                family.ModifyDate = DateTime.UtcNow;
            }

            MapFamily(model, family);
            family.DateEntryPoint = StudentsWebEntryPoint.Web;

            if (string.IsNullOrWhiteSpace(student.StudentId))
                student.StudentId = await BuildStudentId(student);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return student.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateStatus(StudentsWebStatusDTO model, int userId)
    {
        if (model.Status is < 1 or > 2) return false;
        if (!await CanAccessStudent(model.StudentId, userId)) return false;
        var student = await context.Students.FirstOrDefaultAsync(x => x.Id == model.StudentId && !x.IsDeleted);
        if (student is null) return false;
        student.CurrentStatus = (Core.Utilities.Enums.Status)model.Status;
        student.InActiveReason = model.Status == 2 ? model.InActiveReason.Trim() : null;
        student.InActiveDate = model.Status == 2 ? model.InActiveDate : null;
        student.Remarks = NullIfWhiteSpace(model.Remarks);
        student.ModifyBy = userId;
        student.ModifyDate = DateTime.UtcNow;
        student.DateEntryPoint = StudentsWebEntryPoint.Web;
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<StudentsWebTrioSaveStatus> SaveTrio(int studentId, int trioId, int userId)
    {
        if (!await CanAccessStudent(studentId, userId))
            return StudentsWebTrioSaveStatus.NotAuthorizedOrNotFound;

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var student = await context.Students.FirstOrDefaultAsync(x => x.Id == studentId && !x.IsDeleted);
            if (student is null)
                return StudentsWebTrioSaveStatus.NotAuthorizedOrNotFound;
            if (!await context.StudentBaselineDetails.AnyAsync(x => x.StudentId == studentId && !x.IsDeleted))
                return StudentsWebTrioSaveStatus.BaselineRequired;

            var trio = await context.StudentTrios.FirstOrDefaultAsync(x => x.StudentId == studentId && !x.IsDeleted);
            if (trio?.TrioId == trioId)
                return StudentsWebTrioSaveStatus.Saved;

            var matchingAssignments =
                from assignment in context.StudentTrios
                join assignedStudent in context.Students on assignment.StudentId equals assignedStudent.Id
                where !assignment.IsDeleted && !assignedStudent.IsDeleted &&
                      assignment.StudentId != studentId &&
                      assignedStudent.InstitutionId == student.InstitutionId &&
                      assignedStudent.GradeId == student.GradeId
                select assignment;
            var destinationCount = await matchingAssignments.CountAsync(x => x.TrioId == trioId);
            var trioCounts = await matchingAssignments
                .GroupBy(x => x.TrioId)
                .Select(group => group.Count())
                .ToListAsync();
            var fullTrioCount = trioCounts.Count(count => count >= 4);
            if (destinationCount >= 4 || (destinationCount == 3 && fullTrioCount >= 2))
                return StudentsWebTrioSaveStatus.CapacityReached;

            if (trio is null)
            {
                trio = new StudentTrio { StudentId = studentId, CreatedBy = userId, DateCreated = DateTime.UtcNow };
                context.StudentTrios.Add(trio);
            }
            else
            {
                trio.ModifyBy = userId;
                trio.ModifyDate = DateTime.UtcNow;
            }
            trio.TrioId = trioId;
            trio.DateEntryPoint = StudentsWebEntryPoint.Web;
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return StudentsWebTrioSaveStatus.Saved;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<StudentsWebMainstreamDTO?> GetMainstream(int studentId, int userId)
    {
        if (!await CanAccessStudent(studentId, userId)) return null;

        var parameters = new DynamicParameters();
        parameters.Add("@StudentId", studentId);
        return await db.Connection.QuerySingleOrDefaultAsync<StudentsWebMainstreamDTO>(
            "dbo.usp_WebStudents_GetMainstream", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<StudentsWebPromotionDTO?> GetPromotion(int studentId, int userId)
    {
        var student = await GetStudent(studentId, userId);
        if (student is null) return null;
        var grades = await GetGradeSections(student.InstitutionId);
        return new StudentsWebPromotionDTO
        {
            StudentId = student.Id,
            StudentName = $"{student.FirstName} {student.LastName}".Trim(),
            StudentCode = student.StudentId ?? string.Empty,
            Age = student.Age,
            InstitutionId = student.InstitutionId,
            CurrentGradeId = student.GradeId,
            CurrentGradeName = grades.FirstOrDefault(x => x.Id == student.GradeId)?.Text ?? string.Empty,
            CurrentSection = student.Section,
            DestinationGradeId = student.GradeId,
            DestinationSection = student.Section,
            EnrollmentDate = student.EnrollmentDate,
            PromotionDate = DateTime.Today,
            IsEligible = student.IsKadamPlusStudent &&
                         student.CurrentStatus != (int)Core.Utilities.Enums.Status.Closed,
            Grades = grades
        };
    }

    public async Task<StudentsWebPromotionSaveStatus> PromoteStudent(
        StudentsWebPromotionDTO model, int userId)
    {
        if (!await CanAccessStudent(model.StudentId, userId))
            return StudentsWebPromotionSaveStatus.NotAuthorizedOrNotFound;

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var student = await context.Students.FirstOrDefaultAsync(x => x.Id == model.StudentId && !x.IsDeleted);
            if (student is null)
                return StudentsWebPromotionSaveStatus.NotAuthorizedOrNotFound;
            if (!student.IsKadamPlusStudent ||
                student.CurrentStatus == Core.Utilities.Enums.Status.Closed ||
                await context.StudentMainstreams.AnyAsync(x => x.StudentId == student.Id))
                return StudentsWebPromotionSaveStatus.NotEligible;
            if (model.PromotionDate.Date < student.EnrollmentDate.Date ||
                model.PromotionDate.Date > DateTime.Today)
                return StudentsWebPromotionSaveStatus.InvalidDate;

            var grade = (await GetGradeSections(student.InstitutionId))
                .FirstOrDefault(x => x.Id == model.DestinationGradeId);
            var sections = (grade?.Sections ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (grade is null || model.DestinationGradeId == student.GradeId ||
                !sections.Contains(model.DestinationSection.Trim(), StringComparer.OrdinalIgnoreCase))
                return StudentsWebPromotionSaveStatus.InvalidGradeOrSection;

            student.GradeId = model.DestinationGradeId;
            student.Section = model.DestinationSection.Trim();
            student.PromotionDate = model.PromotionDate.Date;
            student.ModifyBy = userId;
            student.ModifyDate = DateTime.UtcNow;
            student.DateEntryPoint = StudentsWebEntryPoint.Web;
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return StudentsWebPromotionSaveStatus.Saved;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IReadOnlyList<StudentsWebLookupDTO>> GetStates() =>
        await context.States
            .Where(x => !x.IsDeleted && x.CurrentStatus == Core.Utilities.Enums.Status.Active)
            .OrderBy(x => x.StateName)
            .Select(x => new StudentsWebLookupDTO { Id = x.Id, Text = x.StateName })
            .ToListAsync();

    public async Task<IReadOnlyList<StudentsWebLookupDTO>> GetDistricts(int stateId) =>
        await context.Districts
            .Where(x => x.StateId == stateId && !x.IsDeleted &&
                        x.CurrentStatus == Core.Utilities.Enums.Status.Active)
            .OrderBy(x => x.DistrictName)
            .Select(x => new StudentsWebLookupDTO { Id = x.Id, Text = x.DistrictName, ParentId = x.StateId })
            .ToListAsync();

    public async Task<IReadOnlyList<StudentsWebLookupDTO>> GetMainstreamInstitutions(
        int userId, int stateId, int districtId)
    {
        var allowedIds = (await GetInstitutions(userId)).Select(x => x.Id).ToArray();
        return await context.Institutions
            .Where(x => allowedIds.Contains(x.Id) && x.StateId == stateId && x.DistrictId == districtId &&
                        !x.IsDeleted && x.CurrentStatus == Core.Utilities.Enums.Status.Active)
            .OrderBy(x => x.InstitutionName)
            .Select(x => new StudentsWebLookupDTO { Id = x.Id, Text = x.InstitutionName })
            .ToListAsync();
    }

    public async Task<StudentsWebMainstreamSaveStatus> SaveMainstream(
        StudentsWebMainstreamDTO model, int userId)
    {
        if (!await CanAccessStudent(model.StudentId, userId))
            return StudentsWebMainstreamSaveStatus.NotAuthorizedOrNotFound;

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var student = await context.Students.FirstOrDefaultAsync(x => x.Id == model.StudentId && !x.IsDeleted);
            if (student is null)
                return StudentsWebMainstreamSaveStatus.NotAuthorizedOrNotFound;
            if (await context.StudentMainstreams.AnyAsync(x => x.StudentId == model.StudentId))
                return StudentsWebMainstreamSaveStatus.AlreadyMainstreamed;

            var assessments = await context.StudentBaselineDetails
                .Where(x => x.StudentId == model.StudentId && !x.IsDeleted && x.CompletedDate.HasValue)
                .Select(x => new { x.BaselineType, x.CompletedDate })
                .ToListAsync();
            var hasBaseline = assessments.Any(x => x.BaselineType == "baselinepreAssessment");
            var endlineDate = assessments
                .Where(x => x.BaselineType == "endlinepreAssessment")
                .Max(x => x.CompletedDate);
            if (student.IsKadamPlusStudent || student.CurrentStatus == Core.Utilities.Enums.Status.Closed ||
                !hasBaseline || !endlineDate.HasValue || model.MainstreamDate!.Value.Date < endlineDate.Value.Date)
                return StudentsWebMainstreamSaveStatus.NotEligible;

            var institutionId = model.IsMainstreamInstitutionSame
                ? student.InstitutionId
                : model.MainstreamInstitutionId!.Value;
            if (!model.IsMainstreamInstitutionSame)
            {
                var allowedIds = (await GetInstitutions(userId)).Select(x => x.Id).ToArray();
                if (!allowedIds.Contains(institutionId))
                    return StudentsWebMainstreamSaveStatus.InvalidInstitution;
            }

            var institution = await context.Institutions.FirstOrDefaultAsync(x =>
                x.Id == institutionId && !x.IsDeleted &&
                x.CurrentStatus == Core.Utilities.Enums.Status.Active);
            if (institution is null ||
                (!model.IsMainstreamInstitutionSame &&
                 (institution.StateId != model.StateId || institution.DistrictId != model.DistrictId)))
                return StudentsWebMainstreamSaveStatus.InvalidInstitution;

            var gradeSection = (await GetGradeSections(institutionId))
                .FirstOrDefault(x => x.Id == model.GradeId &&
                                     !x.Text.Equals("Kadam STC", StringComparison.OrdinalIgnoreCase));
            var validSections = (gradeSection?.Sections ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (gradeSection is null || !validSections.Contains(model.Section!.Trim(), StringComparer.OrdinalIgnoreCase))
                return StudentsWebMainstreamSaveStatus.InvalidGradeOrSection;

            context.StudentMainstreams.Add(new StudentMainstream
            {
                StudentId = student.Id,
                IsMainstreamInstitutionSame = model.IsMainstreamInstitutionSame,
                StateId = institution.StateId,
                DistrictId = institution.DistrictId,
                MainstreamInstitutionName = institution.InstitutionName,
                SchoolDISECode = institution.InstitutionCode,
                GradeId = model.GradeId,
                Section = model.Section.Trim(),
                ChildSRNumber = NullIfWhiteSpace(model.ChildSRNumber),
                MainstreamDate = model.MainstreamDate.Value.Date,
                CreatedBy = userId,
                DateCreated = DateTime.UtcNow,
                DateEntryPoint = StudentsWebEntryPoint.Web
            });
            student.CurrentStatus = Core.Utilities.Enums.Status.Closed;
            student.InActiveReason = null;
            student.InActiveDate = null;
            student.Remarks = "Student Mainstream Added";
            student.ModifyBy = userId;
            student.ModifyDate = DateTime.UtcNow;
            student.DateEntryPoint = StudentsWebEntryPoint.Web;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return StudentsWebMainstreamSaveStatus.Saved;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> SaveHealth(StudentsWebHealthDTO model, int userId)
    {
        if (!await CanAccessStudent(model.StudentId, userId)) return 0;
        var health = model.Id > 0
            ? await context.StudentHealths.FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted)
            : await context.StudentHealths.FirstOrDefaultAsync(x => x.StudentId == model.StudentId && !x.IsDeleted);
        if (health is null)
        {
            health = new StudentHealth
            {
                StudentId = model.StudentId,
                CreatedBy = userId,
                DateCreated = DateTime.UtcNow,
                CurrentStatus = Core.Utilities.Enums.Status.Active
            };
            context.StudentHealths.Add(health);
        }
        else
        {
            health.ModifyBy = userId;
            health.ModifyDate = DateTime.UtcNow;
        }
        health.PhysicallyChallenged = model.PhysicallyChallenged;
        health.PhysicallyChallengedType = model.PhysicallyChallengedType;
        health.PercentagePhysicallyChallenged = model.PercentagePhysicallyChallenged;
        health.DisabilityCertificatePath = model.DisabilityCertificatePath ?? string.Empty;
        health.DisabilityCertificateFileName = model.DisabilityCertificateFileName ?? string.Empty;
        health.DateEntryPoint = StudentsWebEntryPoint.Web;
        await context.SaveChangesAsync();
        return health.Id;
    }

    public async Task<int> SaveDocument(StudentsWebDocumentDTO model, int userId)
    {
        if (!await CanAccessStudent(model.StudentId, userId)) return 0;
        var document = model.Id > 0
            ? await context.StudentDocuments.FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted)
            : null;
        if (document is null)
        {
            document = new StudentDocument
            {
                StudentId = model.StudentId,
                CreatedBy = userId,
                DateCreated = DateTime.UtcNow,
                CurrentStatus = Core.Utilities.Enums.Status.Active
            };
            context.StudentDocuments.Add(document);
        }
        else
        {
            document.ModifyBy = userId;
            document.ModifyDate = DateTime.UtcNow;
        }
        document.DocumentTypeId = model.DocumentTypeId;
        document.DocumentNumber = model.DocumentNumber.Trim();
        document.DocumentPath = model.DocumentPath ?? string.Empty;
        document.DocumentFileName = model.DocumentFileName ?? string.Empty;
        document.DateEntryPoint = StudentsWebEntryPoint.Web;
        await context.SaveChangesAsync();
        return document.Id;
    }

    public async Task<bool> DeleteDocument(int documentId, int userId)
    {
        var document = await context.StudentDocuments.FirstOrDefaultAsync(x => x.Id == documentId && !x.IsDeleted);
        if (document is null) return false;
        if (!await CanAccessStudent(document.StudentId, userId)) return false;
        StampDeleted(document, userId);
        return await context.SaveChangesAsync() > 0;
    }

    private async Task<string> BuildStudentId(Student student)
    {
        var institution = await context.Institutions.AsNoTracking().FirstAsync(x => x.Id == student.InstitutionId);
        var stateCode = await context.States.AsNoTracking()
            .Where(x => x.Id == institution.StateId).Select(x => x.StateCode).FirstOrDefaultAsync() ?? string.Empty;
        var districtCode = await context.Districts.AsNoTracking()
            .Where(x => x.Id == institution.DistrictId).Select(x => x.DistrictCode).FirstOrDefaultAsync() ?? string.Empty;
        var year = student.EnrollmentDate.Year;
        return $"KP/{stateCode.Trim()}/{districtCode.Trim()}/{year}-{(year + 1) % 100:00}/{student.Id}";
    }

    private static void MapStudent(StudentsWebEditDTO source, Student target)
    {
        target.EnrollmentDate = source.EnrollmentDate.Date;
        target.FirstName = source.FirstName.Trim();
        target.LastName = source.LastName.Trim();
        target.GenderId = source.GenderId;
        target.DateOfBirth = source.DateOfBirth.Date;
        target.Age = source.Age;
        target.DoYouHaveAadhaarCard = source.DoYouHaveAadhaarCard;
        target.AadhaarCardNumber = source.DoYouHaveAadhaarCard
            ? source.AadhaarCardNumber?.Trim() ?? string.Empty
            : string.Empty;
        target.InstitutionId = source.InstitutionId;
        target.GradeId = source.GradeId;
        target.Section = source.Section.Trim();
        target.StudentRegistratioNumber = source.StudentRegistratioNumber.Trim();
        target.ChildStatudBeforeKadamSTC = source.ChildStatudBeforeKadamSTC;
        target.HowLongPlaningToStayThisArea = source.HowLongPlaningToStayThisArea;
        target.Class = source.Class;
        target.ReasonId = source.ReasonId;
        target.DropoutClass = NullIfWhiteSpace(source.DropoutClass);
        target.DropoutYear = source.DropoutYear;
        target.IsKadamPlusStudent = source.IsKadamPlusStudent;
        if (!string.IsNullOrWhiteSpace(source.ProfilePicturePath))
            target.ProfilePicturePath = source.ProfilePicturePath;
    }

    private static void MapFamily(StudentsWebEditDTO source, StudentFamilyDetail target)
    {
        target.FatherName = source.FatherName.Trim();
        target.FatherAge = source.FatherAge;
        target.FatherOccupationId = source.FatherOccupationId;
        target.FatherEducationId = source.FatherEducationId;
        target.MotherName = source.MotherName.Trim();
        target.MotherAge = source.MotherAge;
        target.MotherOccupationId = source.MotherOccupationId;
        target.MotherEducationId = source.MotherEducationId;
        target.PrimaryContactNumber = source.PrimaryContactNumber.Trim();
        target.AlternateContactNumber = source.AlternateContactNumber?.Trim() ?? string.Empty;
        target.HouseAddress = source.HouseAddress.Trim();
        target.PinCode = source.PinCode.Trim();
        target.PeopleInHouseId = source.PeopleInHouseId;
        target.CasteId = source.CasteId;
        target.ReligionId = source.ReligionId;
        target.ParentMonthlyIncome = source.ParentMonthlyIncome;
        target.ParentMontlyExpenditure = source.ParentMontlyExpenditure;
    }

    private static void StampDeleted(BaseAuditableEntity entity, int userId)
    {
        entity.IsDeleted = true;
        entity.DeletedBy = userId;
        entity.DeletedDate = DateTime.UtcNow;
        switch (entity)
        {
            case StudentFamilyDetail family: family.DateEntryPoint = StudentsWebEntryPoint.Web; break;
            case StudentHealth health: health.DateEntryPoint = StudentsWebEntryPoint.Web; break;
            case StudentDocument document: document.DateEntryPoint = StudentsWebEntryPoint.Web; break;
        }
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<bool> CanAccessStudent(int studentId, int userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@StudentId", studentId);
        parameters.Add("@UserId", userId);
        return await db.Connection.ExecuteScalarAsync<bool>(
            "dbo.usp_WebStudents_CanAccessStudent", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
    }

    private async Task<bool> CanAccessInstitution(int institutionId, int userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@InstitutionId", institutionId);
        parameters.Add("@UserId", userId);
        return await db.Connection.ExecuteScalarAsync<bool>(
            "dbo.usp_WebStudents_CanAccessInstitution", parameters, db.Transaction,
            commandType: CommandType.StoredProcedure);
    }
}

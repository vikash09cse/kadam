namespace Core.Features.StudentsWeb;

public sealed class StudentsWebService(
    IStudentsWebRepository repository,
    StudentsWebValidator validator)
{
    public Task<bool> HasPageAccess(int userId, string pageUrl) =>
        repository.HasPageAccess(userId, pageUrl);

    public Task<StudentsWebDashboardDTO> GetDashboard(int userId) =>
        repository.GetDashboard(userId);

    public Task<IReadOnlyList<StudentsWebListItemDTO>> GetStudents(
        int userId, int pageNumber, int pageSize, string? studentName, string? studentId, string? aadhaarNumber, int? status) =>
        repository.GetStudents(userId, pageNumber, pageSize, studentName, studentId, aadhaarNumber, status);

    public Task<IReadOnlyList<StudentsWebInstitutionStudentDTO>> GetInstitutionStudents(
        int userId, int pageNumber, int pageSize, string? searchText, int? institutionId,
        int? gradeId, string? section, DateTime? fromDate, DateTime? toDate, int? status) =>
        repository.GetInstitutionStudents(
            userId, pageNumber, pageSize, searchText, institutionId,
            gradeId, section, fromDate, toDate, status);

    public Task<StudentsWebEditDTO?> GetStudent(int id, int userId) => repository.GetStudent(id, userId);

    public Task<IReadOnlyList<StudentsWebLookupDTO>> GetInstitutions(int userId) =>
        repository.GetInstitutions(userId);

    public Task<IReadOnlyList<StudentsWebLookupDTO>> GetGradeSections(int institutionId) =>
        repository.GetGradeSections(institutionId);

    public Task<IReadOnlyList<StudentsWebAttendanceRowDTO>> GetAttendanceRoster(
        int userId, int institutionId, int gradeId, string section, DateTime attendanceDate) =>
        repository.GetAttendanceRoster(userId, institutionId, gradeId, section, attendanceDate.Date);

    public async Task<StudentsWebSaveResult> SaveAttendance(
        StudentsWebAttendanceSaveDTO model, int userId)
    {
        model.Section = model.Section?.Trim() ?? string.Empty;
        model.Entries ??= [];
        var errors = new List<string>();

        if (model.InstitutionId <= 0) errors.Add("Institution is required.");
        if (model.GradeId <= 0) errors.Add("Grade is required.");
        if (string.IsNullOrWhiteSpace(model.Section)) errors.Add("Section is required.");
        else if (model.Section.Length > 25) errors.Add("Section cannot exceed 25 characters.");
        if (model.AttendanceDate == default) errors.Add("Attendance date is required.");
        else if (model.AttendanceDate.Date > DateTime.Today) errors.Add("Attendance date cannot be in the future.");
        if (model.DayType is not (StudentsWebAttendance.WorkingDay or StudentsWebAttendance.HolidayDay))
            errors.Add("Select Working or Holiday.");
        if (model.Entries.Count == 0) errors.Add("Mark at least one student before saving.");
        if (model.Entries.Any(x => x.StudentId <= 0)) errors.Add("Every attendance row must identify a student.");
        if (model.Entries.Select(x => x.StudentId).Distinct().Count() != model.Entries.Count)
            errors.Add("Duplicate students cannot be submitted.");

        foreach (var entry in model.Entries)
        {
            entry.AttendanceNote = string.IsNullOrWhiteSpace(entry.AttendanceNote)
                ? null
                : entry.AttendanceNote.Trim();
            if ((entry.AttendanceNote?.Length ?? 0) > 255)
                errors.Add("Attendance reason cannot exceed 255 characters.");

            if (model.DayType == StudentsWebAttendance.HolidayDay)
            {
                if (entry.AttendanceStatus != StudentsWebAttendance.Holiday)
                    errors.Add("Holiday attendance must use Holiday status.");
                entry.AttendanceNote = null;
            }
            else if (entry.AttendanceStatus == StudentsWebAttendance.Absent)
            {
                if (string.IsNullOrWhiteSpace(entry.AttendanceNote))
                    errors.Add("An absence reason is required for every absent student.");
                else if (!StudentsWebAttendance.AbsenceReasons.Contains(
                             entry.AttendanceNote, StringComparer.OrdinalIgnoreCase))
                    errors.Add("Select a valid absence reason.");
            }
            else
            {
                if (entry.AttendanceStatus != StudentsWebAttendance.Present)
                    errors.Add("Working-day status must be Present or Absent.");
                entry.AttendanceNote = null;
            }
        }

        if (errors.Count > 0)
            return new StudentsWebSaveResult
            {
                Message = "Attendance information is invalid.",
                Errors = errors.Distinct().ToArray()
            };

        model.AttendanceDate = model.AttendanceDate.Date;
        var status = await repository.SaveAttendance(model, userId);
        return status switch
        {
            StudentsWebAttendanceSaveStatus.Saved =>
                new StudentsWebSaveResult
                {
                    Success = true,
                    Id = model.Entries.Count,
                    Message = $"Attendance saved for {model.Entries.Count} student(s)."
                },
            StudentsWebAttendanceSaveStatus.InvalidScope =>
                new StudentsWebSaveResult
                {
                    Message = "The selected institution, grade, or section is invalid or inaccessible."
                },
            _ => new StudentsWebSaveResult
            {
                Message = "The roster changed or contains an ineligible student. Reload and try again."
            }
        };
    }

    public Task<IReadOnlyList<StudentsWebFollowupListItemDTO>> GetFollowups(
        int userId, int pageNumber, int pageSize, int? institutionId,
        int? gradeId, string? section, DateTime? fromDate, DateTime? toDate) =>
        repository.GetFollowups(
            userId, pageNumber, pageSize, institutionId, gradeId, section, fromDate, toDate);

    public Task<StudentsWebFollowupDTO?> GetFollowup(int id, int userId) =>
        repository.GetFollowup(id, userId);

    public async Task<StudentsWebSaveResult> SaveFollowup(
        StudentsWebFollowupSaveDTO model, int userId)
    {
        model.Section = model.Section?.Trim() ?? string.Empty;
        model.TeacherName = model.TeacherName?.Trim() ?? string.Empty;
        model.TeacherContact = model.TeacherContact?.Trim() ?? string.Empty;
        model.ChildrenSitTogether = string.IsNullOrWhiteSpace(model.ChildrenSitTogether)
            ? null
            : model.ChildrenSitTogether.Trim();

        var errors = new List<string>();
        if (model.Id < 0) errors.Add("Follow-up is invalid.");
        if (model.VisitDate == default) errors.Add("Visit date is required.");
        else if (model.VisitDate.Date > DateTime.Today) errors.Add("Visit date cannot be in the future.");
        if (model.InstitutionId <= 0) errors.Add("Institution is required.");
        if (model.GradeId <= 0) errors.Add("Grade is required.");
        if (string.IsNullOrWhiteSpace(model.Section)) errors.Add("Section is required.");
        else if (model.Section.Length > 25) errors.Add("Section cannot exceed 25 characters.");
        if (string.IsNullOrWhiteSpace(model.TeacherName)) errors.Add("Teacher name is required.");
        else if (model.TeacherName.Length > 100) errors.Add("Teacher name cannot exceed 100 characters.");
        if (model.TeacherContact.Length != 10 ||
            model.TeacherContact.Any(character => character is < '0' or > '9'))
            errors.Add("Teacher contact must contain exactly 10 digits.");
        if (model.MaleStudentCount < 0) errors.Add("Male student count cannot be negative.");
        if (model.FemaleStudentCount < 0) errors.Add("Female student count cannot be negative.");
        if (model.MaleStudentCount > 100000 || model.FemaleStudentCount > 100000)
            errors.Add("Student counts cannot exceed 100,000.");

        var totalStudentCount = (long)model.MaleStudentCount + model.FemaleStudentCount;
        if (totalStudentCount <= 0) errors.Add("At least one student is required.");
        if (totalStudentCount > int.MaxValue) errors.Add("Total student count is too large.");
        if (model.PresentTodayCount < 0 || model.PresentTodayCount > totalStudentCount)
            errors.Add("Present today must be between zero and total students.");

        var hasWorkingDays = model.LastMonthWorkingDays.HasValue;
        var hasAttendance = model.LastMonthAttendance.HasValue;
        if (hasWorkingDays != hasAttendance)
            errors.Add("Last-month working days and attendance must be entered together.");
        if (hasWorkingDays && (model.LastMonthWorkingDays < 1 || model.LastMonthWorkingDays > 31))
            errors.Add("Last-month working days must be between 1 and 31.");
        if (hasAttendance && model.LastMonthAttendance < 0)
            errors.Add("Last-month attendance cannot be negative.");
        if (hasWorkingDays && hasAttendance &&
            model.LastMonthAttendance!.Value >
            totalStudentCount * model.LastMonthWorkingDays!.Value)
            errors.Add("Last-month attendance cannot exceed total students multiplied by working days.");
        if (model.ChildrenSitTogether is not null &&
            !new[] { "Yes", "No" }.Contains(model.ChildrenSitTogether, StringComparer.OrdinalIgnoreCase))
            errors.Add("Children sit together must be Yes, No, or blank.");

        if (errors.Count > 0)
            return new StudentsWebSaveResult
            {
                Message = "Follow-up information is invalid.",
                Errors = errors.Distinct().ToArray()
            };

        model.VisitDate = model.VisitDate.Date;
        model.ChildrenSitTogether = model.ChildrenSitTogether is null
            ? null
            : model.ChildrenSitTogether.Equals("Yes", StringComparison.OrdinalIgnoreCase) ? "Yes" : "No";
        var total = (int)totalStudentCount;
        var todayPercentage = Math.Round(model.PresentTodayCount * 100d / total, 2);
        double? lastMonthPercentage = model.LastMonthWorkingDays.HasValue
            ? Math.Round(model.LastMonthAttendance!.Value * 100d /
                         (total * (double)model.LastMonthWorkingDays.Value), 2)
            : null;

        var status = await repository.SaveFollowup(
            model, userId, total, todayPercentage, lastMonthPercentage);
        return status switch
        {
            StudentsWebFollowupSaveStatus.Saved => new StudentsWebSaveResult
            {
                Success = true,
                Id = model.Id,
                Message = "Follow-up saved successfully."
            },
            StudentsWebFollowupSaveStatus.InvalidScope => new StudentsWebSaveResult
            {
                Message = "The selected institution, grade, or section is invalid or inaccessible."
            },
            _ => new StudentsWebSaveResult
            {
                Message = "Follow-up was not found or access was denied."
            }
        };
    }

    public async Task<StudentsWebSaveResult> DeleteFollowup(int id, int userId)
    {
        var deleted = id > 0 && await repository.DeleteFollowup(id, userId);
        return new StudentsWebSaveResult
        {
            Success = deleted,
            Id = id,
            Message = deleted ? "Follow-up deleted." : "Follow-up was not found or access was denied."
        };
    }

    public Task<IReadOnlyList<StudentsWebThemeActivityListItemDTO>> GetThemeActivities(
        int userId, int pageNumber, int pageSize, int? institutionId, int? themeId,
        int? gradeId, string? section, DateTime? fromDate, DateTime? toDate) =>
        repository.GetThemeActivities(
            userId, pageNumber, pageSize, institutionId, themeId,
            gradeId, section, fromDate, toDate);

    public Task<StudentsWebThemeActivityDTO?> GetThemeActivity(int id, int userId) =>
        repository.GetThemeActivity(id, userId);

    public Task<IReadOnlyList<StudentsWebLookupDTO>> GetActiveThemes() =>
        repository.GetActiveThemes();

    public Task<IReadOnlyList<StudentsWebLookupDTO>> GetThemeActivityGradeSections(
        int userId, int institutionId) =>
        repository.GetThemeActivityGradeSections(userId, institutionId);

    public Task<int> GetThemeActivityEligibleCount(
        int userId, int institutionId,
        IReadOnlyList<StudentsWebThemeActivityGradeSectionDTO> gradeSections) =>
        repository.GetThemeActivityEligibleCount(
            userId, institutionId, NormalizeThemeGradeSections(gradeSections));

    public async Task<StudentsWebSaveResult> SaveThemeActivity(
        StudentsWebThemeActivitySaveDTO model, int userId)
    {
        model.GradeSections = NormalizeThemeGradeSections(model.GradeSections);
        var errors = new List<string>();
        var minimumDate = DateTime.Today.AddMonths(-1);

        if (model.Id < 0) errors.Add("Theme activity is invalid.");
        if (model.ActivityDate == default) errors.Add("Activity date is required.");
        else if (model.ActivityDate.Date > DateTime.Today) errors.Add("Activity date cannot be in the future.");
        else if (model.Id == 0 && model.ActivityDate.Date < minimumDate)
            errors.Add("Activity date must be within the last month.");
        if (model.InstitutionId <= 0) errors.Add("Institution is required.");
        if (model.ThemeId <= 0) errors.Add("Theme is required.");
        if (model.GradeSections.Count == 0) errors.Add("Select at least one grade and section.");
        if (model.GradeSections.Any(x => x.GradeId <= 0 || string.IsNullOrWhiteSpace(x.Section)))
            errors.Add("Every grade and section selection must be valid.");
        if (model.GradeSections.Any(x => x.Section.Length > 100))
            errors.Add("Section cannot exceed 100 characters.");
        if (model.StudentsAttended < 0) errors.Add("Students attended cannot be negative.");

        if (!model.DidChildrensDayHappen)
        {
            model.ParentsAttended = null;
        }
        else if (!model.ParentsAttended.HasValue)
        {
            errors.Add("Parents attended is required when Children's Day happened.");
        }
        else if (model.ParentsAttended < 0 || model.ParentsAttended > model.StudentsAttended)
        {
            errors.Add("Parents attended must be between zero and students attended.");
        }

        if (errors.Count > 0)
            return new StudentsWebSaveResult
            {
                Message = "Theme activity information is invalid.",
                Errors = errors.Distinct().ToArray()
            };

        model.ActivityDate = model.ActivityDate.Date;
        var status = await repository.SaveThemeActivity(model, userId);
        return status switch
        {
            StudentsWebThemeActivitySaveStatus.Saved => new StudentsWebSaveResult
            {
                Success = true,
                Id = model.Id,
                Message = "Theme activity saved successfully."
            },
            StudentsWebThemeActivitySaveStatus.InvalidScope => new StudentsWebSaveResult
            {
                Message = "The institution, grade, or section is invalid or inaccessible."
            },
            StudentsWebThemeActivitySaveStatus.InvalidTheme => new StudentsWebSaveResult
            {
                Message = "Select an active theme."
            },
            StudentsWebThemeActivitySaveStatus.InvalidDate => new StudentsWebSaveResult
            {
                Message = "An older activity may retain its original date, but cannot be changed to another old date."
            },
            StudentsWebThemeActivitySaveStatus.NoEligibleStudents => new StudentsWebSaveResult
            {
                Message = "The selected grade and sections have no active students."
            },
            StudentsWebThemeActivitySaveStatus.InvalidAttendance => new StudentsWebSaveResult
            {
                Message = "Students attended must be between zero and total eligible students."
            },
            _ => new StudentsWebSaveResult
            {
                Message = "Theme activity was not found or access was denied."
            }
        };
    }

    public async Task<StudentsWebSaveResult> DeleteThemeActivity(int id, int userId)
    {
        var deleted = id > 0 && await repository.DeleteThemeActivity(id, userId);
        return new StudentsWebSaveResult
        {
            Success = deleted,
            Id = id,
            Message = deleted ? "Theme activity deleted." : "Theme activity was not found or access was denied."
        };
    }

    private static List<StudentsWebThemeActivityGradeSectionDTO> NormalizeThemeGradeSections(
        IEnumerable<StudentsWebThemeActivityGradeSectionDTO>? gradeSections) =>
        (gradeSections ?? [])
            .Select(x => new StudentsWebThemeActivityGradeSectionDTO
            {
                GradeId = x.GradeId,
                Section = x.Section?.Trim() ?? string.Empty
            })
            .GroupBy(x => $"{x.GradeId}:{x.Section}", StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToList();

    public Task<StudentsWebAssessmentDTO?> GetAssessment(
        int studentId, StudentsWebAssessmentKind kind, int userId) =>
        repository.GetAssessment(studentId, kind, userId);

    public async Task<StudentsWebSaveResult> SaveBaseline(
        StudentsWebAssessmentSaveDTO model, int userId)
    {
        var assessment = await repository.GetAssessment(
            model.StudentId, StudentsWebAssessmentKind.Baseline, userId);
        if (assessment is null)
            return new StudentsWebSaveResult { Message = "Student was not found or access was denied." };
        if (assessment.IsLocked)
            return new StudentsWebSaveResult { Message = assessment.LockReason };

        var errors = new List<string>();
        if (!model.CompletedDate.HasValue) errors.Add("Baseline completion date is required.");
        else
        {
            if (model.CompletedDate.Value.Date < assessment.EnrollmentDate.Date)
                errors.Add("Baseline date cannot be before enrollment.");
            if (model.CompletedDate.Value.Date > DateTime.Today)
                errors.Add("Baseline date cannot be in the future.");
        }

        var subjectMap = assessment.Subjects.ToDictionary(x => x.SubjectId);
        if (model.Scores.Count != subjectMap.Count ||
            model.Scores.Select(x => x.SubjectId).Distinct().Count() != subjectMap.Count ||
            model.Scores.Any(x => !subjectMap.ContainsKey(x.SubjectId)))
            errors.Add("Scores are required for every active subject.");
        foreach (var score in model.Scores.Where(x => subjectMap.ContainsKey(x.SubjectId)))
        {
            if (score.ObtainedMarks is decimal marks &&
                (marks < 0 || marks > subjectMap[score.SubjectId].TotalMarks))
                errors.Add($"{subjectMap[score.SubjectId].SubjectName} marks must be between 0 and {subjectMap[score.SubjectId].TotalMarks:0.##}.");
        }
        if (errors.Count > 0)
            return new StudentsWebSaveResult { Errors = errors, Message = "Baseline information is invalid." };

        var status = await repository.SaveBaseline(model, assessment, userId);
        return status switch
        {
            StudentsWebAssessmentSaveStatus.Saved =>
                new StudentsWebSaveResult { Success = true, Id = model.StudentId, Message = "Baseline saved successfully." },
            StudentsWebAssessmentSaveStatus.Locked =>
                new StudentsWebSaveResult { Id = model.StudentId, Message = "Baseline is locked because student progress has started." },
            StudentsWebAssessmentSaveStatus.InvalidSubjects =>
                new StudentsWebSaveResult { Id = model.StudentId, Message = "Student or subject information changed. Reload and try again." },
            _ => new StudentsWebSaveResult { Id = model.StudentId, Message = "Student was not found or access was denied." }
        };
    }

    public async Task<StudentsWebSaveResult> SaveEndline(
        StudentsWebAssessmentSaveDTO model, int userId)
    {
        var assessment = await repository.GetAssessment(
            model.StudentId, StudentsWebAssessmentKind.Endline, userId);
        if (assessment is null)
            return new StudentsWebSaveResult { Message = "Student was not found or access was denied." };
        if (assessment.IsLocked)
            return new StudentsWebSaveResult { Message = assessment.LockReason };

        var errors = new List<string>();
        if (!assessment.BaselineCompletedDate.HasValue)
            errors.Add("A completed baseline is required.");
        if (!model.CompletedDate.HasValue)
            errors.Add("Endline completion date is required.");
        else
        {
            if (assessment.BaselineCompletedDate.HasValue &&
                model.CompletedDate.Value.Date < assessment.BaselineCompletedDate.Value.Date)
                errors.Add("Endline date cannot be before the baseline completion date.");
            if (model.CompletedDate.Value.Date > DateTime.Today)
                errors.Add("Endline date cannot be in the future.");
        }

        var subjectMap = assessment.Subjects.ToDictionary(x => x.SubjectId);
        if (model.Scores.Count != subjectMap.Count ||
            model.Scores.Select(x => x.SubjectId).Distinct().Count() != subjectMap.Count ||
            model.Scores.Any(x => !subjectMap.ContainsKey(x.SubjectId)))
            errors.Add("All active subjects must be submitted.");
        foreach (var score in model.Scores.Where(x => subjectMap.ContainsKey(x.SubjectId)))
        {
            if (score.ObtainedMarks is decimal marks &&
                (marks < 0 || marks > subjectMap[score.SubjectId].TotalMarks))
                errors.Add($"{subjectMap[score.SubjectId].SubjectName} marks must be between 0 and {subjectMap[score.SubjectId].TotalMarks:0.##}.");
        }
        if (errors.Count > 0)
            return new StudentsWebSaveResult { Errors = errors, Message = "Endline information is invalid." };

        var status = await repository.SaveEndline(model, assessment, userId);
        return status switch
        {
            StudentsWebAssessmentSaveStatus.Saved =>
                new StudentsWebSaveResult { Success = true, Id = model.StudentId, Message = "Endline saved successfully." },
            StudentsWebAssessmentSaveStatus.Locked =>
                new StudentsWebSaveResult { Id = model.StudentId, Message = "Endline cannot be changed or its prerequisites are incomplete." },
            StudentsWebAssessmentSaveStatus.InvalidSubjects =>
                new StudentsWebSaveResult { Id = model.StudentId, Message = "Student or subject information changed. Reload and try again." },
            _ => new StudentsWebSaveResult { Id = model.StudentId, Message = "Student was not found or access was denied." }
        };
    }

    public Task<StudentsWebProgressDTO?> GetProgress(int studentId, int userId) =>
        repository.GetProgress(studentId, userId);

    public async Task<StudentsWebSaveResult> CompleteProgressStep(int studentId, int stepId, int userId)
    {
        if (studentId <= 0 || stepId <= 0)
            return new StudentsWebSaveResult { Message = "Student and step are required." };
        var status = await repository.CompleteProgressStep(studentId, stepId, userId);
        return status switch
        {
            StudentsWebProgressSaveStatus.Saved =>
                new StudentsWebSaveResult { Success = true, Id = studentId, Message = "Progress step completed." },
            StudentsWebProgressSaveStatus.BaselineRequired =>
                new StudentsWebSaveResult { Message = "A completed baseline is required." },
            StudentsWebProgressSaveStatus.OutOfRange =>
                new StudentsWebSaveResult { Message = "This step is outside the student's assigned progress range." },
            StudentsWebProgressSaveStatus.PreviousStepRequired =>
                new StudentsWebSaveResult { Message = "Complete the previous step first." },
            StudentsWebProgressSaveStatus.PreviousGradeTestRequired =>
                new StudentsWebSaveResult { Message = "Complete the previous grade test first." },
            _ => new StudentsWebSaveResult { Message = "Student was not found or access was denied." }
        };
    }

    public Task<StudentsWebGradeTestDTO?> GetGradeTest(int studentId, int gradeLevelId, int userId) =>
        repository.GetGradeTest(studentId, gradeLevelId, userId);

    public async Task<StudentsWebSaveResult> SaveGradeTest(StudentsWebGradeTestSaveDTO model, int userId)
    {
        var gradeTest = await repository.GetGradeTest(model.StudentId, model.GradeLevelId, userId);
        if (gradeTest is null)
            return new StudentsWebSaveResult { Message = "Grade test was not found or access was denied." };

        var errors = new List<string>();
        if (!model.CompletedDate.HasValue) errors.Add("Completed date is required.");
        else if (model.CompletedDate.Value.Date < gradeTest.EnrollmentDate.Date ||
                 model.CompletedDate.Value.Date > DateTime.Today)
            errors.Add("Completed date must be between enrollment date and today.");

        var subjects = gradeTest.Subjects.ToDictionary(x => x.SubjectId);
        if (model.Scores.Count != subjects.Count ||
            model.Scores.Select(x => x.SubjectId).Distinct().Count() != subjects.Count ||
            model.Scores.Any(x => !subjects.ContainsKey(x.SubjectId)))
            errors.Add("Marks are required for every active subject.");
        foreach (var score in model.Scores.Where(x => subjects.ContainsKey(x.SubjectId)))
        {
            if (!score.ObtainedMarks.HasValue || score.ObtainedMarks <= 0)
                errors.Add($"{subjects[score.SubjectId].SubjectName} marks must be greater than zero.");
            else if (score.ObtainedMarks > subjects[score.SubjectId].TotalMarks)
                errors.Add($"{subjects[score.SubjectId].SubjectName} marks cannot exceed {subjects[score.SubjectId].TotalMarks:0.##}.");
        }
        if (errors.Count == 0)
        {
            var obtained = model.Scores.Sum(x => x.ObtainedMarks!.Value);
            var total = gradeTest.Subjects.Sum(x => x.TotalMarks);
            var aggregate = Math.Round(obtained / total * 100m, 0, MidpointRounding.ToEven);
            if (aggregate < 60) errors.Add("A minimum aggregate score of 60% is required.");
        }
        if (errors.Count > 0)
            return new StudentsWebSaveResult { Errors = errors, Message = "Grade test information is invalid." };

        var status = await repository.SaveGradeTest(model, gradeTest, userId);
        return status switch
        {
            StudentsWebGradeTestSaveStatus.Saved =>
                new StudentsWebSaveResult { Success = true, Id = model.StudentId, Message = "Grade test saved successfully." },
            StudentsWebGradeTestSaveStatus.StepsIncomplete =>
                new StudentsWebSaveResult { Message = "Complete both grade steps before the grade test." },
            StudentsWebGradeTestSaveStatus.PreviousGradeScoreRequired =>
                new StudentsWebSaveResult { Message = "The previous grade test requires at least 80%." },
            StudentsWebGradeTestSaveStatus.InvalidSubjects =>
                new StudentsWebSaveResult { Message = "Subject information changed. Reload and try again." },
            _ => new StudentsWebSaveResult { Message = "Student was not found or access was denied." }
        };
    }

    public Task<StudentsWebHealthDTO?> GetHealth(int studentId, int userId) => repository.GetHealth(studentId, userId);

    public Task<IReadOnlyList<StudentsWebDocumentDTO>> GetDocuments(int studentId, int userId) =>
        repository.GetDocuments(studentId, userId);

    public async Task<StudentsWebSaveResult> SaveStudent(StudentsWebEditDTO model, int userId)
    {
        var errors = validator.ValidateStudent(model).ToList();
        if (!string.IsNullOrWhiteSpace(model.StudentRegistratioNumber) &&
            await repository.StudentRegistrationNumberExists(
                model.StudentRegistratioNumber.Trim(), model.InstitutionId, model.Id))
            errors.Add("Student registration number already exists for this institution.");

        if (model.DoYouHaveAadhaarCard &&
            !string.IsNullOrWhiteSpace(model.AadhaarCardNumber) &&
            await repository.AadhaarExists(model.AadhaarCardNumber.Trim(), model.Id))
            errors.Add("Aadhaar number already exists.");

        if (errors.Count > 0)
            return new StudentsWebSaveResult { Success = false, Errors = errors, Message = "Please correct the highlighted information." };

        model.Age = StudentsWebValidator.EffectiveAge(model.DateOfBirth, model.EnrollmentDate);
        model.DateEntryPoint = StudentsWebEntryPoint.Web;
        var id = await repository.SaveStudent(model, userId);
        return new StudentsWebSaveResult
        {
            Success = id > 0,
            Id = id,
            Message = id > 0 ? "Student saved successfully." : "Student could not be saved."
        };
    }

    public async Task<StudentsWebSaveResult> UpdateStatus(StudentsWebStatusDTO model, int userId)
    {
        model.InActiveReason ??= string.Empty;
        model.Remarks ??= string.Empty;
        var errors = new List<string>();
        if (model.StudentId <= 0) errors.Add("Student is required.");
        if (model.Status == 3) errors.Add("Completed status can only be set through the Mainstream workflow.");
        else if (model.Status is < 1 or > 2) errors.Add("A valid status is required.");
        if (model.Status == 2)
        {
            if (string.IsNullOrWhiteSpace(model.InActiveReason)) errors.Add("Inactive reason is required.");
            if (!model.InActiveDate.HasValue) errors.Add("Inactive date is required.");
            if (model.InActiveDate?.Date > DateTime.Today) errors.Add("Inactive date cannot be in the future.");
            string[] allowedReasons =
            [
                "Child not comfortable in school environment",
                "Financial constraint of family",
                "Migration of family",
                "Obligation to do household chores",
                "Death of the child",
                "Others"
            ];
            if (!string.IsNullOrWhiteSpace(model.InActiveReason) &&
                !allowedReasons.Contains(model.InActiveReason.Trim(), StringComparer.OrdinalIgnoreCase))
                errors.Add("A valid inactive reason is required.");
        }
        if (model.InActiveReason.Length > 100) errors.Add("Inactive reason cannot exceed 100 characters.");
        if (model.Remarks.Length > 255) errors.Add("Remarks cannot exceed 255 characters.");
        if (errors.Count > 0) return new StudentsWebSaveResult { Errors = errors, Message = "Status is invalid." };

        var saved = await repository.UpdateStatus(model, userId);
        return new StudentsWebSaveResult { Success = saved, Id = model.StudentId, Message = saved ? "Status updated." : "Status could not be updated." };
    }

    public async Task<StudentsWebSaveResult> SaveTrio(StudentsWebTrioDTO model, int userId)
    {
        if (model.StudentId <= 0 || model.TrioId <= 0)
            return new StudentsWebSaveResult { Errors = ["Student and a positive Trio ID are required."], Message = "Trio ID is invalid." };

        var status = await repository.SaveTrio(model.StudentId, model.TrioId, userId);
        return status switch
        {
            StudentsWebTrioSaveStatus.Saved =>
                new StudentsWebSaveResult { Success = true, Id = model.StudentId, Message = "Trio ID saved." },
            StudentsWebTrioSaveStatus.BaselineRequired =>
                new StudentsWebSaveResult { Id = model.StudentId, Message = "A baseline must be added before assigning a Trio ID." },
            StudentsWebTrioSaveStatus.CapacityReached =>
                new StudentsWebSaveResult { Id = model.StudentId, Message = "This Trio has reached its allowed capacity." },
            _ => new StudentsWebSaveResult { Id = model.StudentId, Message = "Student was not found or access was denied." }
        };
    }

    public Task<StudentsWebMainstreamDTO?> GetMainstream(int studentId, int userId) =>
        repository.GetMainstream(studentId, userId);

    public Task<StudentsWebPromotionDTO?> GetPromotion(int studentId, int userId) =>
        repository.GetPromotion(studentId, userId);

    public async Task<StudentsWebSaveResult> PromoteStudent(StudentsWebPromotionDTO model, int userId)
    {
        var errors = new List<string>();
        if (model.StudentId <= 0) errors.Add("Student is required.");
        if (model.DestinationGradeId <= 0) errors.Add("Promoted grade is required.");
        if (string.IsNullOrWhiteSpace(model.DestinationSection)) errors.Add("Promoted section is required.");
        else if (model.DestinationSection.Trim().Length > 25) errors.Add("Section cannot exceed 25 characters.");
        if (model.PromotionDate.Date > DateTime.Today) errors.Add("Promotion date cannot be in the future.");
        if (errors.Count > 0)
            return new StudentsWebSaveResult { Errors = errors, Message = "Promotion information is invalid." };

        var status = await repository.PromoteStudent(model, userId);
        return status switch
        {
            StudentsWebPromotionSaveStatus.Saved =>
                new StudentsWebSaveResult { Success = true, Id = model.StudentId, Message = "Student promoted successfully." },
            StudentsWebPromotionSaveStatus.NotEligible =>
                new StudentsWebSaveResult { Message = "Only eligible Kadam Plus students can be promoted." },
            StudentsWebPromotionSaveStatus.InvalidGradeOrSection =>
                new StudentsWebSaveResult { Message = "Select a different configured grade and section." },
            StudentsWebPromotionSaveStatus.InvalidDate =>
                new StudentsWebSaveResult { Message = "Promotion date must be between enrollment date and today." },
            _ => new StudentsWebSaveResult { Message = "Student was not found or access was denied." }
        };
    }

    public Task<IReadOnlyList<StudentsWebLookupDTO>> GetStates() => repository.GetStates();

    public Task<IReadOnlyList<StudentsWebLookupDTO>> GetDistricts(int stateId) =>
        repository.GetDistricts(stateId);

    public Task<IReadOnlyList<StudentsWebLookupDTO>> GetMainstreamInstitutions(
        int userId, int stateId, int districtId) =>
        repository.GetMainstreamInstitutions(userId, stateId, districtId);

    public async Task<StudentsWebSaveResult> SaveMainstream(StudentsWebMainstreamDTO model, int userId)
    {
        var errors = new List<string>();
        if (model.StudentId <= 0) errors.Add("Student is required.");
        if (!model.MainstreamDate.HasValue) errors.Add("Mainstream date is required.");
        else if (model.MainstreamDate.Value.Date > DateTime.Today) errors.Add("Mainstream date cannot be in the future.");
        if (!model.GradeId.HasValue || model.GradeId <= 0) errors.Add("Grade is required.");
        if (string.IsNullOrWhiteSpace(model.Section)) errors.Add("Section is required.");
        else if (model.Section.Trim().Length > 25) errors.Add("Section cannot exceed 25 characters.");
        if ((model.ChildSRNumber?.Trim().Length ?? 0) > 100) errors.Add("Child SR number cannot exceed 100 characters.");
        if (!model.IsMainstreamInstitutionSame &&
            (!model.MainstreamInstitutionId.HasValue || !model.StateId.HasValue || !model.DistrictId.HasValue))
            errors.Add("State, district, and mainstream institution are required.");

        if (errors.Count > 0)
            return new StudentsWebSaveResult { Errors = errors, Message = "Mainstream information is invalid." };

        var status = await repository.SaveMainstream(model, userId);
        return status switch
        {
            StudentsWebMainstreamSaveStatus.Saved =>
                new StudentsWebSaveResult { Success = true, Id = model.StudentId, Message = "Student mainstream information saved." },
            StudentsWebMainstreamSaveStatus.NotEligible =>
                new StudentsWebSaveResult { Message = "Completed baseline and endline assessments are required." },
            StudentsWebMainstreamSaveStatus.AlreadyMainstreamed =>
                new StudentsWebSaveResult { Message = "This student has already been mainstreamed." },
            StudentsWebMainstreamSaveStatus.InvalidInstitution =>
                new StudentsWebSaveResult { Message = "The selected mainstream institution is invalid or inaccessible." },
            StudentsWebMainstreamSaveStatus.InvalidGradeOrSection =>
                new StudentsWebSaveResult { Message = "The selected grade or section is not available for this institution." },
            _ => new StudentsWebSaveResult { Message = "Student was not found or access was denied." }
        };
    }

    public async Task<StudentsWebSaveResult> SaveHealth(StudentsWebHealthDTO model, int userId)
    {
        if (!model.PhysicallyChallenged)
        {
            model.PhysicallyChallengedType = null;
            model.PercentagePhysicallyChallenged = null;
            model.DisabilityCertificatePath = string.Empty;
            model.DisabilityCertificateFileName = string.Empty;
        }

        var errors = validator.ValidateHealth(model);
        if (errors.Count > 0) return new StudentsWebSaveResult { Errors = errors, Message = "Health information is invalid." };

        model.DateEntryPoint = StudentsWebEntryPoint.Web;
        var id = await repository.SaveHealth(model, userId);
        return new StudentsWebSaveResult { Success = id > 0, Id = id, Message = id > 0 ? "Health information saved." : "Health information could not be saved." };
    }

    public async Task<StudentsWebSaveResult> SaveDocument(StudentsWebDocumentDTO model, int userId)
    {
        var errors = validator.ValidateDocument(model);
        if (errors.Count > 0) return new StudentsWebSaveResult { Errors = errors, Message = "Document information is invalid." };

        model.DateEntryPoint = StudentsWebEntryPoint.Web;
        var id = await repository.SaveDocument(model, userId);
        return new StudentsWebSaveResult { Success = id > 0, Id = id, Message = id > 0 ? "Document saved." : "Document could not be saved." };
    }

    public async Task<StudentsWebSaveResult> DeleteDocument(int documentId, int userId)
    {
        var deleted = documentId > 0 && await repository.DeleteDocument(documentId, userId);
        return new StudentsWebSaveResult { Success = deleted, Id = documentId, Message = deleted ? "Document deleted." : "Document could not be deleted." };
    }
}

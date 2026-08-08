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

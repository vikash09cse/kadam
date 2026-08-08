namespace Core.Features.StudentsWeb;

public static class StudentsWebEntryPoint
{
    public const byte Mobile = 1;
    public const byte Web = 2;
}

public sealed class StudentsWebDashboardDTO
{
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
    public int CompletedCount { get; set; }
}

public sealed class StudentsWebListItemDTO
{
    public long RowNumber { get; set; }
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string InstitutionName { get; set; } = string.Empty;
    public string GradeName { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public int CurrentStatus { get; set; }
    public string? InActiveReason { get; set; }
    public DateTime? InActiveDate { get; set; }
    public string? Remarks { get; set; }
    public int? TrioId { get; set; }
    public bool IsBaselineAdded { get; set; }
    public bool IsBaselineCompleted { get; set; }
    public bool IsEndlineCompleted { get; set; }
    public bool HasMainstream { get; set; }
    public bool IsMainstreamEligible { get; set; }
    public byte DateEntryPoint { get; set; }
    public int TotalCount { get; set; }
    public string EntryPointText => DateEntryPoint == StudentsWebEntryPoint.Web ? "Web" : "Mobile";
}

public sealed class StudentsWebLookupDTO
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public string? Sections { get; set; }
}

public sealed class StudentsWebInstitutionStudentDTO
{
    public long RowNumber { get; set; }
    public int TotalCount { get; set; }
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public int InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public int GradeId { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public int Age { get; set; }
    public int CurrentStatus { get; set; }
    public bool IsKadamPlusStudent { get; set; }
    public bool IsBaselineAdded { get; set; }
    public bool IsBaselineCompleted { get; set; }
    public DateTime? BaselineCompletedDate { get; set; }
    public bool HasProgress { get; set; }
    public int? LastProgressStepId { get; set; }
    public int? ExitStepId { get; set; }
    public bool AllStepsCompleted { get; set; }
    public bool IsEndlineAdded { get; set; }
    public bool IsEndlineCompleted { get; set; }
    public DateTime? EndlineCompletedDate { get; set; }
    public bool HasMainstream { get; set; }
    public bool IsMainstreamEligible { get; set; }
    public bool CanEditBaseline { get; set; }
    public bool CanOpenProgress { get; set; }
    public bool CanAddEndline { get; set; }
}

public sealed class StudentsWebEditDTO
{
    public int Id { get; set; }
    public string? StudentId { get; set; }
    public DateTime EnrollmentDate { get; set; } = DateTime.Today;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int GenderId { get; set; }
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-7);
    public int Age { get; set; }
    public bool DoYouHaveAadhaarCard { get; set; }
    public string? AadhaarCardNumber { get; set; }
    public int InstitutionId { get; set; }
    public int GradeId { get; set; }
    public string Section { get; set; } = string.Empty;
    public string StudentRegistratioNumber { get; set; } = string.Empty;
    public int ChildStatudBeforeKadamSTC { get; set; }
    public int HowLongPlaningToStayThisArea { get; set; }
    public int Class { get; set; }
    public int ReasonId { get; set; }
    public string? DropoutClass { get; set; }
    public int? DropoutYear { get; set; }
    public bool IsKadamPlusStudent { get; set; } = true;
    public string? ProfilePicturePath { get; set; }
    public int CurrentStatus { get; set; } = 1;
    public byte DateEntryPoint { get; set; } = StudentsWebEntryPoint.Web;
    public bool IsBaselineAdded { get; set; }

    public int FamilyId { get; set; }
    public string FatherName { get; set; } = string.Empty;
    public int? FatherAge { get; set; }
    public int? FatherOccupationId { get; set; }
    public int? FatherEducationId { get; set; }
    public string MotherName { get; set; } = string.Empty;
    public int? MotherAge { get; set; }
    public int? MotherOccupationId { get; set; }
    public int? MotherEducationId { get; set; }
    public string PrimaryContactNumber { get; set; } = string.Empty;
    public string? AlternateContactNumber { get; set; }
    public string HouseAddress { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public int? PeopleInHouseId { get; set; }
    public int? CasteId { get; set; }
    public int? ReligionId { get; set; }
    public string? ParentMonthlyIncome { get; set; }
    public string? ParentMontlyExpenditure { get; set; }
}

public sealed class StudentsWebHealthDTO
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public bool PhysicallyChallenged { get; set; }
    public int? PhysicallyChallengedType { get; set; }
    public decimal? PercentagePhysicallyChallenged { get; set; }
    public string? DisabilityCertificatePath { get; set; }
    public string? DisabilityCertificateFileName { get; set; }
    public byte DateEntryPoint { get; set; } = StudentsWebEntryPoint.Web;
}

public sealed class StudentsWebDocumentDTO
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int DocumentTypeId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string? DocumentPath { get; set; }
    public string? DocumentFileName { get; set; }
    public byte DateEntryPoint { get; set; } = StudentsWebEntryPoint.Web;
}

public sealed class StudentsWebStatusDTO
{
    public int StudentId { get; set; }
    public int Status { get; set; }
    public string InActiveReason { get; set; } = string.Empty;
    public DateTime? InActiveDate { get; set; }
    public string Remarks { get; set; } = string.Empty;
}

public sealed class StudentsWebTrioDTO
{
    public int StudentId { get; set; }
    public int TrioId { get; set; }
}

public enum StudentsWebTrioSaveStatus
{
    Saved,
    NotAuthorizedOrNotFound,
    BaselineRequired,
    CapacityReached
}

public sealed class StudentsWebMainstreamDTO
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public int EnrolledInstitutionId { get; set; }
    public string EnrolledInstitutionName { get; set; } = string.Empty;
    public string EnrolledGradeName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public bool IsMainstreamInstitutionSame { get; set; } = true;
    public int? MainstreamInstitutionId { get; set; }
    public int? StateId { get; set; }
    public int? DistrictId { get; set; }
    public string? MainstreamInstitutionName { get; set; }
    public string? SchoolDISECode { get; set; }
    public int? GradeId { get; set; }
    public string? Section { get; set; }
    public string? ChildSRNumber { get; set; }
    public DateTime? MainstreamDate { get; set; }
    public bool IsEligible { get; set; }
    public bool HasExistingMainstream { get; set; }
}

public sealed class StudentsWebPromotionDTO
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public int Age { get; set; }
    public int InstitutionId { get; set; }
    public int CurrentGradeId { get; set; }
    public string CurrentGradeName { get; set; } = string.Empty;
    public string CurrentSection { get; set; } = string.Empty;
    public int DestinationGradeId { get; set; }
    public string DestinationSection { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public DateTime PromotionDate { get; set; } = DateTime.Today;
    public bool IsEligible { get; set; }
    public IReadOnlyList<StudentsWebLookupDTO> Grades { get; set; } = [];
}

public enum StudentsWebPromotionSaveStatus
{
    Saved,
    NotAuthorizedOrNotFound,
    NotEligible,
    InvalidGradeOrSection,
    InvalidDate
}

public enum StudentsWebMainstreamSaveStatus
{
    Saved,
    NotAuthorizedOrNotFound,
    NotEligible,
    AlreadyMainstreamed,
    InvalidInstitution,
    InvalidGradeOrSection
}

public sealed class StudentsWebSaveResult
{
    public bool Success { get; init; }
    public int Id { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<string> Errors { get; init; } = [];
}

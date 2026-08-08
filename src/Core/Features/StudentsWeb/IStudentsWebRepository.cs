namespace Core.Features.StudentsWeb;

public interface IStudentsWebRepository
{
    Task<bool> HasPageAccess(int userId, string pageUrl);
    Task<StudentsWebDashboardDTO> GetDashboard(int userId);
    Task<IReadOnlyList<StudentsWebListItemDTO>> GetStudents(
        int userId, int pageNumber, int pageSize, string? studentName, string? studentId, string? aadhaarNumber, int? status);
    Task<IReadOnlyList<StudentsWebInstitutionStudentDTO>> GetInstitutionStudents(
        int userId, int pageNumber, int pageSize, string? searchText, int? institutionId,
        int? gradeId, string? section, DateTime? fromDate, DateTime? toDate, int? status);
    Task<StudentsWebEditDTO?> GetStudent(int id, int userId);
    Task<IReadOnlyList<StudentsWebLookupDTO>> GetInstitutions(int userId);
    Task<IReadOnlyList<StudentsWebLookupDTO>> GetGradeSections(int institutionId);
    Task<StudentsWebAssessmentDTO?> GetAssessment(
        int studentId, StudentsWebAssessmentKind kind, int userId);
    Task<StudentsWebAssessmentSaveStatus> SaveBaseline(
        StudentsWebAssessmentSaveDTO model, StudentsWebAssessmentDTO assessment, int userId);
    Task<StudentsWebAssessmentSaveStatus> SaveEndline(
        StudentsWebAssessmentSaveDTO model, StudentsWebAssessmentDTO assessment, int userId);
    Task<StudentsWebProgressDTO?> GetProgress(int studentId, int userId);
    Task<StudentsWebProgressSaveStatus> CompleteProgressStep(int studentId, int stepId, int userId);
    Task<StudentsWebGradeTestDTO?> GetGradeTest(int studentId, int gradeLevelId, int userId);
    Task<StudentsWebGradeTestSaveStatus> SaveGradeTest(
        StudentsWebGradeTestSaveDTO model, StudentsWebGradeTestDTO gradeTest, int userId);
    Task<StudentsWebHealthDTO?> GetHealth(int studentId, int userId);
    Task<IReadOnlyList<StudentsWebDocumentDTO>> GetDocuments(int studentId, int userId);

    Task<bool> StudentRegistrationNumberExists(string registrationNumber, int institutionId, int exceptId);
    Task<bool> AadhaarExists(string aadhaarNumber, int exceptId);
    Task<int> SaveStudent(StudentsWebEditDTO model, int userId);
    Task<bool> UpdateStatus(StudentsWebStatusDTO model, int userId);
    Task<StudentsWebTrioSaveStatus> SaveTrio(int studentId, int trioId, int userId);
    Task<StudentsWebMainstreamDTO?> GetMainstream(int studentId, int userId);
    Task<StudentsWebPromotionDTO?> GetPromotion(int studentId, int userId);
    Task<StudentsWebPromotionSaveStatus> PromoteStudent(StudentsWebPromotionDTO model, int userId);
    Task<IReadOnlyList<StudentsWebLookupDTO>> GetStates();
    Task<IReadOnlyList<StudentsWebLookupDTO>> GetDistricts(int stateId);
    Task<IReadOnlyList<StudentsWebLookupDTO>> GetMainstreamInstitutions(int userId, int stateId, int districtId);
    Task<StudentsWebMainstreamSaveStatus> SaveMainstream(StudentsWebMainstreamDTO model, int userId);
    Task<int> SaveHealth(StudentsWebHealthDTO model, int userId);
    Task<int> SaveDocument(StudentsWebDocumentDTO model, int userId);
    Task<bool> DeleteDocument(int documentId, int userId);
}

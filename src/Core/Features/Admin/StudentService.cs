using Core.Abstractions;
using Core.DTOs;
using Core.DTOs.App;
using Core.Entities;
using Core.Utilities;
using static Core.Utilities.Enums;

namespace Core.Features.Admin
{
    public class StudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<ServiceResponseDTO> SaveStudent(Student student)
        {
            int _Id = student.Id;
            if (await _studentRepository.CheckDuplicateStudentRegistrationNumber(student.StudentRegistratioNumber, student.Id))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateStudentRegistrationNumber);
            }

            if (!string.IsNullOrEmpty(student.AadhaarCardNumber) &&
                await _studentRepository.CheckDuplicateAadhaarNumber(student.AadhaarCardNumber, student.Id))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateAadhaarNumber);
            }

            bool isSaved = await _studentRepository.SaveStudent(student);
            if (isSaved && _Id == 0)
            {
                await _studentRepository.GenerateStudentId(student.Id);
            }
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, result: student.Id, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> DeleteStudent(int id, int userId)
        {
            ServiceResponseDTO response;
            var isDeleted = await _studentRepository.DeleteStudent(id, userId);
            response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }

        public async Task<ServiceResponseDTO> GetStudent(int id)
        {
            var student = await _studentRepository.GetStudent(id);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, student, MessageSuccess.Found);
            return response;
        }

        public async Task<IEnumerable<Student>> GetAllStudents()
        {
            return await _studentRepository.GetAllStudents();
        }

        public async Task<IEnumerable<AppInstitutionDTO>> GetInstitutionsByUserId(int userId)
        {
            return await _studentRepository.GetInstitutionsByUserId(userId);
        }

        public async Task<ServiceResponseDTO> GetStudentListMobile(int createdBy)
        {
            var studentList = await _studentRepository.GetStudentListMobile(createdBy);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, studentList, MessageSuccess.Found);
            return response;
        }

        public async Task<ServiceResponseDTO> GetStudentListMyInstitutionMobile(int? institutionId, int? gradeId, string section, DateTime? fromDate, DateTime? toDate, int createdBy)
        {
            var studentList = await _studentRepository.GetStudentListMyInstitutionMobile(institutionId, gradeId, section, fromDate, toDate, createdBy);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, studentList, MessageSuccess.Found);
            return response;
        }

        public async Task<ServiceResponseDTO> GetStudentDefaultData(int userId)
        {
            var institutions = await _studentRepository.GetInstitutionsByUserId(userId);
            var studentDefaultData = new StudentDefaultDataDTO
            {
                Institutions = institutions,
                Genders = EnumHelper<Gender>.GetEnumDropdownList(),
                StudentReasons = EnumHelper<StudentReasonType>.GetEnumDropdownList(),
                ChildStatusBeforeKadamSTCs = EnumHelper<ChildStatusBeforKadamType>.GetEnumDropdownList(),
                HowLongStayAreaType = EnumHelper<HowLongStayInThisAreaType>.GetEnumDropdownList(),
                Occupations = EnumHelper<OccupationType>.GetEnumDropdownList(),
                Educations = EnumHelper<EducationType>.GetEnumDropdownList(),
                PeopleLivingCounts = EnumHelper<PeopleLivingCountType>.GetEnumDropdownList(),
                Castes = EnumHelper<CasteType>.GetEnumDropdownList(),
                Religions = EnumHelper<ReligionType>.GetEnumDropdownList(),
                MonthlyIncomes = EnumHelper<MonthlyIncomeType>.GetEnumDropdownList()
            };

            ServiceResponseDTO response = new(true, AppStatusCodes.Success, studentDefaultData, MessageSuccess.Found);

            return response;
        }

        public async Task<ServiceResponseDTO> GetInstitutionGradeByStudentId(int studentId)
        {
            var grades = await _studentRepository.GetInstitutionGradeByStudentId(studentId);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, grades, MessageSuccess.Found);
            return response;
        }

        public async Task<ServiceResponseDTO> SaveStudentProfilePicture(int id, string profilePicturePath)
        {
            var isSaved = await _studentRepository.SaveStudentProfilePicture(id, profilePicturePath);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> UpdateStudentPromotion(StudentPromotionUpdateDTO studentPromotionUpdateDTO)
        {
            var isUpdated = await _studentRepository.UpdateStudentPromotion(studentPromotionUpdateDTO);
            return new ServiceResponseDTO(isUpdated, isUpdated ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isUpdated, isUpdated ? MessageSuccess.Updated : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> GetDashboardCount(int createdBy)
        {
            var dashboardCount = await _studentRepository.GetDashboardCount(createdBy);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, dashboardCount, MessageSuccess.Found);
            return response;
        }

        public async Task<ServiceResponseDTO> UpdateStudentStatus(StudentStatusUpdateDTO model)
        {
            var isUpdated = await _studentRepository.UpdateStudentStatus(model);
            return new ServiceResponseDTO(isUpdated, isUpdated ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isUpdated, isUpdated ? MessageSuccess.Updated : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> GetStudentDetailForMainstream(int id)
        {
            var studentDetail = await _studentRepository.GetStudentDetailForMainstream(id);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, studentDetail, MessageSuccess.Found);
            return response;
        }
    }
}

using Core.Abstractions;
using Core.DTOs;
using Core.DTOs.App;
using Core.Entities;
using Core.Utilities;

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
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> DeleteStudent(int id, int userId)
        {
            ServiceResponseDTO response;
            var isDeleted = await _studentRepository.DeleteStudent(id, userId);
            response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }

        public async Task<Student> GetStudent(int id)
        {
            var student = await _studentRepository.GetStudent(id);
            return student;
        }

        public async Task<IEnumerable<Student>> GetAllStudents()
        {
            return await _studentRepository.GetAllStudents();
        }

        public async Task<IEnumerable<AppInstitutionDTO>> GetInstitutionsByUserId(int userId)
        {
            return await _studentRepository.GetInstitutionsByUserId(userId);
        }
    }
}

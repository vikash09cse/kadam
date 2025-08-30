using Core.Abstractions;
using Core.DTOs;
using Core.Entities;
using Core.Utilities;

namespace Core.Features.Admin
{
    public class StudentTrioService
    {
        private readonly IStudentTrioRepository _studentTrioRepository;

        public StudentTrioService(IStudentTrioRepository studentTrioRepository)
        {
            _studentTrioRepository = studentTrioRepository;
        }

        public async Task<ServiceResponseDTO> SaveStudentTrio(StudentTrio studentTrio)
        {
            // Check if trio has capacity (max 3 students)
            var capacityCheck = await _studentTrioRepository.CheckTrioCapacity(studentTrio.StudentId, studentTrio.TrioId);
            
            if (!capacityCheck.HasCapacity)
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, false, capacityCheck.Message);
            }

            bool isSaved = await _studentTrioRepository.SaveStudentTrio(studentTrio);
            
            return new ServiceResponseDTO(
                isSaved, 
                isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, 
                result: studentTrio.Id, 
                isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> GetStudentTrios()
        {
            var studentTrios = await _studentTrioRepository.GetStudentTrios();
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, studentTrios, MessageSuccess.Found);
            return response;
        }

        public async Task<ServiceResponseDTO> GetStudentTrio(int id)
        {
            var studentTrio = await _studentTrioRepository.GetStudentTrio(id);
            ServiceResponseDTO response = new(true, AppStatusCodes.Success, studentTrio, MessageSuccess.Found);
            return response;
        }
    }
}

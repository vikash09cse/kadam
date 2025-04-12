using Core.Abstractions;
using Core.DTOs;
using Core.Entities;
using Core.Utilities;
using static Core.Utilities.Enums;

namespace Core.Features.Admin
{
    public class StudentDocumentService
    {
        private readonly IStudentDocumentRepository _studentDocumentRepository;

        public StudentDocumentService(IStudentDocumentRepository studentDocumentRepository)
        {
            _studentDocumentRepository = studentDocumentRepository;
        }

        public async Task<ServiceResponseDTO> SaveStudentDocument(StudentDocument document)
        {
            bool isSaved = await _studentDocumentRepository.SaveStudentDocument(document);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, 
                result: document.Id, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> DeleteStudentDocument(int id, int userId)
        {
            var isDeleted = await _studentDocumentRepository.DeleteStudentDocument(id, userId);
            return new ServiceResponseDTO(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, 
                isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
        }

        public async Task<ServiceResponseDTO> GetStudentDocument(int id)
        {
            var document = await _studentDocumentRepository.GetStudentDocument(id);
            return new ServiceResponseDTO(true, AppStatusCodes.Success, document, MessageSuccess.Found);
        }

        public async Task<ServiceResponseDTO> GetStudentDocumentsByStudentId(int studentId)
        {
            var documents = await _studentDocumentRepository.GetStudentDocumentsByStudentId(studentId);
            return new ServiceResponseDTO(true, AppStatusCodes.Success, documents, MessageSuccess.Found);
        }

        public async Task<ServiceResponseDTO> GetAllStudentDocuments()
        {
            var documents = await _studentDocumentRepository.GetAllStudentDocuments();
            return new ServiceResponseDTO(true, AppStatusCodes.Success, documents, MessageSuccess.Found);
        }
    }
} 
using Core.Abstractions;
using Core.DTOs;
using Core.Entities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;

namespace Infrastructure
{
    public class InstitutionRepository(IDbSession db, DatabaseContext context) : IInstitutionRepository
    {
        private readonly IDbSession _db = db;
        private readonly DatabaseContext _context = context;
        public async Task<bool> AssignPartnerToInstitution(IEnumerable<InstitutionPartner> partners)
        {
            // Delete all existing partners for the institution
            var existingPartners = await _context.InstitutionPartners.Where(x => partners.Select(p => p.InstitutionId).Contains(x.InstitutionId)).ToListAsync();
            _context.InstitutionPartners.RemoveRange(existingPartners);

            foreach (var partner in partners)
            {
                var institutionPartner = new InstitutionPartner
                {
                    InstitutionId = partner.InstitutionId,
                    PartnerId = partner.PartnerId
                };
                _context.InstitutionPartners.Add(institutionPartner);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AssignProjectToInstitution(IEnumerable<InstitutionProject> projects)
        {
            // Delete all existing projects for the institution
            var existingProjects = await _context.InstitutionProjects.Where(x => projects.Select(p => p.InstitutionId).Contains(x.InstitutionId)).ToListAsync();
            _context.InstitutionProjects.RemoveRange(existingProjects); 

            foreach (var project in projects)
            {
                var institutionProject = new InstitutionProject
                {
                    InstitutionId = project.InstitutionId,
                    ProjectId = project.ProjectId
                };
                _context.InstitutionProjects.Add(institutionProject);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CheckInstitutionCode(string code, int id)
        {
            var institution = await _context.Institutions.FirstOrDefaultAsync(x => x.InstitutionCode == code && x.Id != id && x.IsDeleted == false);
            if (institution != null)
            { 
                return true; 
            }
            return false;
        }

        public async Task<bool> CheckInstitutionEmail(string email, int id)
        {
            var institution = await _context.Institutions.FirstOrDefaultAsync(x => x.InstitutionEmail == email && x.Id != id && x.IsDeleted == false);
            if (institution != null)
            { 
                return true; 
            }
            return false;
        }

        public async Task<bool> CheckInstitutionId(string institutionId, int id)
        {
            var institution = await _context.Institutions.FirstOrDefaultAsync(x => x.InstitutionId == institutionId && x.Id != id && x.IsDeleted == false);
            if (institution != null)
            { 
                return true; 
            }
            return false;
        }

        public async Task<bool> CheckInstitutionName(string name, int id)
        {
            var institution = await _context.Institutions.FirstOrDefaultAsync(x => x.InstitutionName == name && x.Id != id && x.IsDeleted == false);
            if (institution != null)
            { 
                return true; 
            }
            return false;
        }

        public async Task<bool> UpdateStatusInstitution(int id, int userId, Core.Utilities.Enums.Status status, string reason)
        {
            var institution = await _context.Institutions.FirstOrDefaultAsync(x => x.Id == id);
            if (institution == null) return false;

            institution.CurrentStatus = status;
            institution.ModifyBy = userId;
            institution.ModifyDate = DateTime.Now;

            if (status == Core.Utilities.Enums.Status.Closed)
            {
                institution.ClosedBy = userId;
                institution.ClosedDate = DateTime.Now;
                institution.ClosedReason = reason;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteInstitution(int id, int userId)
        {
            var institution = await _context.Institutions.FirstOrDefaultAsync(x => x.Id == id);
            if (institution != null)
            {
                institution.IsDeleted = true;
                institution.DeletedBy = userId;
                institution.DeletedDate = DateTime.Now;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;

        }

        public async Task<InstitutionSave> GetInstitutionById(int id)
        {
            var institution = await _context.Institutions.FirstOrDefaultAsync(x => x.Id == id);
            if (institution == null) 
            {
                return null;
            }
            InstitutionSave institutionDetails = new InstitutionSave();
            institutionDetails.Id = institution.Id;
            institutionDetails.DivisionId = institution.DivisionId;
            institutionDetails.StateId = institution.StateId;
            institutionDetails.DistrictId = institution.DistrictId;
            institutionDetails.BlockId = institution.BlockId;
            institutionDetails.VillageId = institution.VillageId;
            institutionDetails.InstitutionType = institution.InstitutionType;
            institutionDetails.InstitutionBuilding = institution.InstitutionBuilding;
            institutionDetails.InstitutionName = institution.InstitutionName;
            institutionDetails.InstitutionCode = institution.InstitutionCode;
            institutionDetails.InstitutionId = institution.InstitutionId;
            institutionDetails.InstitutionHeadMasterName = institution.InstitutionHeadMasterName;
            institutionDetails.InstitutionPhone = institution.InstitutionPhone;
            institutionDetails.InstitutionEmail = institution.InstitutionEmail;
            institutionDetails.InstitutionWebsite = institution.InstitutionWebsite;
            institutionDetails.InstitutionLogo = institution.InstitutionLogo;
            institutionDetails.InstitutionAddress = institution.InstitutionAddress;
            institutionDetails.InstitutionMaleTeacherCount = institution.InstitutionMaleTeacherCount;
            institutionDetails.InstitutionFemaleTeacherCount = institution.InstitutionFemaleTeacherCount;
            institutionDetails.InstitutionTotalTeacherCount = institution.InstitutionTotalTeacherCount;
            institutionDetails.InstitutionTotalStudentCount = institution.InstitutionTotalStudentCount;
            institutionDetails.FinancialYearStart = institution.FinancialYearStart;
            institutionDetails.FinancialYearEnd = institution.FinancialYearEnd;
            institutionDetails.CurrentStatus = institution.CurrentStatus;
            institutionDetails.CreatedBy = institution.CreatedBy;
            institutionDetails.ModifyBy = institution.ModifyBy;

            var gradeSections = await _context.InstitutionGradeSections.Where(x => x.InstitutionId == id).ToListAsync();
            institutionDetails.GradeSections = gradeSections;

            return institutionDetails;
        }

        public async Task<IEnumerable<InstitutionListDTO>> GetInstitutions(int pageNumber, int pageSize, Core.Utilities.Enums.Status? currentStatus, string searchTerm)
        {
            var storedProcedure = "dbo.usp_GetInstitutions";
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@CurrentStatus", currentStatus);
            parameters.Add("@SearchTerm", searchTerm);

            var institutions = await _db.Connection.QueryAsync<InstitutionListDTO>(
                storedProcedure,
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure
            );
            return institutions;
        }

        public async Task<bool> SaveInstitution(InstitutionSave institution)
        {
            try
            {
                string jsonGradeSection = JsonConvert.SerializeObject(institution.GradeSections);
                var storedProcedure = "dbo.usp_UpsertInstitution";
                var parameters = new DynamicParameters();
                parameters.Add("@Id", institution.Id);
                parameters.Add("@DivisionId", institution.DivisionId);
                parameters.Add("@StateId", institution.StateId);
                parameters.Add("@DistrictId", institution.DistrictId);
                parameters.Add("@BlockId", institution.BlockId);
                parameters.Add("@VillageId", institution.VillageId);
                parameters.Add("@InstitutionType", institution.InstitutionType);
                parameters.Add("@InstitutionBuilding", institution.InstitutionBuilding);
                parameters.Add("@InstitutionName", institution.InstitutionName);
                parameters.Add("@InstitutionCode", institution.InstitutionCode);
                parameters.Add("@InstitutionId", institution.InstitutionId);
                parameters.Add("@InstitutionHeadMasterName", institution.InstitutionHeadMasterName);
                parameters.Add("@InstitutionPhone", institution.InstitutionPhone);
                parameters.Add("@InstitutionEmail", institution.InstitutionEmail);
                parameters.Add("@InstitutionWebsite", institution.InstitutionWebsite);
                parameters.Add("@InstitutionLogo", institution.InstitutionLogo);
                parameters.Add("@InstitutionAddress", institution.InstitutionAddress);
                parameters.Add("@InstitutionMaleTeacherCount", institution.InstitutionMaleTeacherCount);
                parameters.Add("@InstitutionFemaleTeacherCount", institution.InstitutionFemaleTeacherCount);
                parameters.Add("@InstitutionTotalTeacherCount", institution.InstitutionTotalTeacherCount);
                parameters.Add("@InstitutionTotalStudentCount", institution.InstitutionTotalStudentCount);
                parameters.Add("@FinancialYearStart", institution.FinancialYearStart);
                parameters.Add("@FinancialYearEnd", institution.FinancialYearEnd);
                parameters.Add("@CurrentStatus", institution.CurrentStatus);
                parameters.Add("@CreatedBy", institution.CreatedBy);
                parameters.Add("@ModifyBy", institution.ModifyBy);
                parameters.Add("@jsonGradeSection", jsonGradeSection);

                return await _db.Connection.ExecuteAsync(storedProcedure, parameters, _db.Transaction, commandType: CommandType.StoredProcedure) > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

        public async Task<IEnumerable<DropdownDTO>> GetInstitutionsByVillageId(int villageId, int institutionTypeId)
        {
            var storedProcedure = "dbo.usp_GetInstitutionByVillageId";
            var parameters = new DynamicParameters();
            parameters.Add("@VillageId", villageId);
            parameters.Add("@InstitutionTypeId", institutionTypeId);

            var institutions = await _db.Connection.QueryAsync<DropdownDTO>(
                storedProcedure,
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure
            );
            return institutions;
        }
    }
}

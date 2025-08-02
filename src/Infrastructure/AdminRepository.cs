using Core.Abstractions;
using Core.DTOs;
using Core.DTOs.Users;
using Core.Entities;
using Core.Utilities;
using Dapper;
using Kadam.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;

namespace Infrastructure
{
    public class AdminRepository(IDbSession db, DatabaseContext context) : IAdminRepository
    {
        private readonly IDbSession _db = db;
        private readonly DatabaseContext _context = context;

        #region "Users"
        public async Task<bool> CheckUserExist(string email, int id)
        {
            var SP = DBConstant.SP.usp_Users;
            var P = new DynamicParameters();
            P.Add(DBConstant.Param.QueryType, 1);
            P.Add(DBConstant.Param.Id, id);
            P.Add(DBConstant.Param.Email, email);
            var rowCount = await _db.Connection
                .ExecuteScalarAsync<int>(SP, P, _db.Transaction, null, CommandType.StoredProcedure);
            return rowCount > 0;
        }

        public async Task<bool> DeleteUser(int id)
        {
            var SP = DBConstant.SP.usp_Users;
            var P = new DynamicParameters();
            P.Add(DBConstant.Param.QueryType, 2);
            P.Add(DBConstant.Param.Id, id);
            var rowEffected = await _db.Connection.ExecuteAsync(SP, P, _db.Transaction, null, CommandType.StoredProcedure);
            return rowEffected > 0;
        }

        public async Task<Users> GetUser(int id)
        {
            var SP = DBConstant.SP.usp_Users;
            var P = new DynamicParameters();
            P.Add(DBConstant.Param.QueryType, 3);
            P.Add(DBConstant.Param.Id, id);
            var _user = await _db.Connection.QueryFirstOrDefaultAsync<Users>(SP, P, _db.Transaction, null, CommandType.StoredProcedure);
            return _user;
        }

        public async Task<IEnumerable<Users>> GetUsers()
        {
            var SP = DBConstant.SP.usp_Users;
            var P = new DynamicParameters();
            P.Add(DBConstant.Param.QueryType, 4);
            var _users = await _db.Connection.QueryAsync<Users>(SP, P, _db.Transaction, null, CommandType.StoredProcedure);
            return _users;
        }
        public async Task<bool> SaveUser(Users user)
        {
            try
            {
                var SP = DBConstant.SP.usp_UpsertUser;
                var P = new DynamicParameters();
                P.Add(DBConstant.Param.Id, user.Id);
                P.Add(DBConstant.Param.Email, user.Email);
                P.Add(DBConstant.Param.UserName, user.UserName);
                P.Add(DBConstant.Param.PasswordHash, user.PasswordHash);
                P.Add(DBConstant.Param.PasswordSalt, user.PasswordSalt);
                P.Add(DBConstant.Param.FirstName, user.FirstName);
                P.Add(DBConstant.Param.LastName, user.LastName);
                P.Add(DBConstant.Param.Phone, user.Phone);
                P.Add(DBConstant.Param.AlternatePhone, user.AlternatePhone);
                P.Add(DBConstant.Param.Gender, user.Gender);
                P.Add(DBConstant.Param.Grade, user.Grade);
                P.Add(DBConstant.Param.Section, user.Section);
                P.Add(DBConstant.Param.GradeSection, user.GradeSection);
                P.Add(DBConstant.Param.DivisionId, user.DivisionId);
                P.Add(DBConstant.Param.RoleId, user.RoleId);
                P.Add(DBConstant.Param.ReporteeRoleId, user.ReporteeRoleId);
                P.Add(DBConstant.Param.UserStatus, user.UserStatus);
                P.Add(DBConstant.Param.ActivityType, user.ActivityType);
                P.Add(DBConstant.Param.DateCreated, user.DateCreated);
                P.Add(DBConstant.Param.CreatedBy, user.CreatedBy);
                P.Add(DBConstant.Param.ModifyDate, user.ModifyDate);
                P.Add(DBConstant.Param.ModifyBy, user.ModifyBy);
                var rowEffected = await _db.Connection.ExecuteAsync(SP, P, _db.Transaction, null, CommandType.StoredProcedure);
                return rowEffected > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

        public async Task<UserLoginValidateDTO> ValidateUser(string userName)
        {
            var SP = DBConstant.SP.usp_UserLoginValidate;
            var P = new DynamicParameters();
            P.Add(DBConstant.Param.UserName, userName);
            

            var _user = await _db.Connection.QueryFirstOrDefaultAsync<UserLoginValidateDTO>(SP, P, _db.Transaction, null, CommandType.StoredProcedure);
            return _user ?? new UserLoginValidateDTO();
        }

        public async Task<IEnumerable<UserListDTO>> GetUserList(int pageNumber, int pageSize, string searchTerm)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);

            var result = await _db.Connection.QueryAsync<UserListDTO>(
                "dbo.usp_GetUserList",
                parameters,
                _db.Transaction,
                null,
                CommandType.StoredProcedure
            );

            return result;
        }
        public async Task<IEnumerable<UserProgramDTO>> GetUserPrograms(int userId)
        {
            var SP = "dbo.usp_GetUserPrograms";
            var P = new DynamicParameters();
            P.Add("@UserId", userId);
            var _userProgram = await _db.Connection.QueryAsync<UserProgramDTO>(SP, P, _db.Transaction, null, CommandType.StoredProcedure);
            return _userProgram ?? new List<UserProgramDTO>();
        }
        public async Task<bool> SaveUserPrograms(IEnumerable<UserProgram> userPrograms)
        {
            try
            {
                if (userPrograms.Any())
                {
                    var userId = userPrograms.First().UserId;
                    var existingUserPrograms = _context.UserPrograms.Where(up => up.UserId == userId);
                    _context.UserPrograms.RemoveRange(existingUserPrograms);
                }

                foreach (var userProgram in userPrograms)
                {
                    _context.UserPrograms.Add(userProgram);
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                // Log the exception
                return false;
            }
        }

        public async Task<bool> SavePeopleInstitution(PeopleInstitution peopleInstitution)
        {
            if (peopleInstitution.Id > 0)
            {
                _context.PeopleInstitutions.Update(peopleInstitution);
            }
            else
            {
                _context.PeopleInstitutions.Add(peopleInstitution);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<PeopleInstitution> GetPeopleInstitution(int userId)
        {
            return await _context.PeopleInstitutions.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        #endregion

        #region "Division"
        public async Task<bool> SaveDivision(Division division)
        {
            if (division.Id > 0)
            {
                var filterDivision = _context.Divisions.FirstOrDefault(x => x.Id == division.Id);
                if (filterDivision != null)
                {
                    filterDivision.DivisionName = division.DivisionName;
                    filterDivision.DivisionCode = division.DivisionCode;
                    filterDivision.ModifyDate = DateTime.UtcNow;
                    filterDivision.ModifyBy = division.ModifyBy;
                    _context.Divisions.Update(filterDivision);
                }
            }
            else
            {
                _context.Divisions.Add(division);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Division> GetDivision(int id)
        {
            return await _context.Divisions.FirstOrDefaultAsync(x => x.Id == id) ?? new Division();
        }

        public async Task<IEnumerable<DivisionListDTO>> GetDivisionList(int pageNumber, int pageSize, string searchTerm)
        {
            var storedProcedure = "dbo.usp_DivisionList";
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);

            var divisions = await _db.Connection.QueryAsync<DivisionListDTO>(
                storedProcedure,
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure
            );

            return divisions;
        }

        public async Task<bool> DeleteDivision(int id,int deletedBy)
        {
            var division = await _context.Divisions.FirstOrDefaultAsync(x => x.Id == id);
            if (division != null)
            {
                division.IsDeleted = true;
                division.DeletedBy = deletedBy;
                division.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> CheckDuplicateDivisionName(string divisionName, int id)
        {
            var division = await _context.Divisions.FirstOrDefaultAsync(x => x.DivisionName == divisionName && x.Id != id && !x.IsDeleted);
            return division != null;

        }

        public async Task<bool> CheckDuplicateDivisionCode(string divisionCode, int id)
        {
            var division = await _context.Divisions.FirstOrDefaultAsync(x => x.DivisionCode == divisionCode && x.Id != id && !x.IsDeleted);
            return division != null;
        }

        public async Task<IEnumerable<DropdownDTO>> GetDivisionsByStatus(Enums.Status? currentStatus)
        {
            var storedProcedure = "dbo.usp_GetDivisionsByStatus";
            var parameters = new DynamicParameters();
            parameters.Add("@CurrentStatus", currentStatus);
            return await _db.Connection.QueryAsync<DropdownDTO>(storedProcedure, parameters, _db.Transaction, commandType: CommandType.StoredProcedure);
        }
        #endregion

        #region "States"
        public async Task<bool> SaveState(State state)
        {
            if (state.Id > 0)
            {
                _context.States.Update(state);
            }
            else
            {
                _context.States.Add(state);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<State> GetState(int id)
        {
            return await _context.States.FirstOrDefaultAsync(x => x.Id == id) ?? new State();
        }
        public async Task<IEnumerable<StateListDTO>> GetStateList(int pageNumber, int pageSize, string searchTerm)
        {
            var storedProcedure = "dbo.usp_GetStates";
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);

            var states = await _db.Connection.QueryAsync<StateListDTO>(
                storedProcedure,
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure
            );

            return states;
        }

        public async Task<bool> DeleteState(int id,int deletedBy)
        {
            var state = await _context.States.FirstOrDefaultAsync(x => x.Id == id);
            if (state != null)
            {
                state.IsDeleted = true;
                state.DeletedBy = deletedBy;
                state.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> CheckDuplicateStateName(string stateName, int id)
        {
            var state = await _context.States.FirstOrDefaultAsync(x => x.StateName == stateName && x.Id != id && !x.IsDeleted);
            return state != null;
        }

        public async Task<bool> CheckDuplicateStateCode(string stateCode, int id)
        {
            var state = await _context.States.FirstOrDefaultAsync(x => x.StateCode == stateCode && x.Id != id && !x.IsDeleted);
            return state != null;
        }   

        public async Task<IEnumerable<DropdownDTO>> GetStatesByStatus(Enums.Status currentStatus)
        {
            var storedProcedure = "dbo.usp_StatesByStatus";
            var parameters = new DynamicParameters();
            parameters.Add("@CurrentStatus", currentStatus);
            return await _db.Connection.QueryAsync<DropdownDTO>(storedProcedure, parameters, _db.Transaction, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> CloseState(int id, int closedBy)
        {
            var state = await _context.States.FirstOrDefaultAsync(x => x.Id == id);
            state.CurrentStatus = Enums.Status.Closed;
            state.ModifyDate = DateTime.UtcNow;
            state.ModifyBy = closedBy;
            return await _context.SaveChangesAsync() > 0;
        }
        #endregion 

        #region "District"
        public async Task<bool> SaveDistrict(District district)
        {
            if (district.Id > 0)
            {
                _context.Districts.Update(district);
            }
            else
            {
                _context.Districts.Add(district);
            }
            return await _context.SaveChangesAsync() > 0;
        }

            public async Task<District> GetDistrict(int id)
        {
            return await _context.Districts.FirstOrDefaultAsync(x => x.Id == id) ?? new District();
        }

        public async Task<IEnumerable<DistrictListDTO>> GetDistrictList(int pageNumber, int pageSize, string searchTerm)
        {
            var storedProcedure = "dbo.usp_GetDistricts";
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);

            var districts = await _db.Connection.QueryAsync<DistrictListDTO>(
                storedProcedure,
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure
            );
            return districts;
        }

        public async Task<bool> DeleteDistrict(int id,int deletedBy)
        {
            var district = await _context.Districts.FirstOrDefaultAsync(x => x.Id == id);
            if (district != null)
            {
                district.IsDeleted = true;
                district.DeletedBy = deletedBy;
                district.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> CheckDuplicateDistrictName(string districtName, int id, int stateId)
        {
            var district = await _context.Districts.FirstOrDefaultAsync(x => x.DistrictName == districtName && x.Id != id && !x.IsDeleted && x.StateId == stateId);
            return district != null;
        }   

        public async Task<bool> CheckDuplicateDistrictCode(string districtCode, int id, int stateId)
        {
            var district = await _context.Districts.FirstOrDefaultAsync(x => x.DistrictCode == districtCode && x.Id != id && !x.IsDeleted && x.StateId == stateId);
            return district != null;
        }

        public async Task<IEnumerable<DropdownDTO>> GetDistrictsByState(int stateId)
        {
            var storedProcedure = "dbo.usp_GetDistrictByStateId";
            var parameters = new DynamicParameters();
            parameters.Add("@StateId", stateId);
            return await _db.Connection.QueryAsync<DropdownDTO>(storedProcedure, parameters, _db.Transaction, commandType: CommandType.StoredProcedure);
        }
        #endregion
    
        #region "Blocks"
        public async Task<bool> SaveBlock(Block block)
        {
            if (block.Id > 0)
            {
                _context.Blocks.Update(block);
            }
            else
            {
                _context.Blocks.Add(block);
            }
            return await _context.SaveChangesAsync() > 0;
        }   

        public async Task<Block> GetBlock(int id)
        {
            return await _context.Blocks.FirstOrDefaultAsync(x => x.Id == id) ?? new Block();
        }

        public async Task<IEnumerable<BlockListDTO>> GetBlockList(int pageNumber, int pageSize, string searchTerm)
        {
            var storedProcedure = "dbo.usp_Blocks";
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);

            var blocks = await _db.Connection.QueryAsync<BlockListDTO>(
                storedProcedure,
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure
            );
            return blocks;          
        }

        public async Task<bool> DeleteBlock(int id,int deletedBy)
        {
            var block = await _context.Blocks.FirstOrDefaultAsync(x => x.Id == id);
            if (block != null)
            {
                block.IsDeleted = true;
                block.DeletedBy = deletedBy;
                block.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> CheckDuplicateBlockName(string blockName, int id, int districtId)
        {
            var block = await _context.Blocks.FirstOrDefaultAsync(x => x.BlockName == blockName && x.Id != id && !x.IsDeleted && x.DistrictId == districtId);
            return block != null;
        }

        public async Task<IEnumerable<DropdownDTO>> GetBlocksByDistrict(int districtId)
        {
            var storedProcedure = "dbo.usp_GetBlockByDistrictId";
            var parameters = new DynamicParameters();
            parameters.Add("@DistrictId", districtId);
            return await _db.Connection.QueryAsync<DropdownDTO>(storedProcedure, parameters, _db.Transaction, commandType: CommandType.StoredProcedure);
        }
        #endregion

        #region "Villages"
        public async Task<IEnumerable<VillageListDTO>> GetVillageList(int pageNumber, int pageSize, string searchTerm)
        {
            var storedProcedure = "dbo.usp_Villages";
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);

            var villages = await _db.Connection.QueryAsync<VillageListDTO>(
                storedProcedure,
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure
            );
            return villages;
        }

        public async Task<bool> SaveVillage(Village village)
        {
            if (village.Id > 0)
            {
                _context.Villages.Update(village);
            }
            else
            {
                _context.Villages.Add(village);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Village> GetVillage(int id)
        {
            return await _context.Villages.FirstOrDefaultAsync(x => x.Id == id) ?? new Village();
        }

        public async Task<bool> DeleteVillage(int id, int deletedBy)
        {
            var village = await _context.Villages.FirstOrDefaultAsync(x => x.Id == id);
            if (village != null)
            {
                village.IsDeleted = true;
                village.DeletedBy = deletedBy;
                village.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> CheckDuplicateVillageName(string villageName, int id, int blockId)
        {
            var village = await _context.Villages.FirstOrDefaultAsync(x => x.VillageName == villageName && x.Id != id && !x.IsDeleted && x.BlockId == blockId);
            return village != null;
        }
        public async Task<IEnumerable<DropdownDTO>> GetVillagesByBlock(int blockId)
        {
            var storedProcedure = "dbo.usp_GetVillageByBlockId";
            var parameters = new DynamicParameters();
            parameters.Add("@BlockId", blockId);
            return await _db.Connection.QueryAsync<DropdownDTO>(storedProcedure, parameters, _db.Transaction, commandType: CommandType.StoredProcedure);
        }
        public async Task<IEnumerable<GradeSectionDTO>> GetGradesAndSections()
        {
            var storedProcedure = "usp_GetGradesAndSections";
            var multi = await _db.Connection.QueryMultipleAsync(storedProcedure, null, _db.Transaction, commandType: CommandType.StoredProcedure);
            var grades = (await multi.ReadAsync<GradeSectionDTO>()).ToList();
            var sections = (await multi.ReadAsync<SectionDTO>()).ToList();

            grades.ForEach(grade => grade.Sections = sections);

            return grades;
        }
        #endregion

        #region "Programs"
        public async Task<IEnumerable<ProgramListDTO>> GetProgramList(int pageNumber, int pageSize, string searchTerm)
        {
            var storedProcedure = "dbo.usp_GetPrograms";
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);
            var programs = await _db.Connection.QueryAsync<ProgramListDTO>(
                storedProcedure,
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure
            );
            return programs;
        }
        public async Task<bool> SaveProgram(Program program)
        {
            if (program.Id > 0)
            {
                _context.Programs.Update(program);
            }
            else
            {
                _context.Programs.Add(program);
            }
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<Program> GetProgram(int id)
        {
            return await _context.Programs.FirstOrDefaultAsync(x => x.Id == id) ?? new Program();
        }
        public async Task<bool> DeleteProgram(int id, int deletedBy)
        {
            var program = await _context.Programs.FirstOrDefaultAsync(x => x.Id == id);
            if (program != null)
            {
                program.IsDeleted = true;
                program.DeletedBy = deletedBy;
                program.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<bool> CheckDuplicateProgramName(string programName, int id)
        {
            var program = await _context.Programs.FirstOrDefaultAsync(x => x.ProgramName == programName && x.Id != id && !x.IsDeleted);
            return program != null;
        }

        #endregion

        #region "Roles"
        public async Task<IEnumerable<RoleListDTO>> GetRoleList(int pageNumber, int pageSize, string searchTerm)
        {
            var storedProcedure = "dbo.usp_GetRoles";
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);
            var roles = await _db.Connection.QueryAsync<RoleListDTO>(
                storedProcedure,
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure
            );
            return roles;
        }
        public async Task<bool> SaveRole(Role role)
        {
            if (role.Id > 0)
            {
                _context.Roles.Update(role);
            }
            else
            {
                _context.Roles.Add(role);
            }
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<Role> GetRole(int id)
        {
            return await _context.Roles.FirstOrDefaultAsync(x => x.Id == id) ?? new Role();
        }
        public async Task<bool> DeleteRole(int id, int deletedBy)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == id);
            if (role != null)
            {
                role.IsDeleted = true;
                role.DeletedBy = deletedBy;
                role.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<bool> CheckDuplicateRoleName(string roleName, int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.RoleName == roleName && x.Id != id && !x.IsDeleted);
            return role != null;
        }
        public async Task<IEnumerable<RolePermission>> GetRolePermissions(int roleId)
        {
            return await _context.RolePermissions.Where(x => x.RoleId == roleId).ToListAsync();
        }
        public async Task<bool> SaveRolePermissions(RolePermissionsDTO rolePermissions, int createdBy)
        {
            var rolePermission = await _context.RolePermissions.Where(x => x.RoleId == rolePermissions.RoleId).ToListAsync();
            _context.RolePermissions.RemoveRange(rolePermission);
            foreach (var menuId in rolePermissions.PermissionIds)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = rolePermissions.RoleId,
                    MenuId = menuId,
                    CreatedBy = createdBy,
                    DateCreated = DateTime.UtcNow
                });
            }
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<IEnumerable<MenuPermissionDTO>> GetMenuPermissions()
        {
            return await Task.FromResult(_context.MenuPermissions.ToList().Select(x => new MenuPermissionDTO
            {
                Id = x.Id,
                MenuName = x.MenuName,
            }));
        }
        public async Task<IEnumerable<DropdownDTO>> GetRolesDropDown()
        {
            var storedProcedure = "dbo.usp_GetRolesDropDown";
            return await _db.Connection.QueryAsync<DropdownDTO>(storedProcedure, null, _db.Transaction, commandType: CommandType.StoredProcedure);
        }


        #endregion

        #region "Subjects"
        public async Task<IEnumerable<SubjectListDTO>> GetSubjectList(int pageNumber, int pageSize, string searchTerm)
        {
            var storedProcedure = "dbo.usp_GetSubjects";
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);
            var subjects = await _db.Connection.QueryAsync<SubjectListDTO>(
                storedProcedure,
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure
            );
            return subjects;
        }
        public async Task<bool> SaveSubject(Subject subject)
        {
            if (subject.Id > 0)
            {
                _context.Subjects.Update(subject);
            }
            else
            {
                _context.Subjects.Add(subject);
            }
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<Subject> GetSubject(int id)
        {
            return await _context.Subjects.FirstOrDefaultAsync(x => x.Id == id) ?? new Subject();
        }
        public async Task<bool> DeleteSubject(int id, int deletedBy)
        {
            var subject = await _context.Subjects.FirstOrDefaultAsync(x => x.Id == id);
            if (subject != null)
            {
                subject.IsDeleted = true;
                subject.DeletedBy = deletedBy;
                subject.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<bool> CheckDuplicateSubjectName(string subjectName, int id)
        {
            var subject = await _context.Subjects.FirstOrDefaultAsync(x => x.SubjectName == subjectName && x.Id != id && !x.IsDeleted);
            return subject != null;
        }
        #endregion

        #region "Steps"
        public async Task<IEnumerable<StepListDTO>> GetStepList(int pageNumber, int pageSize, string searchTerm)
        {
            var storedProcedure = "dbo.usp_GetSteps";
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);
            var steps = await _db.Connection.QueryAsync<StepListDTO>(
                storedProcedure,
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure
            );
            return steps;
        }
        public async Task<bool> SaveStep(Step step)
        {
            if (step.Id > 0)
            {
                _context.Steps.Update(step);
            }
            else
            {
                _context.Steps.Add(step);
            }
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<Step> GetStep(int id)
        {
            return await _context.Steps.FirstOrDefaultAsync(x => x.Id == id) ?? new Step();
        }
        public async Task<bool> DeleteStep(int id, int deletedBy)
        {
            var step = await _context.Steps.FirstOrDefaultAsync(x => x.Id == id);
            if (step != null)
            {
                step.IsDeleted = true;
                step.DeletedBy = deletedBy;
                step.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<bool> CheckDuplicateStepName(string stepName, int id)
        {
            var step = await _context.Steps.FirstOrDefaultAsync(x => x.StepName == stepName && x.Id != id && !x.IsDeleted);
            return step != null;
        }
        #endregion

        #region "Themes"
        public async Task<IEnumerable<ThemeListDTO>> GetThemeList(int pageNumber, int pageSize, string searchTerm)
        {
            var storedProcedure = "dbo.usp_GetThemes";
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);
            var themes = await _db.Connection.QueryAsync<ThemeListDTO>(
                storedProcedure,
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure
            );
            return themes;
        }
        public async Task<bool> SaveTheme(Theme theme)
        {
            if (theme.Id > 0)
            {
                _context.Themes.Update(theme);
            }
            else
            {
                _context.Themes.Add(theme);
            }
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<Theme> GetTheme(int id)
        {
            return await _context.Themes.FirstOrDefaultAsync(x => x.Id == id) ?? new Theme();
        }
        public async Task<bool> DeleteTheme(int id, int deletedBy)
        {
            var theme = await _context.Themes.FirstOrDefaultAsync(x => x.Id == id);
            if (theme != null)
            {
                theme.IsDeleted = true;
                theme.DeletedBy = deletedBy;
                theme.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<bool> CheckDuplicateThemeName(string themeName, int id)
        {
            var theme = await _context.Themes.FirstOrDefaultAsync(x => x.ThemeName == themeName && x.Id != id && !x.IsDeleted);
            return theme != null;
        }

        public async Task<IEnumerable<DropdownDTO>> GetActiveThemes()
        {
            var storedProcedure = "dbo.usp_GetActiveThemes";
            return await _db.Connection.QueryAsync<DropdownDTO>(storedProcedure, null, _db.Transaction, commandType: CommandType.StoredProcedure);
        }
        #endregion

        #region "Menu Permission"
        public async Task<IEnumerable<MenuPermissionListDTO>> GetMenuPermissionList(int pageNumber, int pageSize, string searchTerm)
        {
            var storedProcedure = "dbo.usp_GetMenuPermissions";
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);
            var menuPermissions = await _db.Connection.QueryAsync<MenuPermissionListDTO>(
                storedProcedure,
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure
            );
            return menuPermissions;
        }
        public async Task<bool> SaveMenuPermission(MenuPermission menuPermission)
        {
            if (menuPermission.Id > 0)
            {
                _context.MenuPermissions.Update(menuPermission);
            }
            else
            {
                _context.MenuPermissions.Add(menuPermission);
            }
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<MenuPermission> GetMenuPermission(int id)
        {
            return await _context.MenuPermissions.FirstOrDefaultAsync(x => x.Id == id) ?? new MenuPermission();
        }
        public async Task<bool> DeleteMenuPermission(int id, int deletedBy)
        {
            var menuPermission = await _context.MenuPermissions.FirstOrDefaultAsync(x => x.Id == id);
            if (menuPermission != null)
            {
                menuPermission.IsDeleted = true;
                menuPermission.DeletedBy = deletedBy;
                menuPermission.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<IEnumerable<DropdownDTO>> GetMenus()
        {
            var storedProcedure = "dbo.usp_GetMenusDropdown";
            return await _db.Connection.QueryAsync<DropdownDTO>(storedProcedure, null, _db.Transaction, commandType: CommandType.StoredProcedure);
        }
        public async Task<bool> CheckDuplicateMenuName(string menuName, int id)
        {
            var menu = await _context.MenuPermissions.FirstOrDefaultAsync(x => x.MenuName == menuName && x.Id != id && !x.IsDeleted);
            return menu != null;
        }
        #endregion
    }
}

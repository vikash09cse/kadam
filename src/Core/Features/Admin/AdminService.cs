using Core.Abstractions;
using Core.DTOs;
using Core.DTOs.Users;
using Core.Entities;
using Core.Utilities;
using Kadam.Core.DTOs;

namespace Core.Features.Admin
{
    public class AdminService //: IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }
        #region "User"
        public async Task<IEnumerable<Users>> GetUsers()
        {
            return await _adminRepository.GetUsers();
        }
        public async Task<Users> GetUser(int id)
        {
            return await _adminRepository.GetUser(id);
        }
        public async Task<ServiceResponseDTO> SaveUser(Users user)
        {
            if (await _adminRepository.CheckUserExist(user.Email, user.Id))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateEmail);
            }
            user.DivisionId = 0;
            user.ActivityType = "";
            user.Grade = "";
            user.Section = "";
            user.GradeSection = "";

            // Generate random password
            var randomPassword = PasswordManagement.GenerateRandomPassword(6, 10);
            var (passwordHash, passwordSalt) = PasswordManagement.HashPassword(randomPassword);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            bool isSaved = await _adminRepository.SaveUser(user);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> DeleteUser(int id)
        {
            ServiceResponseDTO response;
            var isDeleted = await _adminRepository.DeleteUser(id);
            response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
        public async Task<ServiceResponseDTO> ValidateUser(string userName, string password)
        {
            var loginInfo = await _adminRepository.ValidateUser(userName);
            if (loginInfo == null || loginInfo.Id <= 0)
            {
                return new ServiceResponseDTO(false,AppStatusCodes.Unauthorized, false, MessageError.InvalidCredential);
            }
            
            var verified= PasswordManagement.VerifyPassword(password, loginInfo.PasswordHash, loginInfo.PasswordSalt);
            if (!verified)
            {
                return new ServiceResponseDTO(false,AppStatusCodes.Unauthorized, false, MessageError.InvalidCredential);
            }

            return new ServiceResponseDTO(true,AppStatusCodes.Success, loginInfo, MessageSuccess.LoggedIn);
        }
        public async Task<DataTableResponseDTO<UserListDTO>> GetUserList(
            int draw,
            int start,
            int length,
            string searchValue)
        {
            int pageNumber = (start / length) + 1;
            
            var users = await _adminRepository.GetUserList(
                pageNumber: pageNumber,
                pageSize: length,
                searchTerm: searchValue);
            
            var response = new DataTableResponseDTO<UserListDTO>
            {
                Draw = draw,
                RecordsTotal = users.FirstOrDefault()?.TotalCount ?? 0,
                RecordsFiltered = users.FirstOrDefault()?.TotalCount ?? 0,
                Data = users
            };

            return response;
        }
        public async Task<IEnumerable<UserProgramDTO>> GetUserPrograms(int userId)
        {
            return await _adminRepository.GetUserPrograms(userId);
        }
        public async Task<ServiceResponseDTO> SaveUserPrograms(IEnumerable<UserProgram> userPrograms)
        {
            bool isSaved = await _adminRepository.SaveUserPrograms(userPrograms);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> SavePeopleInstitution(PeopleInstitution peopleInstitution)
        {
            bool isSaved = await _adminRepository.SavePeopleInstitution(peopleInstitution);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<PeopleInstitution> GetPeopleInstitution(int userId)
        {
            return await _adminRepository.GetPeopleInstitution(userId);
        }
        #endregion

        #region "Division"
        public async Task<DataTableResponseDTO<DivisionListDTO>> GetDivisions(
            int draw,
            int start,
            int length,
            string searchValue)
        {
            int pageNumber = (start / length) + 1;
            
            var users = await _adminRepository.GetDivisionList(
                pageNumber: pageNumber,
                pageSize: length,
                searchTerm: searchValue);
            
            var response = new DataTableResponseDTO<DivisionListDTO>
            {
                Draw = draw,
                RecordsTotal = users.FirstOrDefault()?.TotalCount ?? 0,
                RecordsFiltered = users.FirstOrDefault()?.TotalCount ?? 0,
                Data = users
            };

            return response;
        }

        public async Task<ServiceResponseDTO> SaveDivision(Division division, int currentUserId)
        {
            if (await _adminRepository.CheckDuplicateDivisionName(division.DivisionName, division.Id))
            {
                return new ServiceResponseDTO(false,AppStatusCodes.BadRequest, true, MessageError.DuplicateDivisionName);
            }
            if (await _adminRepository.CheckDuplicateDivisionCode(division.DivisionCode, division.Id))
            {
                return new ServiceResponseDTO(false,AppStatusCodes.BadRequest, true, MessageError.DuplicateDivisionCode);
            }
            int userId = currentUserId;
            if (division.Id > 0)
            {
                division.ModifyBy = userId;
            }
            else
            {
                division.DateCreated = DateTime.UtcNow;
                division.CreatedBy = userId;
            }

            bool isSaved = await _adminRepository.SaveDivision(division);
            return new ServiceResponseDTO(isSaved,isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> DeleteDivision(int id, int userId)
        {
            ServiceResponseDTO response;
            var isDeleted = await _adminRepository.DeleteDivision(id, userId);
            response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
        public async Task<Division> GetDivision(int id)
        {
            var division = await _adminRepository.GetDivision(id);
            return division;
        }
        public async Task<IEnumerable<DropdownDTO>> GetDivisionsByStatus(Enums.Status? currentStatus)
        {
            return await _adminRepository.GetDivisionsByStatus(currentStatus);
        }
        #endregion

        #region "State"
        public async Task<DataTableResponseDTO<StateListDTO>> GetStates(
            int draw,
            int start,
            int length,
            string searchValue)
        {
            int pageNumber = (start / length) + 1;

            var states = await _adminRepository.GetStateList(
                pageNumber: pageNumber,
                pageSize: length,
                searchTerm: searchValue);

            var response = new DataTableResponseDTO<StateListDTO>
            {
                Draw = draw,
                RecordsTotal = states.FirstOrDefault()?.TotalCount ?? 0,
                RecordsFiltered = states.FirstOrDefault()?.TotalCount ?? 0,
                Data = states
            };
            return response;
        }
        public async Task<ServiceResponseDTO> SaveState(State state, int currentUserId)
        {
            if (await _adminRepository.CheckDuplicateStateName(state.StateName, state.Id))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateStateName);
            }
            if (await _adminRepository.CheckDuplicateStateCode(state.StateCode, state.Id))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateStateCode);
            }

            int userId = currentUserId;
            if (state.Id > 0)
            {
                state.ModifyBy = userId;
            }
            else
            {
                state.CurrentStatus = Enums.Status.Active;
                state.DateCreated = DateTime.UtcNow;
                state.CreatedBy = userId;
            }
            bool isSaved = await _adminRepository.SaveState(state);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> DeleteState(int id, int userId)
        {
            ServiceResponseDTO response;
            var isDeleted = await _adminRepository.DeleteState(id, userId);
            response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
        public async Task<State> GetState(int id)
        {
            var state = await _adminRepository.GetState(id);
            return state;
        }
        public async Task<IEnumerable<DropdownDTO>> GetStatesByStatus(Enums.Status currentStatus)
        {
            return await _adminRepository.GetStatesByStatus(currentStatus);
        }
        public async Task<ServiceResponseDTO> CloseState(int id, int userId)
        {
            ServiceResponseDTO response;
            var isClosed = await _adminRepository.CloseState(id, userId);
            response = new(isClosed, isClosed ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isClosed, isClosed ? MessageSuccess.Closed : MessageError.CodeIssue);
            return response;
        }
        #endregion

        #region "District"
        public async Task<DataTableResponseDTO<DistrictListDTO>> GetDistricts(
            int draw,
            int start,
            int length,
            string searchValue)
        {
            int pageNumber = (start / length) + 1;

            var districts = await _adminRepository.GetDistrictList(
                pageNumber: pageNumber,
                pageSize: length,
                searchTerm: searchValue);

            var response = new DataTableResponseDTO<DistrictListDTO>
            {
                Draw = draw,
                RecordsTotal = districts.FirstOrDefault()?.TotalCount ?? 0,
                RecordsFiltered = districts.FirstOrDefault()?.TotalCount ?? 0,
                Data = districts
            };
            return response;
        }
        public async Task<ServiceResponseDTO> SaveDistrict(District district, int currentUserId)
        {
            if (await _adminRepository.CheckDuplicateDistrictName(district.DistrictName, district.Id, district.StateId))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateDistrictName);
            }
            if (await _adminRepository.CheckDuplicateDistrictCode(district.DistrictCode, district.Id, district.StateId))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateDistrictCode);
            }
            int userId = currentUserId;
            if (district.Id > 0)
            {
                district.ModifyBy = userId;
            }
            else
            {
                district.DateCreated = DateTime.UtcNow;
                district.CreatedBy = userId;
            }
            bool isSaved = await _adminRepository.SaveDistrict(district);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> DeleteDistrict(int id, int userId)
        {
            ServiceResponseDTO response;
            var isDeleted = await _adminRepository.DeleteDistrict(id, userId);
            response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
        public async Task<District> GetDistrict(int id)
        {
            var district = await _adminRepository.GetDistrict(id);
            return district;
        }
        public async Task<IEnumerable<DropdownDTO>> GetDistrictsByState(int stateId)
        {
            return await _adminRepository.GetDistrictsByState(stateId);
        }
        #endregion
    
        #region "Blocks"
        public async Task<DataTableResponseDTO<BlockListDTO>> GetBlocks(
            int draw,
            int start,
            int length,
            string searchValue)
        {
            int pageNumber = (start / length) + 1;

            var blocks = await _adminRepository.GetBlockList(
                pageNumber: pageNumber,
                pageSize: length,
                searchTerm: searchValue);   

            var response = new DataTableResponseDTO<BlockListDTO>
            {
                Draw = draw,
                RecordsTotal = blocks.FirstOrDefault()?.TotalCount ?? 0,
                RecordsFiltered = blocks.FirstOrDefault()?.TotalCount ?? 0,
                Data = blocks
            };
            return response;    
        }
        public async Task<ServiceResponseDTO> SaveBlock(Block block, int currentUserId)
        {
            if (await _adminRepository.CheckDuplicateBlockName(block.BlockName, block.Id, block.DistrictId))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateBlockName);
            }   
            int userId = currentUserId;
            if (block.Id > 0)
            {
                block.ModifyBy = userId;
            }
            else
            {
                block.DateCreated = DateTime.UtcNow;
                block.CreatedBy = userId;
            }
            bool isSaved = await _adminRepository.SaveBlock(block);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> DeleteBlock(int id, int userId)
        {
            ServiceResponseDTO response;
            var isDeleted = await _adminRepository.DeleteBlock(id, userId);
            response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
        public async Task<Block> GetBlock(int id)
        {
            var block = await _adminRepository.GetBlock(id);
            return block;
        }
        public async Task<IEnumerable<DropdownDTO>> GetBlocksByDistrict(int districtId)
        {
            return await _adminRepository.GetBlocksByDistrict(districtId);
        }
        #endregion
    
        #region "Villages"
        public async Task<DataTableResponseDTO<VillageListDTO>> GetVillages(
            int draw,
            int start,
            int length,
            string searchValue)
        {   
            int pageNumber = (start / length) + 1;

            var villages = await _adminRepository.GetVillageList(
                pageNumber: pageNumber,
                pageSize: length,
                searchTerm: searchValue);

            var response = new DataTableResponseDTO<VillageListDTO>
            {
                Draw = draw,
                RecordsTotal = villages.FirstOrDefault()?.TotalCount ?? 0,
                RecordsFiltered = villages.FirstOrDefault()?.TotalCount ?? 0,
                Data = villages
            };
            return response;    
        }
        public async Task<ServiceResponseDTO> SaveVillage(Village village, int currentUserId)
        {
            if (await _adminRepository.CheckDuplicateVillageName(village.VillageName, village.Id, village.BlockId))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateVillageName);
            }
            int userId = currentUserId;
            if (village.Id > 0)
            {
                village.ModifyBy = userId;
            }
            else
            {
                village.DateCreated = DateTime.UtcNow;
                village.CreatedBy = userId;
            }   
            bool isSaved = await _adminRepository.SaveVillage(village);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> DeleteVillage(int id, int userId)
        {
            ServiceResponseDTO response;
            var isDeleted = await _adminRepository.DeleteVillage(id, userId);
            response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
        public async Task<Village> GetVillage(int id)
        {
            var village = await _adminRepository.GetVillage(id);
            return village;
        }
        public async Task<IEnumerable<DropdownDTO>> GetVillagesByBlock(int blockId)
        {
            return await _adminRepository.GetVillagesByBlock(blockId);
        }
        #endregion

        #region "Programs"
        public async Task<DataTableResponseDTO<ProgramListDTO>> GetPrograms(
            int draw,
            int start,
            int length,
            string searchValue)
        {
            int pageNumber = (start / length) + 1;
            var programs = await _adminRepository.GetProgramList(
                pageNumber: pageNumber,
                pageSize: length,
                searchTerm: searchValue);
            var response = new DataTableResponseDTO<ProgramListDTO>
            {
                Draw = draw,
                RecordsTotal = programs.FirstOrDefault()?.TotalCount ?? 0,
                RecordsFiltered = programs.FirstOrDefault()?.TotalCount ?? 0,
                Data = programs
            };
            return response;
        }
        public async Task<ServiceResponseDTO> SaveProgram(Program program, int currentUserId)
        {
            if (await _adminRepository.CheckDuplicateProgramName(program.ProgramName, program.Id))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateProgramName);
            }
            int userId = currentUserId;
            if (program.Id > 0)
            {
                program.ModifyBy = userId;
            }
            else
            {
                program.DateCreated = DateTime.UtcNow;
                program.CreatedBy = userId;
            }
            bool isSaved = await _adminRepository.SaveProgram(program);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> DeleteProgram(int id, int userId)
        {
            ServiceResponseDTO response;
            var isDeleted = await _adminRepository.DeleteProgram(id, userId);
            response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
        public async Task<Program> GetProgram(int id)
        {
            var program = await _adminRepository.GetProgram(id);
            return program;
        }
        #endregion

        #region "Roles"
        public async Task<DataTableResponseDTO<RoleListDTO>> GetRoles(
            int draw,
            int start,
            int length,
            string searchValue)
        {
            int pageNumber = (start / length) + 1;
            var roles = await _adminRepository.GetRoleList(
                pageNumber: pageNumber,
                pageSize: length,
                searchTerm: searchValue);
            var response = new DataTableResponseDTO<RoleListDTO>
            {
                Draw = draw,
                RecordsTotal = roles.FirstOrDefault()?.TotalCount ?? 0,
                RecordsFiltered = roles.FirstOrDefault()?.TotalCount ?? 0,
                Data = roles
            };
            return response;
        }
        public async Task<Role> GetRole(int id)
        {
            return await _adminRepository.GetRole(id);
        }
        public async Task<ServiceResponseDTO> SaveRole(Role role, int currentUserId)
        {
            if (await _adminRepository.CheckDuplicateRoleName(role.RoleName, role.Id))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateRoleName);
            }
            int userId = currentUserId;
            if (role.Id > 0)
            {
                role.ModifyBy = userId;
            }
            else
            {
                role.DateCreated = DateTime.UtcNow;
                role.CreatedBy = userId;
            }
            bool isSaved = await _adminRepository.SaveRole(role);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> DeleteRole(int id, int userId)
        {
            ServiceResponseDTO response;
            var isDeleted = await _adminRepository.DeleteRole(id, userId);
            response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
        public async Task<IEnumerable<MenuPermissionDTO>> GetRolePermissions(int roleId)
        {
            try
            {
                // Get all menu permissions
                var permissions = await _adminRepository.GetMenuPermissions();
                if (permissions == null || !permissions.Any())
                    return Enumerable.Empty<MenuPermissionDTO>();

                // Get the selected permissions for this role
                var selectedMenuIds = await _adminRepository.GetRolePermissions(roleId);

                // Mark permissions as selected if they exist in the role's permissions
                var permissionsList = permissions.ToList();
                for (var i = 0; i < permissionsList.Count; i++)
                {
                    permissionsList[i].IsSelected = selectedMenuIds.Any(x => x.MenuId == permissionsList[i].Id);
                }
                permissions = permissionsList;

                return permissions;
            }
            catch (Exception)
            {
                // Log the exception
                return Enumerable.Empty<MenuPermissionDTO>();
            }
        }
        public async Task<ServiceResponseDTO> SaveRolePermissions(RolePermissionsDTO rolePermissions, int createdBy)
        {
            bool isSaved = await _adminRepository.SaveRolePermissions(rolePermissions, createdBy);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<IEnumerable<DropdownDTO>> GetRolesDropDown()
        {
            return await _adminRepository.GetRolesDropDown();
        }
        #endregion

        #region "Menu Permission"
        public async Task<DataTableResponseDTO<MenuPermissionListDTO>> GetMenuPermissions(
            int draw,
            int start,
            int length,
            string searchValue)
        {
            int pageNumber = (start / length) + 1;
            var menuPermissions = await _adminRepository.GetMenuPermissionList(
                pageNumber: pageNumber,
                pageSize: length,
                searchTerm: searchValue);
            var response = new DataTableResponseDTO<MenuPermissionListDTO>
            {
                Draw = draw,
                RecordsTotal = menuPermissions.FirstOrDefault()?.TotalCount ?? 0,
                RecordsFiltered = menuPermissions.FirstOrDefault()?.TotalCount ?? 0,
                Data = menuPermissions
            };
            return response;
        }
        public async Task<MenuPermission> GetMenuPermission(int id)
        {
            return await _adminRepository.GetMenuPermission(id);
        }
        public async Task<ServiceResponseDTO> SaveMenuPermission(MenuPermission menuPermission, int currentUserId)
        {
            if (await _adminRepository.CheckDuplicateMenuName(menuPermission.MenuName, menuPermission.Id))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateMenuName);
            }
            int userId = currentUserId;
            if (menuPermission.Id > 0)
            {
                menuPermission.ModifyBy = userId;
            }
            else
            {
                menuPermission.DateCreated = DateTime.UtcNow;
                menuPermission.CreatedBy = userId;
            }
            bool isSaved = await _adminRepository.SaveMenuPermission(menuPermission);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> DeleteMenuPermission(int id, int userId)
        {
            ServiceResponseDTO response;
            var isDeleted = await _adminRepository.DeleteMenuPermission(id, userId);
            response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
        public async Task<IEnumerable<DropdownDTO>> GetMenus()
        {
            return await _adminRepository.GetMenus();
        }
        #endregion

        #region "Subjects"
        public async Task<DataTableResponseDTO<SubjectListDTO>> GetSubjects(
            int draw,
            int start,
            int length,
            string searchValue)
        {
            int pageNumber = (start / length) + 1;
            var subjects = await _adminRepository.GetSubjectList(
                pageNumber: pageNumber,
                pageSize: length,
                searchTerm: searchValue);
            var response = new DataTableResponseDTO<SubjectListDTO>
            {
                Draw = draw,
                RecordsTotal = subjects.FirstOrDefault()?.TotalCount ?? 0,
                RecordsFiltered = subjects.FirstOrDefault()?.TotalCount ?? 0,
                Data = subjects
            };
            return response;
        }
        public async Task<ServiceResponseDTO> SaveSubject(Subject subject, int currentUserId)
        {
            if (await _adminRepository.CheckDuplicateSubjectName(subject.SubjectName, subject.Id))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateSubjectName);
            }
            int userId = currentUserId;
            if (subject.Id > 0)
            {
                subject.ModifyBy = userId;
            }
            else
            {
                subject.DateCreated = DateTime.UtcNow;
                subject.CreatedBy = userId;
            }
            bool isSaved = await _adminRepository.SaveSubject(subject);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> DeleteSubject(int id, int userId)
        {
            ServiceResponseDTO response;
            var isDeleted = await _adminRepository.DeleteSubject(id, userId);
            response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
        public async Task<Subject> GetSubject(int id)
        {
            var subject = await _adminRepository.GetSubject(id);
            return subject;
        }
        #endregion

        #region "Steps"
        public async Task<DataTableResponseDTO<StepListDTO>> GetSteps(
            int draw,
            int start,
            int length,
            string searchValue)
        {
            int pageNumber = (start / length) + 1;
            var steps = await _adminRepository.GetStepList(
                pageNumber: pageNumber,
                pageSize: length,
                searchTerm: searchValue);
            var response = new DataTableResponseDTO<StepListDTO>
            {
                Draw = draw,
                RecordsTotal = steps.FirstOrDefault()?.TotalCount ?? 0,
                RecordsFiltered = steps.FirstOrDefault()?.TotalCount ?? 0,
                Data = steps
            };
            return response;
        }
        public async Task<ServiceResponseDTO> SaveStep(Step step, int currentUserId)
        {
            if (await _adminRepository.CheckDuplicateStepName(step.StepName, step.Id))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateStepName);
            }
            int userId = currentUserId;
            if (step.Id > 0)
            {
                step.ModifyBy = userId;
            }
            else
            {
                step.DateCreated = DateTime.UtcNow;
                step.CreatedBy = userId;
            }
            bool isSaved = await _adminRepository.SaveStep(step);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> DeleteStep(int id, int userId)
        {
            ServiceResponseDTO response;
            var isDeleted = await _adminRepository.DeleteStep(id, userId);
            response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
        public async Task<Step> GetStep(int id)
        {
            var step = await _adminRepository.GetStep(id);
            return step;
        }
        #endregion

        #region "Themes"
        public async Task<DataTableResponseDTO<ThemeListDTO>> GetThemes(
            int draw,
            int start,
            int length,
            string searchValue)
        {
            int pageNumber = (start / length) + 1;
            var themes = await _adminRepository.GetThemeList(
                pageNumber: pageNumber,
                pageSize: length,
                searchTerm: searchValue);
            var response = new DataTableResponseDTO<ThemeListDTO>
            {
                Draw = draw,
                RecordsTotal = themes.FirstOrDefault()?.TotalCount ?? 0,
                RecordsFiltered = themes.FirstOrDefault()?.TotalCount ?? 0,
                Data = themes
            };
            return response;
        }
        public async Task<ServiceResponseDTO> SaveTheme(Theme theme, int currentUserId)
        {
            if (await _adminRepository.CheckDuplicateThemeName(theme.ThemeName, theme.Id))
            {
                return new ServiceResponseDTO(false, AppStatusCodes.BadRequest, true, MessageError.DuplicateThemeName);
            }
            int userId = currentUserId;
            if (theme.Id > 0)
            {
                theme.ModifyBy = userId;
            }
            else
            {
                theme.DateCreated = DateTime.UtcNow;
                theme.CreatedBy = userId;
            }
            bool isSaved = await _adminRepository.SaveTheme(theme);
            return new ServiceResponseDTO(isSaved, isSaved ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isSaved, isSaved ? MessageSuccess.Saved : MessageError.CodeIssue);
        }
        public async Task<ServiceResponseDTO> DeleteTheme(int id, int userId)
        {
            ServiceResponseDTO response;
            var isDeleted = await _adminRepository.DeleteTheme(id, userId);
            response = new(isDeleted, isDeleted ? AppStatusCodes.Success : AppStatusCodes.Unauthorized, isDeleted, isDeleted ? MessageSuccess.Deleted : MessageError.CodeIssue);
            return response;
        }
        public async Task<Theme> GetTheme(int id)
        {
            var theme = await _adminRepository.GetTheme(id);
            return theme;
        }
        #endregion
        public async Task<IEnumerable<GradeSectionDTO>> GetGradesAndSections()
        {
            return await _adminRepository.GetGradesAndSections();
        }
    }
}
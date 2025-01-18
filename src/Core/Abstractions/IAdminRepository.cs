using Core.DTOs;
using Core.DTOs.Users;
using Core.Entities;
using Core.Utilities;
using Kadam.Core.DTOs;

namespace Core.Abstractions
{
    public interface IAdminRepository
    {
        #region "Users"
        Task<IEnumerable<Users>> GetUsers();
        Task<IEnumerable<UserListDTO>> GetUserList(int pageNumber, int pageSize, string searchTerm);
        Task<bool> CheckUserExist(string email, int id);
        Task<Users> GetUser(int id);
        Task<bool> SaveUser(Users user);
        Task<bool> DeleteUser(int id);
        Task<UserLoginValidateDTO> ValidateUser(string email);
        Task<IEnumerable<UserProgramDTO>> GetUserPrograms(int userId);
        Task<bool> SaveUserPrograms(IEnumerable<UserProgram> userPrograms);
        Task<bool> SavePeopleInstitution(PeopleInstitution peopleInstitution);
        Task<PeopleInstitution> GetPeopleInstitution(int userId);
        #endregion

        #region "Division"
        Task<bool> SaveDivision(Division division);
        Task<Division> GetDivision(int id);
        Task<IEnumerable<DivisionListDTO>> GetDivisionList(int pageNumber, int pageSize, string searchTerm);
        Task<bool> DeleteDivision(int id, int deletedBy);
        Task<bool> CheckDuplicateDivisionName(string divisionName, int id);
        Task<bool> CheckDuplicateDivisionCode(string divisionCode, int id);
        Task<IEnumerable<DropdownDTO>> GetDivisionsByStatus(Enums.Status? currentStatus);
        #endregion

        #region States
        Task<bool> SaveState(State state);
        Task<State> GetState(int id);
        Task<IEnumerable<StateListDTO>> GetStateList(int pageNumber, int pageSize, string searchTerm);
        Task<bool> DeleteState(int id, int deletedBy);
        Task<bool> CheckDuplicateStateName(string stateName, int id);
        Task<bool> CheckDuplicateStateCode(string stateCode, int id);
        Task<IEnumerable<DropdownDTO>> GetStatesByStatus(Enums.Status currentStatus);
        Task<bool> CloseState(int id, int closedBy);
        #endregion

        #region "District"
        Task<bool> SaveDistrict(District district);
        Task<District> GetDistrict(int id);
        Task<IEnumerable<DistrictListDTO>> GetDistrictList(int pageNumber, int pageSize, string searchTerm);
        Task<bool> DeleteDistrict(int id, int deletedBy);
        Task<bool> CheckDuplicateDistrictName(string districtName, int id, int stateId);
        Task<bool> CheckDuplicateDistrictCode(string districtCode, int id, int stateId);
        Task<IEnumerable<DropdownDTO>> GetDistrictsByState(int stateId);
        #endregion

        #region "Blocks"
        Task<bool> SaveBlock(Block block);
        Task<Block> GetBlock(int id);
        Task<IEnumerable<BlockListDTO>> GetBlockList(int pageNumber, int pageSize, string searchTerm);
        Task<bool> DeleteBlock(int id, int deletedBy);
        Task<bool> CheckDuplicateBlockName(string blockName, int id, int districtId);
        Task<IEnumerable<DropdownDTO>> GetBlocksByDistrict(int districtId);
        #endregion

        #region "Villages"
        Task<IEnumerable<VillageListDTO>> GetVillageList(int pageNumber, int pageSize, string searchTerm);
        Task<bool> SaveVillage(Village village);
        Task<Village> GetVillage(int id);
        Task<bool> DeleteVillage(int id, int deletedBy);
        Task<bool> CheckDuplicateVillageName(string villageName, int id, int blockId);
        Task<IEnumerable<DropdownDTO>> GetVillagesByBlock(int blockId);
        #endregion

        #region "Programs"
        Task<IEnumerable<ProgramListDTO>> GetProgramList(int pageNumber, int pageSize, string searchTerm);
        Task<bool> SaveProgram(Program program);
        Task<Program> GetProgram(int id);
        Task<bool> DeleteProgram(int id, int deletedBy);
        Task<bool> CheckDuplicateProgramName(string programName, int id);
        #endregion

        #region "Roles"
        Task<IEnumerable<RoleListDTO>> GetRoleList(int pageNumber, int pageSize, string searchTerm);
        Task<bool> SaveRole(Role role);
        Task<Role> GetRole(int id);
        Task<bool> DeleteRole(int id, int deletedBy);
        Task<bool> CheckDuplicateRoleName(string roleName, int id);
        Task<IEnumerable<RolePermission>> GetRolePermissions(int roleId);
        Task<bool> SaveRolePermissions(RolePermissionsDTO rolePermissions, int createdBy);
        Task<IEnumerable<DropdownDTO>> GetRolesDropDown();
        #endregion

        #region "Menu Permission"
        Task<IEnumerable<MenuPermissionListDTO>> GetMenuPermissionList(int pageNumber, int pageSize, string searchTerm);
        Task<bool> SaveMenuPermission(MenuPermission menuPermission);
        Task<MenuPermission> GetMenuPermission(int id);
        Task<bool> DeleteMenuPermission(int id, int deletedBy);
        Task<IEnumerable<DropdownDTO>> GetMenus();
        Task<bool> CheckDuplicateMenuName(string menuName, int id);
        Task<IEnumerable<MenuPermissionDTO>> GetMenuPermissions();
        #endregion

        #region "Subjects"
        Task<IEnumerable<SubjectListDTO>> GetSubjectList(int pageNumber, int pageSize, string searchTerm);
        Task<bool> SaveSubject(Subject subject);
        Task<Subject> GetSubject(int id);
        Task<bool> DeleteSubject(int id, int deletedBy);
        Task<bool> CheckDuplicateSubjectName(string subjectName, int id);
        #endregion

        #region "Steps"
        Task<IEnumerable<StepListDTO>> GetStepList(int pageNumber, int pageSize, string searchTerm);
        Task<bool> SaveStep(Step step);
        Task<Step> GetStep(int id);
        Task<bool> DeleteStep(int id, int deletedBy);
        Task<bool> CheckDuplicateStepName(string stepName, int id);
        #endregion

        #region "Themes"
        Task<IEnumerable<ThemeListDTO>> GetThemeList(int pageNumber, int pageSize, string searchTerm);
        Task<bool> SaveTheme(Theme theme);
        Task<Theme> GetTheme(int id);
        Task<bool> DeleteTheme(int id, int deletedBy);
        Task<bool> CheckDuplicateThemeName(string themeName, int id);
        #endregion

        #region  "Common"
        Task<IEnumerable<GradeSectionDTO>> GetGradesAndSections();
        #endregion
    }
}
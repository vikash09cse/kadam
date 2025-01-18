using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core.Features.Admin;
using Core.DTOs;
using Core.Utilities;
using Core.Entities;
using static Core.Utilities.Enums;
using Core.DTOs.Users;

namespace WebUI.Pages.Admin
{
    public class InstitutionsModel(InstitutionService _institutionService, AdminService adminService, AuthenticationService authenticationService) : PageModel
    {
        

        public void OnGet()
        {
            // Render the Institutions.cshtml page
        }

        /// <summary>
        /// Handler to fetch the list of institutions for the DataTable.
        /// </summary>
        public async Task<IActionResult> OnGetGetInstitutionListAsync(int draw, int start, int length, Enums.Status? currentStatus, string searchValue)
        {
            try
            {
                // Calculate page number based on start and length
                int pageNumber = (start / length) + 1;

                // Assume current user ID is retrieved from claims
                int currentUserId = authenticationService.GetCurrentUserId();

                var institutions = await _institutionService.GetInstitutions(pageNumber, length, currentStatus, searchValue);

                // Assuming each InstitutionListDTO has a TotalCount property
                int totalCount = institutions.FirstOrDefault()?.TotalCount ?? 0;

                var response = new DataTableResponseDTO<InstitutionListDTO>
                {
                    Draw = draw,
                    RecordsTotal = totalCount,
                    RecordsFiltered = totalCount,
                    Data = institutions
                };

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "An error occurred while fetching the institution list." });
            }
        }

        /// <summary>
        /// Handler to fetch a single institution by ID.
        /// </summary>
        public async Task<IActionResult> OnGetGetInstitutionByIdAsync(int id)
        {
            try
            {
                var institution = await _institutionService.GetInstitutionById(id);
                if (institution == null)
                {
                    return new JsonResult(new { success = false, message = "Institution not found." });
                }

                return new JsonResult(institution);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "An error occurred while fetching the institution details." });
            }
        }

        /// <summary>
        /// Handler to save or update an institution.
        /// </summary>
        public async Task<IActionResult> OnPostSaveInstitutionAsync([FromBody] InstitutionSave institution)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Invalid data." });
            }

            try
            {
                // Retrieve current user ID
                int currentUserId = authenticationService.GetCurrentUserId();

                var response = await _institutionService.SaveInstitution(institution, currentUserId);

                return new JsonResult(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "An error occurred while saving the institution." });
            }
        }

        /// <summary>
        /// Handler to delete an institution by ID.
        /// </summary>
        public async Task<IActionResult> OnPostDeleteInstitutionAsync(int id)
        {
            try
            {
                // Retrieve current user ID
                int currentUserId = authenticationService.GetCurrentUserId();

                var response = await _institutionService.DeleteInstitution(id, currentUserId);

                return new JsonResult(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "An error occurred while deleting the institution." });
            }
        }
        public async Task<IActionResult> OnGetInitialData()
        {
            var response = new { 
                InstitutionTypes = EnumHelper<InstitutionType>.GetEnumDropdownList() ,
                InstitutionBuildings = EnumHelper<InstitutionBuilding>.GetEnumDropdownList(),
                Divisions = await adminService.GetDivisionsByStatus(Enums.Status.Active),
                States = await adminService.GetStatesByStatus(Enums.Status.Active),
                Grades = await adminService.GetGradesAndSections(),
            };

            return new JsonResult(response);
        }

        //public async Task<IActionResult> OnGetDivisionList()
        //{
        //    var divisions= await adminService.GetDivisionsByStatus(Enums.Status.Active);
        //    return new JsonResult(divisions);
        //}

        //public async Task<IActionResult> OnGetStateList()
        //{
        //    var states = await adminService.GetStatesByStatus(Enums.Status.Active); // Assuming this method returns a list of states
        //    return new JsonResult(states);
        //}
        public async Task<IActionResult> OnGetDistrictListByState(int stateId)
        {
            var districts = await adminService.GetDistrictsByState(stateId);
            return new JsonResult(districts);
        }
        public async Task<IActionResult> OnGetBlockListByDistrict(int districtId)
        {
            var blocks = await adminService.GetBlocksByDistrict(districtId);
            return new JsonResult(blocks);
        }

        public async Task<IActionResult> OnGetVillagesByBlock(int blockId)
        {
            var villages = await adminService.GetVillagesByBlock(blockId);
            return new JsonResult(villages);
        }
    }
}
    
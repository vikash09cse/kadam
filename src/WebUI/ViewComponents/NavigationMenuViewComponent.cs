using Core.DTOs;
using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc;
using Core.Utilities;

namespace WebUI.ViewComponents
{
    public class NavigationMenuViewComponent(AdminService adminService, AuthenticationService authenticationService) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = authenticationService.GetCurrentUser();
            if (user == null || user.Id <= 0)
            {
                return Content(string.Empty);
            }

            await adminService.EnsureNavigationMenusSeeded();
            var flatMenus = (await adminService.GetUserNavigationMenus(user.Id)).ToList();
            flatMenus = FilterForPortal(flatMenus, user.PortalType);
            var menuTree = BuildMenuTree(flatMenus);
            return View(menuTree);
        }

        private static List<NavigationMenuDTO> FilterForPortal(
            List<NavigationMenuDTO> menus,
            Enums.PortalType portalType)
        {
            var studentMenuIds = menus
                .Where(IsStudentPortalMenu)
                .Select(x => x.Id)
                .ToHashSet();

            foreach (var child in menus.Where(x => x.ParentId.HasValue && studentMenuIds.Contains(x.ParentId.Value)))
                studentMenuIds.Add(child.Id);

            return portalType == Enums.PortalType.Student
                ? menus.Where(x => studentMenuIds.Contains(x.Id) && !IsHiddenStudentMenu(x)).ToList()
                : menus.Where(x => !studentMenuIds.Contains(x.Id)).ToList();
        }

        private static bool IsHiddenStudentMenu(NavigationMenuDTO menu)
        {
            return menu.MenuName.Equals("Student Health", StringComparison.OrdinalIgnoreCase)
                || menu.MenuName.Equals("Student Documents", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsStudentPortalMenu(NavigationMenuDTO menu)
        {
            var url = menu.MenuUrl ?? string.Empty;
            var key = menu.MenuKey ?? string.Empty;
            return url.StartsWith("/StudentPortal", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("/Students", StringComparison.OrdinalIgnoreCase)
                || key.Equals("studentportal", StringComparison.OrdinalIgnoreCase)
                || key.Equals("studentoperations", StringComparison.OrdinalIgnoreCase)
                || menu.MenuName.Equals("Student Portal", StringComparison.OrdinalIgnoreCase)
                || menu.MenuName.Equals("Student Operations", StringComparison.OrdinalIgnoreCase);
        }

        private static List<NavigationMenuDTO> BuildMenuTree(List<NavigationMenuDTO> flatMenus)
        {
            var menuLookup = flatMenus.ToDictionary(x => x.Id);
            var roots = new List<NavigationMenuDTO>();

            foreach (var menu in flatMenus.OrderBy(x => x.SortOrder).ThenBy(x => x.Id))
            {
                if (menu.ParentId is null or 0)
                {
                    roots.Add(menu);
                    continue;
                }

                if (menuLookup.TryGetValue(menu.ParentId.Value, out var parent))
                {
                    parent.Children.Add(menu);
                }
            }

            foreach (var root in roots)
            {
                root.Children = root.Children.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToList();
            }

            return roots.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToList();
        }
    }
}

using Core.DTOs;
using Core.Features.Admin;
using Microsoft.AspNetCore.Mvc;

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
            var menuTree = BuildMenuTree(flatMenus);
            return View(menuTree);
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

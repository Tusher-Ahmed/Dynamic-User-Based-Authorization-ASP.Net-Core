using DynamicRoleWeb.Models;
using DynamicRoleWeb.ViewModels;
using System;
using System.Collections.Generic;

namespace DynamicRoleWeb.Services
{
    public interface IDataAccessService
    {
        List<MenuGroupViewModel> LoadAreasFromDbAndRefelction();
        void AddMenuGroup(MenuGroupViewModel model);
        List<AreaControllerViewModel> GetControllerThrowDbAndReflection();
        List<ActionsViewModel> GetActionsThrowReflectionAndDb();
        List<ActionsViewModel> GetActionsFromDb();
        List<MenuViewModel> GetMenusFromDb();
        List<MenuViewModel> LoadMenus();
        void AddAreaControllerToDb(List<AreaControllerViewModel> model);
        void AddActionsToDb(List<ActionsViewModel> model);
        void AddMenuToDb(MenuViewModel model);
        AreaController GetAreaControllerName(int areaId);
        MenuGroup GetAreaUsingAreaControllerId(int menuGroupId);
        bool isAlreadyPermitted(int actionId, string userId);
        void AddUserMenuPermission(List<UserMenuViewModel> model);
        List<AllPermissionMenuViewModel> GetAllMenus();
        List<Actions> LoadActionNames(int areaControllerId);
        List<AreaController> LoadAreaController();
    }
}

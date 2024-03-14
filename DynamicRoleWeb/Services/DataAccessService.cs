using Dapper;
using DynamicRoleWeb.Data;
using DynamicRoleWeb.Models;
using DynamicRoleWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DynamicRoleWeb.Services
{
    [Area("Administrator")]
    [Authorize]
    public class DataAccessService : IDataAccessService
    {
        private readonly ILogger<DataAccessService> _logger;
        private readonly AppDbContext _context;
        public DataAccessService(AppDbContext context, ILogger<DataAccessService> logger)
        {
            _context = context;
            _logger = logger;

        }

        public void AddActionsToDb(List<ActionsViewModel> model)
        {
            foreach (var action in model)
            {
                if (!action.IsPresent && action.IsChecked)
                {
                    var item = new Actions
                    {
                        ActionName = action.ActionName,
                        AreaControllerId = action.AreaControllerId,
                    };
                    _context.Actions.Add(item);
                    _context.SaveChanges();
                }
            }
        }

        public void AddAreaControllerToDb(List<AreaControllerViewModel> model)
        {
            try
            {
                foreach (var controller in model)
                {
                    var isPresent = _context.AreaControllers.Where(x => x.ControllerName == controller.ControllerName).Any();
                    var menuGroup = _context.MenuGroups.Where(x => x.AreaName == controller.AreaName).FirstOrDefault();
                    if (!isPresent && controller.IsChecked && menuGroup != null)
                    {
                        var AreaId = menuGroup.Id;
                        var item = new AreaController
                        {
                            ControllerName = controller.ControllerName,
                            MenuGroupId = AreaId,
                        };
                        _context.Add(item);
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }

        }

        public void AddMenuGroup(MenuGroupViewModel model)
        {
            var area = new MenuGroup
            {
                AreaName = model.AreaName,
            };
            _context.MenuGroups.Add(area);
            _context.SaveChanges();
        }

        public void AddMenuToDb(MenuViewModel model)
        {
            var isPresent = _context.Menus.Where(x => x.DisplayName.ToLower() == model.DisplayName.ToLower() && x.ActionId == model.ActionId && x.ParentsId == model.ParentsId).Any();
            if (!isPresent)
            {
                // var ActionName = _context.Actions.Find(model.ActionId);
                var item = new Menu
                {
                    DisplayName = model.DisplayName,
                    ActionId = model.ActionId,
                    ParentsId = model.ParentsId
                };
                _context.Menus.Add(item);
                _context.SaveChanges();
            }
        }

        public List<MenuViewModel> LoadMenus()
        {
            var menus = _context.Menus.ToList();
            List<MenuViewModel> menuViewModels = new List<MenuViewModel>();

            foreach (var menu in menus)
            {
                string actionName = string.Empty;
                string parentName = string.Empty;

                if (menu.ActionId != 0)
                {
                    actionName = _context.Actions.Find(menu.ActionId).ActionName;
                }
                if (menu.ParentsId != 0)
                {
                    parentName = _context.Menus.Find(menu.ParentsId).DisplayName;
                }

                var item = new MenuViewModel
                {
                    DisplayName = menu.DisplayName,
                    ActionName = actionName,
                    ParentName = parentName
                };
                menuViewModels.Add(item);
            }
            return menuViewModels;
        }
        public List<ActionsViewModel> GetActionsFromDb()
        {
            var actions = _context.Actions.ToList();
            List<ActionsViewModel> list = new List<ActionsViewModel>();

            foreach (var action in actions)
            {
                var IsPresent = _context.Menus.Where(x=>x.ActionId == action.Id).Any();
                if (!IsPresent)
                {
                    var item = new ActionsViewModel
                    {
                        ActionName = action.ActionName,
                        Id = action.Id,
                        AreaControllerId = action.AreaControllerId,
                    };
                    list.Add(item);
                }
                
            }
            return list;
        }

        public List<MenuViewModel> GetMenusFromDb()
        {
            var list = new List<MenuViewModel>();
            var MenuList = _context.Menus.ToList();
            foreach (var menu in MenuList)
            {
                var item = new MenuViewModel
                {
                    DisplayName = menu.DisplayName,
                    Id = menu.Id,
                };
                list.Add(item);
            }
            return list;
        }
        public List<AllPermissionMenuViewModel> GetAllMenus()
        {
            var sql = @"
SELECT nm.Id,nm.ActionId,a.ActionName,ac.ControllerName,mg.AreaName
FROM Menus AS nm
LEFT JOIN Actions AS a ON a.Id = nm.ActionId
LEFT JOIN AreaControllers AS ac ON ac.Id = a.AreaControllerId
LEFT JOIN MenuGroups AS mg ON mg.Id = ac.MenuGroupId
";
            var result = LoadDataUsingRawSql(sql);
            return result;
        }
        private List<AllPermissionMenuViewModel> LoadDataUsingRawSql(string query)
        {
            using (IDbConnection db = new SqlConnection(_context.Database.GetConnectionString()))
            {
                return db.Query<AllPermissionMenuViewModel>(query).ToList();
            }
        }
        public AreaController GetAreaControllerName(int areaId)
        {
            var item = _context.AreaControllers.Where(x => x.Id == areaId).FirstOrDefault();

            return item;
        }

        public MenuGroup GetAreaUsingAreaControllerId(int menuGroupId)
        {
            return _context.MenuGroups.Where(x => x.Id == menuGroupId).FirstOrDefault();
        }
        public bool isAlreadyPermitted(int menuId, string userId)
        {
            return _context.UserMenuPermissions.Where(x => x.UserId == userId && x.MenuId == menuId).Any();
        }
        public List<ActionsViewModel> GetActionsThrowReflectionAndDb()
        {
            List<ActionsViewModel> actions = new List<ActionsViewModel>();
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract);
            if (controllers.Any())
            {
                foreach (var controller in controllers)
                {
                    var controllerName = controller.Name.Substring(0, controller.Name.Length - 10);
                    var controllerAttributes = controller.GetCustomAttributes(true);
                    var authorizeAttribute = controllerAttributes.FirstOrDefault(attr => attr.GetType() == typeof(AuthorizeAttribute)) as AuthorizeAttribute;
                    if (authorizeAttribute != null)
                    {
                        var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                        var AreaController = _context.AreaControllers.Where(x => x.ControllerName == controllerName).FirstOrDefault();
                        foreach (var method in methods)
                        {
                            var attributes = method.GetCustomAttributes(true);
                            var allowAnonymousAttribute = attributes.FirstOrDefault(attr => attr.GetType() == typeof(AllowAnonymousAttribute)) as AllowAnonymousAttribute;

                            if (allowAnonymousAttribute == null)
                            {

                                if (AreaController != null)
                                {
                                    var isPresent = _context.Actions.Where(item => item.AreaControllerId == AreaController.Id && item.ActionName == method.Name).Any();

                                    var returnType = method.ReturnType;

                                    if (typeof(IActionResult).IsAssignableFrom(returnType) || typeof(Task<IActionResult>).IsAssignableFrom(returnType))
                                    {
                                        var menuItem = new ActionsViewModel
                                        {
                                            ActionName = method.Name,
                                            AreaControllerName = controllerName,
                                            AreaControllerId = AreaController.Id,
                                            IsPresent = isPresent
                                        };

                                        actions.Add(menuItem);
                                    }

                                }
                            }


                        }
                    }
                }
            }
            return actions;
        }

        public List<AreaControllerViewModel> GetControllerThrowDbAndReflection()
        {
            var controllerList = GetControllerThrowReflection();
            return controllerList;
        }

        public List<Actions> LoadActionNames(int areaControllerId)
        {
            var sql = @"
SELECT
a.Id,
a.ActionName
FROM Actions as a
WHERE a.AreaControllerId = @AreaControllerId
";
            var result = LoadActionNamesUsingDapper(sql, new { AreaControllerId = areaControllerId });
            return result;
        }
        public List<Actions> LoadActionNamesUsingDapper( string query, object parameters)
        {
            using (IDbConnection db = new SqlConnection(_context.Database.GetConnectionString()))
            {
                return db.Query<Actions>(query, parameters).ToList();
            }
        }

        public List<AreaController> LoadAreaController()
        {
            return _context.AreaControllers.ToList();
        }
        public void AddUserMenuPermission(List<UserMenuViewModel> model)
        {
            foreach (var item in model)
            {
                
                var menu = _context.Menus.FirstOrDefault(x => x.ActionId == item.Actionid);
                if (menu != null)
                {                    
                   var IsPresent = _context.UserMenuPermissions.Where(x => x.UserId == item.UserId && x.MenuId == menu.Id).Any();

                    if (!IsPresent && item.IsChecked)
                    {

                        var menuPermission = new UserMenuPermission
                        {
                            UserId = item.UserId,
                            MenuId = menu.Id
                        };
                        _context.UserMenuPermissions.Add(menuPermission);
                        
                    }
                    if (IsPresent && !item.IsChecked)
                    {
                        var menuPermission = _context.UserMenuPermissions
                   .FirstOrDefault(x => x.UserId == item.UserId && x.MenuId == menu.Id);
                        _context.UserMenuPermissions.Remove(menuPermission);
                    }
                }
            }
            _context.SaveChanges();
        }

        public List<MenuGroupViewModel> LoadAreasFromDbAndRefelction()
        {
            var area = _context.MenuGroups.ToList();
            List<MenuGroupViewModel> menuGroupViewModels = new List<MenuGroupViewModel>();
            foreach (var item in area)
            {
                var group = new MenuGroupViewModel
                {
                    Id = item.Id,
                    AreaName = item.AreaName,
                };
                menuGroupViewModels.Add(group);
            }
            return menuGroupViewModels;
        }

        private List<AreaControllerViewModel> GetControllerThrowReflection()
        {
            List<AreaControllerViewModel> controllerList = new List<AreaControllerViewModel>();
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                           .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract);

            if (controllers.Any())
            {
                foreach (var controller in controllers)
                {
                    var controllerName = controller.Name.Substring(0, controller.Name.Length - 10);
                    var controllerAttributes = controller.GetCustomAttributes(true);
                    var authorizeAttribute = controllerAttributes.FirstOrDefault(attr => attr.GetType() == typeof(AuthorizeAttribute)) as AuthorizeAttribute;
                    if (authorizeAttribute != null)
                    {
                        string controllerNamespace = ParseAreaName(controller.Namespace);
                        var isPresent = _context.AreaControllers.Where(x => x.ControllerName == controllerName).Any();

                        var item = new AreaControllerViewModel
                        {
                            ControllerName = controllerName,
                            AreaName = controllerNamespace,
                            Permitted = isPresent
                        };
                        controllerList.Add(item);
                    }
                }
            }
            return controllerList;
        }

        string ParseAreaName(string namespaceString)
        {
            var parts = namespaceString.Split('.');
            int areasIndex = Array.IndexOf(parts, "Areas");

            if (areasIndex >= 0 && areasIndex < parts.Length - 1)
            {
                return parts[areasIndex + 1];
            }
            else
            {
                return "Unknown";
            }
        }
    }
}

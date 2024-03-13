using Dapper;
using DynamicRoleWeb.Data;
using DynamicRoleWeb.Models;
using DynamicRoleWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DynamicRoleWeb
{
    public class AuthorizeAccess : ActionFilterAttribute
    {
        public AuthorizeAccess() { }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var currentUser = filterContext.HttpContext.User;
            var context = filterContext.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var name = currentUser.Identity.Name;
            var actionDescriptor = filterContext.ActionDescriptor;
            var authorizeAttributeName = GetAuthorizeAttributeName(actionDescriptor);
            var controllerName = actionDescriptor.RouteValues["controller"];
            var actionName = actionDescriptor.RouteValues["action"];
            var controllerType = filterContext.Controller.GetType();
            var hasAuthorizeC = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
            string authorizeKeyword = hasAuthorizeC ? "Authorize" : "";

            if (currentUser != null && currentUser.Identity != null && currentUser.Identity.IsAuthenticated && name != null && name != "superadmin@gmail.com")
            {
                var menuList = GetMenusByUsername(filterContext, name);
                var isAllowedMenu = menuList.Where(x => x.ControllerName == controllerName && x.ActionName == actionName).Any();

                if (!isAllowedMenu && authorizeKeyword != "" && authorizeAttributeName == null)
                {
                    RedirectToPermissionDenied(filterContext);
                }

                var list = menuList.GroupBy(a => new { a.ControllerName, a.ActionName }).Where(x => x.Count() == 1).SelectMany(x => x).ToList();
                filterContext.HttpContext.Items["MenuList"] = list;
            }
            else if(currentUser != null && currentUser.Identity != null && currentUser.Identity.IsAuthenticated && name != null && name == "superadmin@gmail.com")
            {
                var menuList = GetAllMenus(filterContext);
                var list = menuList.GroupBy(a => new { a.ControllerName, a.ActionName }).Where(x => x.Count() == 1).SelectMany(x => x).ToList();
                filterContext.HttpContext.Items["MenuList"] = list;
            }
        }

        private List<MenuAccessViewModel> GetMenusByUsername(ActionExecutingContext filterContext, string name)
        {
            var sqlQuery = @"
            SELECT ump.Id, nm.DisplayName, a.ActionName, ac.ControllerName, mg.AreaName
            FROM UserMenuPermissions AS ump
            LEFT JOIN Menus AS nm ON ump.MenuId = nm.Id
            LEFT JOIN AspNetUsers AS u ON u.UserName = @UserName
            LEFT JOIN Actions AS a ON a.Id = nm.ActionId
            LEFT JOIN AreaControllers AS ac ON ac.Id = a.AreaControllerId
            LEFT JOIN MenuGroups AS mg ON mg.Id = ac.MenuGroupId
            WHERE ump.UserId = u.Id";

            var result = LoadRawSqlQueryResult(filterContext, sqlQuery, new { UserName = name });
            return result;
        }

        private List<MenuAccessViewModel> GetAllMenus(ActionExecutingContext filterContext)
        {
            var sqlQuery = @"
SELECT m.Id, m.DisplayName, a.ActionName, ac.ControllerName, mg.AreaName
FROM Menus AS m
LEFT JOIN Actions AS a ON a.Id = m.ActionId
LEFT JOIN AreaControllers AS ac ON ac.Id = a.AreaControllerId
LEFT JOIN MenuGroups AS mg ON mg.Id = ac.MenuGroupId";

            var result = LoadSuperRawSqlQueryResult(filterContext, sqlQuery);
            return result;
        }
        public List<MenuAccessViewModel> LoadRawSqlQueryResult(ActionExecutingContext filterContext, string query, object parameters)
        {
            var context = filterContext.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            using (IDbConnection db = new SqlConnection(context.Database.GetConnectionString()))
            {
                return db.Query<MenuAccessViewModel>(query, parameters).ToList();
            }
        }

        public List<MenuAccessViewModel> LoadSuperRawSqlQueryResult(ActionExecutingContext filterContext, string query)
        {
            var context = filterContext.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            using (IDbConnection db = new SqlConnection(context.Database.GetConnectionString()))
            {
                return db.Query<MenuAccessViewModel>(query).ToList();
            }
        }

        private string GetAuthorizeAttributeName(ActionDescriptor actionDescriptor)
        {
            foreach (var metadata in actionDescriptor.EndpointMetadata)
            {
                if (metadata is IAllowAnonymous)
                {
                    return "AllowAnonymous";
                }
            }
            return null;
        }

        private List<Menu> GetAllNavigationForSuperAdmin(ActionExecutingContext filterContext)
        {
            var context = filterContext.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var menuList = context.Menus.ToList();
            return menuList;
        }
        private void RedirectToPermissionDenied(ActionExecutingContext filterContext)
        {
            var rvd = new RouteValueDictionary();
            rvd.Add("message", "Permission Denied. User appropiate login");
            filterContext.Result = new RedirectToRouteResult("PermissionDenied", rvd);
        }
        private void RedirectToLogin(ActionExecutingContext filterContext)
        {
            var rvd = new RouteValueDictionary();
            rvd.Add("message", "Permission Denied. User appropiate login");
            filterContext.Result = new RedirectToRouteResult("Login", rvd);
        }
    }
}

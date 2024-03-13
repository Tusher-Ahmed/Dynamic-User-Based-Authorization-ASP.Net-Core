using DynamicRoleWeb.Models;
using DynamicRoleWeb.Services;
using DynamicRoleWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System;

namespace DynamicRoleWeb.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    [Authorize]
    public class UserMenuController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IDataAccessService _dataAccessService;

        public UserMenuController(UserManager<IdentityUser> userManager, IDataAccessService dataAccessService)
        {
            _userManager = userManager;
            _dataAccessService = dataAccessService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAreas()
        {
            var areas = _dataAccessService.LoadAreasFromDbAndRefelction();
            return View(areas);
        }

        [HttpGet]
        public IActionResult CreateArea()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateArea(MenuGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                _dataAccessService.AddMenuGroup(model);
                return RedirectToAction("GetAreas");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult GetController()
        {
            var controller = _dataAccessService.GetControllerThrowDbAndReflection();

            return View(controller);
        }

        [HttpPost]
        public IActionResult AddController(List<AreaControllerViewModel> list)
        {
            if (list == null)
            {
                return RedirectToAction("GetController");
            }

            _dataAccessService.AddAreaControllerToDb(list);

            return RedirectToAction("GetController");
        }

        [HttpGet]
        public IActionResult GetActions()
        {
            var Actions = _dataAccessService.GetActionsThrowReflectionAndDb();
            return View(Actions);
        }

        [HttpPost]
        public IActionResult AddActions(List<ActionsViewModel> list)
        {
            if (list == null)
            {
                return RedirectToAction("GetActions");
            }

            _dataAccessService.AddActionsToDb(list);
            return RedirectToAction("GetActions");
        }

        [HttpGet]
        public IActionResult Getmenu()
        {
            var Actiondata = _dataAccessService.GetActionsFromDb();
            var parent = _dataAccessService.GetMenusFromDb();
            ViewData["Actions"] = new SelectList(Actiondata, "Id", "ActionName");
            ViewData["Parent"] = new SelectList(parent, "Id", "DisplayName");
            return View();
        }

        [HttpPost]
        public IActionResult AddMenu(MenuViewModel model)
        {
            if (model == null)
            {
                return RedirectToAction("Getmenu");
            }

            _dataAccessService.AddMenuToDb(model);
            return RedirectToAction("Getmenu");
        }

        [HttpGet]
        public IActionResult ShowMenus()
        {
            var data = _dataAccessService.LoadMenus();
            return View(data);
        }
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var userViewModel = new List<UserMenuViewModel>();

            var users = await _userManager.Users.ToListAsync();
            foreach (var item in users)
            {
                if (item.UserName != "superadmin@gmail.com")
                {
                    userViewModel.Add(new UserMenuViewModel()
                    {
                        UserId = item.Id,
                        Email = item.Email,
                        UserName = item.UserName,
                    });
                }

            }

            return View(userViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditUserMenuPermission(string id)
        {
            List<UserMenuViewModel> list = new List<UserMenuViewModel>();
            if (!string.IsNullOrWhiteSpace(id))
            {
                var user = await _userManager.FindByIdAsync(id);
                ViewBag.UserName = user?.UserName;

                //var allActions = _dataAccessService.GetActionsFromDb();
                var menus = _dataAccessService.GetAllMenus();
                # region Previous
                //foreach (var action in allActions)
                //{
                //    var getAreaController = _dataAccessService.GetAreaControllerName(action.AreaControllerId);
                //    var getArea = _dataAccessService.GetAreaUsingAreaControllerId(getAreaController.MenuGroupId);
                //    var isPresent = _dataAccessService.isAlreadyPermitted(action.Id, id);
                //    var item = new UserMenuViewModel
                //    {
                //        UserId = id,
                //        AreaName = getArea.AreaName,
                //        ControllerName = getAreaController.ControllerName,
                //        ActionName = action.ActionName,
                //        Actionid = action.Id,
                //        Permitted = isPresent
                //    };
                //    list.Add(item);
                //}
                #endregion

                foreach (var menu in menus)
                {
                    var isPresent = _dataAccessService.isAlreadyPermitted(menu.Id, id);
                    var item = new UserMenuViewModel
                    {
                        UserId = id,
                        AreaName = menu.AreaName,
                        ControllerName = menu.ControllerName,
                        ActionName = menu.ActionName,
                        Actionid = menu.ActionId,
                        Permitted = isPresent,
                        IsChecked = isPresent,
                    };
                    list.Add(item);
                }
            }

            return View(list);
        }

        [HttpPost]
        public IActionResult EditUserMenuPermission(List<UserMenuViewModel> list)
        {
            if(list == null || list.Count == 0)
            {
                return RedirectToAction("Users");
            }

            _dataAccessService.AddUserMenuPermission(list);

            return RedirectToAction("Users");
        }
    }
}

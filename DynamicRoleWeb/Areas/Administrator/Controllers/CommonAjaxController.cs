using DynamicRoleWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DynamicRoleWeb.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    [Authorize]
    public class CommonAjaxController : Controller
    {
        private readonly IDataAccessService _dataAccessService;
        public CommonAjaxController(IDataAccessService dataAccessService)
        {
            _dataAccessService = dataAccessService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult LoadActionName(int areaControllerId)
        {
            var ActionList = _dataAccessService.LoadActionNames(areaControllerId);
            var actionNameSelectList = new SelectList(ActionList.ToList(), "Id", "ActionName");
            return Json(new {returnActionNameList = actionNameSelectList, IsSuccess = true});
        }
    }
}

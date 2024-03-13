using DynamicRoleWeb.Models;

namespace DynamicRoleWeb.ViewModels
{
    public class AreaControllerViewModel
    {
        public int Id { get; set; }
        public string ControllerName { get; set; }
        public int MenuGroupId { get; set; }
        public string AreaName { get; set; }
        public bool Permitted {  get; set; }
        public bool IsChecked {  get; set; }
    }
}

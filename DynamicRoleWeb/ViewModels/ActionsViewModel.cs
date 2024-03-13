using DynamicRoleWeb.Models;

namespace DynamicRoleWeb.ViewModels
{
    public class ActionsViewModel
    {
        public int Id { get; set; }
        public string ActionName { get; set; }
        public int AreaControllerId { get; set; }
        public string AreaControllerName { get; set; }
        public bool IsChecked {  get; set; }
        public bool IsPresent { get; set; }
    }
}

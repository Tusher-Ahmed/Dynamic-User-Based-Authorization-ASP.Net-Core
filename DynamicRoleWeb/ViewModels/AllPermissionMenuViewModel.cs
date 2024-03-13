namespace DynamicRoleWeb.ViewModels
{
    public class AllPermissionMenuViewModel
    {
        public int Id {  get; set; }
        public int ActionId {  get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public string AreaName { get; set; }
    }
}

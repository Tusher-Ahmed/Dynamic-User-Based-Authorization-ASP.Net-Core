namespace DynamicRoleWeb.ViewModels
{
    public class UserMenuViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string AreaName { get; set; }
        public string ControllerName { get; set; }
        public int Actionid {  get; set; }
        public string ActionName {  get; set; }
        public bool Permitted { get; set; }
        public bool IsChecked { get; set; }
    }
}


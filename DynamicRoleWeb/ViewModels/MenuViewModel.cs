namespace DynamicRoleWeb.ViewModels
{
    public class MenuViewModel
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string ActionName { get; set; }
        public string ParentName { get; set; }
        public int ParentsId {  get; set; }
        public int ActionId { get; set;}
    }
}

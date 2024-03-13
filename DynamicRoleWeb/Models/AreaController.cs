namespace DynamicRoleWeb.Models
{
    public class AreaController
    {
        public int Id { get; set; }
        public string ControllerName { get; set; }
        public int MenuGroupId { get; set; }
        public MenuGroup MenuGroup { get; set; }
    }
}

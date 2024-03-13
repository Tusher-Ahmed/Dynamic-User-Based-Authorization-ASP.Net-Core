namespace DynamicRoleWeb.Models
{
    public class Actions
    {
        public int Id { get; set; }
        public string ActionName { get; set; }
        public int AreaControllerId { get; set; }
        public AreaController AreaController { get; set; }
    }
}

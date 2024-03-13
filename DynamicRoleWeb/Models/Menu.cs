using System.ComponentModel.DataAnnotations.Schema;

namespace DynamicRoleWeb.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public int ParentsId { get; set; }
        public int ActionId { get; set; }
        public Actions Action { get; set; }
        [NotMapped]
        public string ParentName { get; set; }
        [NotMapped]
        public string ActionName { get; set; }
    }
}

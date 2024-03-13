using System.ComponentModel.DataAnnotations.Schema;

namespace DynamicRoleWeb.Models
{
    public class UserMenuPermission
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int MenuId {  get; set; }
        [NotMapped]
        public bool Permitted { get; set; }
    }
}

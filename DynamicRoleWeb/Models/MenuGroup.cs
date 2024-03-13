using System.ComponentModel.DataAnnotations.Schema;

namespace DynamicRoleWeb.Models
{
    public class MenuGroup
    {
        public int Id { get; set; }
        public string AreaName { get; set; }
        [NotMapped]
        public bool IsPresent {  get; set; }
    }
}

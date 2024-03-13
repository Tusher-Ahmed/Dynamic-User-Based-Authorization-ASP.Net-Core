using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DynamicRoleWeb.ViewModels
{
    public class MenuGroupViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Area Name")]
        public string AreaName { get; set; }
        public bool IsPresent { get; set; }
    }        
}


namespace DynamicRoleWeb.ViewModels
{
    public class MenuAccessViewModel
    {
        public string DisplayName { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public string AreaName { get; set; }

        internal static object FromSqlRaw(string sqlQuery)
        {
            throw new NotImplementedException();
        }
    }
}

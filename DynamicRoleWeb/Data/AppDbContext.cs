using DynamicRoleWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DynamicRoleWeb.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options) 
        {
            
        }

        public DbSet<AreaController> AreaControllers { get; set; }
        public DbSet<Actions> Actions {  get; set; }
        public DbSet<Menu> Menus {  get; set; }
        public DbSet<MenuGroup> MenuGroups {  get; set; }
        public DbSet<UserMenuPermission> UserMenuPermissions {  get; set; }

    }
}

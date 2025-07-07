using Microsoft.EntityFrameworkCore;
using OmegleCloneMVC.Models;

namespace OmegleCloneMVC.Data
{
    public class OmegleCloneMVCContext : DbContext
    {
        public OmegleCloneMVCContext(DbContextOptions<OmegleCloneMVCContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var roles = Enum.GetValues(typeof(Roles))
                .Cast<Roles>()
                .Select(e => new Role
                {
                    Id = (int)e,
                    Name = e.ToString()
                });

            modelBuilder.Entity<Role>().HasData(roles);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);
        }

        public DbSet<User> User { get; set; }
        public DbSet<Role> Role { get; set; }
    }
}

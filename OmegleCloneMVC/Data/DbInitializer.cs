using Microsoft.AspNetCore.Identity;
using OmegleCloneMVC.Data;
using OmegleCloneMVC.Models;
using OmegleCloneMVC.Models;

namespace OmegleCloneMVC.Data
{
    public static class DbInitializer
    {
        public static void Initialize(OmegleCloneMVCContext context)
        {
            Console.WriteLine(">>> DbInitializer started <<<");

            if (context.User.Any())
            {
                Console.WriteLine(">>> Users already exist. Skipping seeding.");
                return;
            }

            // Kreiraj role ako ne postoje
            var roles = new List<Role>
            {
                new Role { Name = "Admin" },
                new Role { Name = "Moderator" },
                new Role { Name = "User" }
            };
            context.Role.AddRange(roles);
            context.SaveChanges();

            // Kreiraj korisnike
            var hasher = new PasswordHasher<User>();
            var users = new List<User>();

            for (int i = 1; i <= 20; i++)
            {
                var user = new User
                {
                    Username = $"user{i}",
                    Mail = $"user{i}@mail.com",
                    RoleId = 3,          // User role
                    IsActive = 1
                };
                user.Password = hasher.HashPassword(user, "pass");
                users.Add(user);
            }

            // Admin user
            var admin = new User
            {
                Username = "admin",
                Mail = "admin@mail.com",
                RoleId = 1,             // Admin role
                IsActive = 1
            };
            admin.Password = hasher.HashPassword(admin, "admin");
            users.Add(admin);

            context.User.AddRange(users);
            context.SaveChanges();

            Console.WriteLine(">>> DbInitializer finished <<<");
        }
    }
}

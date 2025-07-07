using System.ComponentModel.DataAnnotations;

namespace OmegleCloneMVC.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; } = 0;
        public string Username { get; set; }
        public string Mail { get; set; }
        public string Password { get; set; }
        public int IsActive { get; set; } = 0;

        public int RoleId { get; set; }
        public Role Role { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        // Obrisano nepotrebno Tasks polje
    }
}

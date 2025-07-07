using System.ComponentModel.DataAnnotations;

namespace OmegleCloneMVC.Models
{
    public class UserDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Mail { get; set; }

        [Required]
        public string Password { get; set; }
    }
}

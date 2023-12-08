using System.ComponentModel.DataAnnotations;

namespace ServerBlogManagement.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }
    }
}

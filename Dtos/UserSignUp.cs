using System.ComponentModel.DataAnnotations;

namespace WebAPI.Dtos
{
    public class UserSignUp
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Mobile { get; set; }
    }

}

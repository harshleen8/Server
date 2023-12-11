namespace WebAPI.Dtos
{
    public class ResetPasswordRequest
    {
        public string UserName { get; set; }
        public string NewPassword { get; set; }
    }
}

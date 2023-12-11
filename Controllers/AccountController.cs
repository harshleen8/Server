using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerBlogManagement.Data;
using ServerBlogManagement.Models;
using System.IdentityModel.Tokens.Jwt;
using WebAPI;
using WebAPI.Dtos;

namespace ServerBlogManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<BlogManagement> _userManager;
        private readonly JwtHandler _jwtHandler;
        private readonly AppDbContext _context;

        public AccountController(UserManager<BlogManagement> userManager, JwtHandler jwtHandler, AppDbContext context)
        {
            _userManager = userManager;
            _jwtHandler = jwtHandler;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            BlogManagement? user = await _userManager.FindByNameAsync(loginRequest.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
            {
                return Unauthorized(new LoginResult
                {
                    Success = false,
                    Message = "Invalid Username or Password."
                });
            }
            var newUser = new User
            {
                Username = loginRequest.UserName,
                PasswordHash = loginRequest.Password
            };

            // Save the new user to the Users table
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            JwtSecurityToken secToken = await _jwtHandler.GetTokenAsync(user);
            string? jwt = new JwtSecurityTokenHandler().WriteToken(secToken);
            return Ok(new LoginResult
            {
                Success = true,
                Message = "Login successful",
                Token = jwt
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserSignUp userSignUp)
        {
            // Check if the user already exists in UserSignUps table
            var existingUser = await _context.UserSignUps.FirstOrDefaultAsync(u => u.UserName == userSignUp.UserName);
            if (existingUser != null)
            {
                return BadRequest(new { Message = "Username already exists." });
            }

            // Create a new user object
            var newUser = new UserSignUp
            {
                UserName = userSignUp.UserName,
                Email = userSignUp.Email,
                Password = userSignUp.Password,
                Mobile = userSignUp.Mobile
            };

            // Save the user registration details to the UserSignUps table
            _context.UserSignUps.Add(newUser);
            await _context.SaveChangesAsync();

            // User created successfully
            return Ok(new { Message = "User created successfully." });
        }







        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
        {
            var user = await _userManager.FindByNameAsync(changePasswordRequest.UserName);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Validate the current password
            var isValidPassword = await _userManager.CheckPasswordAsync(user, changePasswordRequest.CurrentPassword);
            if (!isValidPassword)
            {
                return BadRequest(new { Message = "Invalid current password." });
            }

            // Change the user's password
            var result = await _userManager.ChangePasswordAsync(user, changePasswordRequest.CurrentPassword, changePasswordRequest.NewPassword);
            if (!result.Succeeded)
            {
                // Failed to change the password, return the error messages
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Message = "Failed to change password.", Errors = errors });
            }

            // Update the user's password hash in the Users table
            using (var dbContext = new AppDbContext())
            {
                var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == user.UserName);
                if (existingUser != null)
                {
                    existingUser.PasswordHash = user.PasswordHash;
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    // Invalid user Id format or user not found in the database
                    return BadRequest(new { Message = "Invalid user Id or user not found." });
                }
            }

            // Password changed successfully
            return Ok(new { Message = "Password changed successfully." });
        }





        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
        {
            var user = await _userManager.FindByNameAsync(resetPasswordRequest.UserName);
            if (user == null)
            {
                return BadRequest(new { Message = "User not found." });
            }

            // Generate a password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Reset the user's password
            var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordRequest.NewPassword);
            if (!result.Succeeded)
            {
                // Failed to reset the password, return the error messages
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Message = "Failed to reset password.", Errors = errors });
            }

            // Update the user's password hash in the database
            using (var dbContext = new AppDbContext())
            {
                var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == user.UserName);
                if (existingUser != null)
                {
                    existingUser.PasswordHash = user.PasswordHash;
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    // Invalid user Id format or user not found in the database
                    return BadRequest(new { Message = "Invalid user Id or user not found." });
                }
            }

            // Password reset successfully
            return Ok(new { Message = "Password reset successfully." });
        }




    }
}

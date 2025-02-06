using DIplomServer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DIplomServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HbtContext _context;

        public AuthController(HbtContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User newUser)
        {
            if (await _context.Users.AnyAsync(u => u.Email == newUser.Email))
            {
                return BadRequest("Email уже зарегистрирован.");
            }

            newUser.ProfilePicture = newUser.ProfilePicture ?? Array.Empty<byte>();

            newUser.Password = HashPassword(newUser.Password);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Регистрация успешна!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginUser.Email);
            if (user == null || !VerifyPassword(loginUser.Password, user.Password))
            {
                return Unauthorized("Неверный email или пароль.");
            }

            return Ok(new
            {
                message = "Авторизация успешна!",
                nickname = user.Nickname,
                level = user.Level,
                profilePicture = Convert.ToBase64String(user.ProfilePicture ?? Array.Empty<byte>())
            });
        }

        
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

       
        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            return HashPassword(inputPassword) == storedPassword;
        }
    }
}

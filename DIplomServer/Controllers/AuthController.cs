using ProfanityFilter;
using DIplomServer.Model;
using DIplomServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
namespace DiplomServer.Controllers
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
            var filter = new ProfanityFilter.ProfanityFilter();
            if (filter.IsProfanity(newUser.Nickname))
            {
                return BadRequest("Никнейм содержит недопустимые слова");
            }

            if (await _context.Users.AnyAsync(u => u.Email == newUser.Email))
            {
                return BadRequest("Email уже зарегистрирован.");
            }
            newUser.ProfilePicture = newUser.ProfilePicture ?? string.Empty;
            newUser.Password = HashPassword(newUser.Password);
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                UserId = newUser.Id,
                Message = "Регистрация успешна!",
                Nickname = newUser.Nickname,
                Level = newUser.Level,
                ProfilePicturePath = newUser.ProfilePicture 
            });
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
                UserId = user.Id,
                nickname = user.Nickname,
                level = user.Level,
                profilePicturePath = user.ProfilePicture,
                  experiencePoints = user.ExperiencePoints, 
                
            });
        }
        [HttpGet("checkUserExists")]
        public async Task<IActionResult> CheckUserExists(string email, string nickname)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email || u.Nickname == nickname);
            if (existingUser != null)
            {
                return Conflict("Пользователь с таким email или никнеймом уже существует.");
            }

            return Ok("Пользователь не найден, можно регистрироваться.");
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

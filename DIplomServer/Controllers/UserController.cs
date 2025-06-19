using DIplomServer.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static DIplomServer.Controllers.UserController;

namespace DIplomServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly HbtContext _context;

        public UserController(HbtContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            if (users == null || users.Count == 0)
            {
                return NotFound("No users found.");
            }

            return Ok(users);
        }

        [HttpGet("getProfile")]
        public IActionResult GetUserProfile(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            return Ok(new
            {
                nickname = user.Nickname,
                level = user.Level,
                profilePicturePath = user.ProfilePicture ?? string.Empty 
            });
        }

        [HttpGet("getUserInfo")]
        public IActionResult GetUserInfo(int? userId, string? nickname)
        {
            User? user = null;

            if (userId.HasValue)
            {
                user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            }
            else if (!string.IsNullOrEmpty(nickname))
            {
                user = _context.Users.FirstOrDefault(u => u.Nickname == nickname);
            }

            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            return Ok(new
            {
                userId = user.Id,
                nickname = user.Nickname,
                email = user.Email,
                level = user.Level,
                experiencePoints = user.ExperiencePoints,
                profilePicturePath = user.ProfilePicture ?? string.Empty,
                currentStreak = user.CurrentStreak,
                maxStreak = user.MaxStreak,
                borderstyle = user.BorderStyle


            });
        }

        [HttpGet("getUserByNickname")]
        public IActionResult GetUserByNickname(string nickname)
        {
            var user = _context.Users.FirstOrDefault(u => u.Nickname == nickname);
            if (user == null) return NotFound("Пользователь не найден");

            return Ok(new
            {
                user.Id,
                user.Nickname,
                user.Email,
                user.Level,
                user.ExperiencePoints,
                ProfilePicturePath = user.ProfilePicture ?? string.Empty 
            });
        }

        [HttpPost("uploadProfilePicture")]
        public async Task<IActionResult> UploadProfilePictureTest([FromBody] JsonElement request)
        {
            if (request.ValueKind == JsonValueKind.Null || request.ValueKind == JsonValueKind.Undefined)
            {
                return BadRequest("Некорректные данные.");
            }

            if (!request.TryGetProperty("userId", out var userIdElement) ||
                !request.TryGetProperty("imagePath", out var imagePathElement))
            {
                return BadRequest("Некорректные данные: отсутствуют userId или imagePath.");
            }

            int userId = userIdElement.GetInt32();
            string imagePath = imagePathElement.GetString();

            // Проверяем, что imagePath не пустой
            if (string.IsNullOrEmpty(imagePath))
            {
                return BadRequest("Некорректные данные: imagePath не может быть пустым.");
            }

            // Находим пользователя
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            // Обновляем путь к фотографии
            user.ProfilePicture = imagePath;
            await _context.SaveChangesAsync();

            return Ok("Путь сохранен");
        }

        [HttpGet("getProfilePicturePath")]
        public IActionResult GetProfilePicturePath(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            return Ok(new
            {
                profilePicturePath = user.ProfilePicture ?? string.Empty 
            });
        }
        [HttpPost("changeNickname")]
        public async Task<IActionResult> ChangeNickname([FromBody] ChangeNicknameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewNickname))
            {
                return BadRequest("Новый никнейм не может быть пустым");
            }

            var nicknameExists = await _context.Users
                .AnyAsync(u => u.Nickname == request.NewNickname);

            if (nicknameExists)
            {
                return BadRequest("Этот никнейм уже занят");
            }

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            user.Nickname = request.NewNickname;
            await _context.SaveChangesAsync();

            return Ok("Никнейм успешно изменен");
        }

        public class ChangeNicknameRequest
        {
            public int UserId { get; set; }
            public string NewNickname { get; set; }
        }
        public class ChangePasswordRequest
        {
            public int UserId { get; set; }
            public string OldPassword { get; set; } = null!;
            public string NewPassword { get; set; } = null!;
        }


        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
            if (user == null) return NotFound("Пользователь не найден.");

            // Используем ваш метод проверки пароля
            if (!VerifyPassword(model.OldPassword, user.Password))
                return BadRequest("Неверный текущий пароль");

            // Хэшируем новый пароль по тому же алгоритму
            user.Password = HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            return Ok("Пароль успешно изменён.");
        }

        // Скопируйте эти методы из AuthController
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
        [HttpDelete("deleteUser")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("Пользователь не найден");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok("Пользователь удалён");
    }
        [HttpGet("{userId}/level")]
        public async Task<IActionResult> GetUserLevel(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            return Ok(new { level = user.Level });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers(
    [FromQuery] string query,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Поисковый запрос не может быть пустым");
            }

            try
            {
                // Базовый запрос с фильтрацией по никнейму (регистронезависимый поиск)
                var usersQuery = _context.Users
                    .Where(u => EF.Functions.ILike(u.Nickname, $"%{query}%"))
                    .OrderBy(u => u.Nickname);

                // Получаем общее количество результатов для пагинации
                var totalCount = await usersQuery.CountAsync();

                // Применяем пагинацию
                var users = await usersQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        id = u.Id,
                        nickname = u.Nickname,
                        level = u.Level,
                        profilePicturePath = u.ProfilePicture ?? string.Empty,
                        borderstyle = u.BorderStyle // Добавляем стиль рамки

                    })
                    .ToListAsync();

                if (users.Count == 0)
                {
                    return NotFound("Пользователи не найдены");
                }

                return Ok(new
                {
                    totalCount,
                    currentPage = page,
                    pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    results = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при выполнении поиска: {ex.Message}");
            }
        }


        [HttpPut("{userId}/border")]
        public async Task<IActionResult> UpdateUserBorder(int userId, [FromBody] UpdateBorderDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.BorderStyle = dto.BorderStyle;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }

    public class UpdateBorderDto
    {
        public string BorderStyle { get; set; }
    }
}


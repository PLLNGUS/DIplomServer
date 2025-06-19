using DIplomServer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;


namespace DIplomServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AchievementsController : ControllerBase
    {
        private readonly HbtContext _context;
        private readonly ILogger<AchievementsController> _logger;

        public AchievementsController(HbtContext context, ILogger<AchievementsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserAchievement>>> GetUserAchievements(int userId)
        {
            return await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Include(ua => ua.Achievement)
                .OrderByDescending(ua => ua.DateReceived)
                .ToListAsync();
        }

        [HttpPost("check/{userId}")]
        public async Task<IActionResult> CheckAchievements(int userId)
        {
            try
            {
                var userLogs = await _context.HabitLogs
                    .Where(log => log.UserId == userId)
                    .OrderBy(log => log.Timestamp)
                    .ToListAsync();

                var allAchievements = await _context.Achievements.ToListAsync();
                var userAchievements = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId)
                    .ToListAsync();

                foreach (var achievement in allAchievements)
                {
                    int progress = 0;
                    bool isCompleted = false;

                    _logger.LogInformation($"Checking achievement: {achievement.Title}");

                    switch (achievement.Title.Trim())
                    {
                        case "Первый шаг в привычках 🌱":
                            progress = userLogs.Any(log => log.Action == "Added") ? 1 : 0;
                            break;

                        case "Первая неделя 📅":
                            progress = CheckConsecutiveDays(userLogs, 7) ? 7 : 0;
                            break;

                        case "Стальной характер 💪":
                            var hardHabitIds = await _context.Habits
                                .Where(h => h.UserId == userId && h.Difficulty == 3)
                                .Select(h => h.Id)
                                .ToListAsync();

                            progress = userLogs.Count(log =>
                                log.Action == "Completed" &&
                                log.HabitId.HasValue &&
                                hardHabitIds.Contains(log.HabitId.Value));
                            break;

                        case "30 дней успеха 🏆":
                            progress = CheckConsecutiveDays(userLogs, 30) ? 30 : 0;
                            break;

                        case "Мастер продуктивности ⚙️":
                            progress = userLogs
                                .Where(log => log.Action == "Completed" && log.HabitId.HasValue)
                                .Select(log => log.HabitId.Value)
                                .Distinct()
                                .Count();
                            break;

                        case "Ранний пташка 🌞":
                            progress = userLogs.Count(log =>
                                log.Action == "Completed" &&
                                log.Timestamp.TimeOfDay < new TimeSpan(9, 0, 0));
                            break;

                        case "Железная дисциплина 🏅":
                            progress = userLogs
                                .Where(log => log.Action == "Completed" && log.HabitId.HasValue)
                                .GroupBy(log => log.HabitId.Value)
                                .Select(g => g.Count())
                                .DefaultIfEmpty(0)
                                .Max();
                            break;

                        case "Не пропустил ни дня! 📆":
                            progress = CheckPerfectStreak(userLogs, 30) ? 30 : 0;
                            break;

                        case "Путь к совершенству ⛰️":
                            progress = userAchievements.Count(ua => ua.IsCompleted);
                            break;

                        case "Легенда Habitasky 👑":
                            progress = userAchievements.Count(ua => ua.IsCompleted);
                            break;
                    }

                    isCompleted = progress >= achievement.DefaultTarget;

                    var userAch = userAchievements.FirstOrDefault(ua => ua.AchievementId == achievement.Id);
                    if (userAch == null && (progress > 0 || isCompleted))
                    {
                        userAch = new UserAchievement
                        {
                            UserId = userId,
                            AchievementId = achievement.Id,
                            CurrentProgress = progress,
                            TargetProgress = achievement.DefaultTarget,
                            IsCompleted = isCompleted,
                            DateReceived = isCompleted ? DateTime.UtcNow : null
                        };
                        _context.UserAchievements.Add(userAch);
                    }
                    else if (userAch != null)
                    {
                        userAch.CurrentProgress = progress;
                        if (!userAch.IsCompleted && isCompleted)
                        {
                            userAch.IsCompleted = true;
                            userAch.DateReceived = DateTime.UtcNow;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                var result = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId)
                    .Include(ua => ua.Achievement)
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking achievements");
                return StatusCode(500, "Internal server error");
            }
        }

        private bool CheckConsecutiveDays(List<HabitLog> logs, int daysRequired)
        {
            var completedDates = logs
                .Where(log => log.Action == "Completed")
                .Select(log => log.Timestamp.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            if (completedDates.Count < daysRequired)
                return false;

            for (int i = 0; i <= completedDates.Count - daysRequired; i++)
            {
                var firstDate = completedDates[i];
                var lastDate = completedDates[i + daysRequired - 1];

                if ((lastDate - firstDate).TotalDays == daysRequired - 1)
                    return true;
            }

            return false;
        }

        private bool CheckPerfectStreak(List<HabitLog> logs, int daysRequired)
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-daysRequired + 1);

            var completedDates = logs
                .Where(log => log.Action == "Completed" &&
                             log.Timestamp.Date >= startDate &&
                             log.Timestamp.Date <= endDate)
                .Select(log => log.Timestamp.Date)
                .Distinct()
                .ToList();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (!completedDates.Contains(date))
                    return false;
            }

            return true;
        }

        [HttpPost("seed")]
        public async Task<IActionResult> SeedAchievements()
        {
            if (await _context.Achievements.AnyAsync())
                return Conflict("Achievements already exist");

            var achievements = new List<Achievement>
            {
                new Achievement { Title = "Первый шаг в привычках 🌱", Description = "Первый шаг сделан!", ImageIndex = "1", DefaultTarget = 1 },
                new Achievement { Title = "Первая неделя 📅", Description = "Неделя позади!", ImageIndex = "2", DefaultTarget = 7 },
                new Achievement { Title = "Стальной характер 💪", Description = "Ты закалил свою силу воли!", ImageIndex = "3", DefaultTarget = 10 },
                new Achievement { Title = "30 дней успеха 🏆", Description = "Настоящий чемпион!", ImageIndex = "4", DefaultTarget = 30 },
                new Achievement { Title = "Мастер продуктивности ⚙️", Description = "Твой список привычек впечатляет!", ImageIndex = "5", DefaultTarget = 5 },
                new Achievement { Title = "Ранний пташка 🌞", Description = "Ранние подъемы!", ImageIndex = "6", DefaultTarget = 20 },
                new Achievement { Title = "Железная дисциплина 🏅", Description = "100 раз - это не шутка!", ImageIndex = "7", DefaultTarget = 100 },
                new Achievement { Title = "Не пропустил ни дня! 📆", Description = "Привычки становятся образом жизни", ImageIndex = "8", DefaultTarget = 30 },
                new Achievement { Title = "Путь к совершенству ⛰️", Description = "Покорение новых высот", ImageIndex = "9", DefaultTarget = 5 },
                new Achievement { Title = "Легенда Habitasky 👑", Description = "Ты достиг вершины!", ImageIndex = "10", DefaultTarget = 9 }
            };

            await _context.Achievements.AddRangeAsync(achievements);
            await _context.SaveChangesAsync();

            return Ok($"Added {achievements.Count} achievements");
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Achievement>>> GetAllAchievements()
        {
            return await _context.Achievements.ToListAsync();
        }
    }
}
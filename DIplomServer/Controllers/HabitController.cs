using DIplomServer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace DIplomServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HabitController : ControllerBase
    {
        private readonly HbtContext _context;

        public HabitController(HbtContext context)
        {
            _context = context;
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddHabit([FromBody] Habit habit)
        {
            if (habit == null)
                return BadRequest("Habit data is null.");

            var existingUser = await _context.Users.FindAsync(habit.UserId);
            if (existingUser == null)
                return NotFound("User not found.");

            // Проверка лимита привычек
            var currentHabitsCount = await _context.Habits.CountAsync(h => h.UserId == habit.UserId);
            var maxHabitsAllowed = GetMaxHabitsAllowed(existingUser.Level);

            if (currentHabitsCount >= maxHabitsAllowed)
            {
                return BadRequest($"You have reached the maximum number of habits ({maxHabitsAllowed}) for your level ({existingUser.Level}). Level up to increase your limit!");
            }

            habit.User = existingUser;
            habit.StartDate = habit.StartDate.ToUniversalTime();
            if (habit.EndDate.HasValue)
                habit.EndDate = habit.EndDate.Value.ToUniversalTime();

            _context.Habits.Add(habit);
            await _context.SaveChangesAsync();

            var log = new HabitLog
            {
                UserId = habit.UserId,
                HabitId = habit.Id,
                Action = "Added",
                Timestamp = DateTime.UtcNow,
                Notes = $"Привычка \"{habit.Name}\" была добавлена."
            };
            _context.HabitLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Habit added successfully.",
                habitId = habit.Id,
                userId = habit.UserId
            });
        }

        private int GetMaxHabitsAllowed(int userLevel)
        {
            return userLevel switch
            {
                <= 5 => 5,
                <= 10 => 10,
                <= 15 => 15,
                _ => 20 // Максимальный лимит
            };
        }
        [HttpPost("completeHabit")]
        public async Task<IActionResult> CompleteHabit([FromBody] CompleteHabitRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request");
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return NotFound("User not found");
            var habit = await _context.Habits
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.Id == request.HabitId);
            if (habit == null)
                return NotFound("Habit not found");
            var today = DateTime.UtcNow.Date;
            var alreadyCompleted = await _context.HabitDiaries
                .AnyAsync(h => h.HabitId == habit.Id &&
                              h.UserId == user.Id &&
                              h.Date.Date == today &&
                              h.IsCompleted);
            if (alreadyCompleted)
            {
                return Ok(new
                {
                    Message = "Привычка уже выполнена сегодня",
                    CurrentStreak = user.CurrentStreak,
                    MaxStreak = user.MaxStreak
                });
            }
            var diaryEntry = new HabitDiary
            {
                UserId = user.Id,
                HabitId = habit.Id,
                Date = DateTime.UtcNow,
                IsCompleted = true,
                Notes = request.Notes
            };
            _context.HabitDiaries.Add(diaryEntry);
            var habitDiaries = await _context.HabitDiaries
                .Where(h => h.UserId == user.Id && h.IsCompleted)
                .ToListAsync();
            int newStreak = CalculateCurrentStreak(habitDiaries);
            bool streakIncreased = newStreak > user.CurrentStreak;
            bool streakBroken = newStreak == 0 && user.CurrentStreak > 0;
            if (newStreak > user.MaxStreak)
            {
                user.MaxStreak = newStreak;
            }
            else if (streakBroken && user.CurrentStreak > user.MaxStreak)
            {
                user.MaxStreak = user.CurrentStreak;
            }
            user.CurrentStreak = newStreak;
            int xpGain = habit.Difficulty switch
            {
                1 => 10,
                2 => 20,
                3 => 30,
                _ => 10
            };
            user.ExperiencePoints += xpGain;
            if (user.ExperiencePoints >= user.Level * 100)
            {
                user.Level++;
                user.ExperiencePoints = 0;
            }
            var updatedQuests = await UpdateQuestsProgress(user, habit);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                Message = streakIncreased ? "Серия увеличена!" :
                         streakBroken ? "Серия прервана" : "Привычка засчитана",
                Level = user.Level,
                Experience = user.ExperiencePoints,
                CurrentStreak = user.CurrentStreak,
                MaxStreak = user.MaxStreak,
                CompletedQuests = updatedQuests
                    .Where(q => q.IsCompleted)
                    .Select(q => new
                    {
                        q.Id,
                        q.Template.Title,
                        Reward = q.Template.RewardExperience
                    })
            });
        }
        public static int CalculateCurrentStreak(List<HabitDiary> userHabits)
        {
            var completedDates = userHabits
                .Where(h => h.IsCompleted)
                .Select(h => h.Date.Date) // нормализуем дату
                .Distinct()
                .OrderByDescending(date => date)
                .ToList();
            if (!completedDates.Any())
                return 0;
            var today = DateTime.UtcNow.Date; // 👈 сервер должен быть в UTC
            Console.WriteLine("DEBUG: today = " + today);
            Console.WriteLine("DEBUG: completedDates = " + string.Join(", ", completedDates));
            if (completedDates[0] != today)
                return 0;
            int streak = 1;
            for (int i = 1; i < completedDates.Count; i++)
            {
                if (completedDates[i] == today.AddDays(-streak))
                    streak++;
                else
                    break;
            }
            return streak;
        }
        private async Task<List<Quest>> UpdateQuestsProgress(User user, Habit habit)
        {
            var activeQuests = await _context.Quests
                .Include(q => q.Template)
                .Where(q => q.UserId == user.Id && !q.IsCompleted)
                .ToListAsync();
            foreach (var quest in activeQuests)
            {
                bool shouldUpdate = quest.Template.GoalType switch
                {
                    GoalType.COMPLETE_HABITS_TOTAL => true,
                    GoalType.COMPLETE_HARD_HABITS => habit.Difficulty >= 3,
                    GoalType.COMPLETE_MORNING_HABITS => DateTime.UtcNow.Hour is >= 5 and < 12,
                    GoalType.STREAK_DAYS => user.CurrentStreak >= quest.Template.GoalValue,
                    _ => false
                };
                if (shouldUpdate)
                {
                    quest.CurrentProgress++;
                    if (quest.CurrentProgress >= quest.Template.GoalValue)
                    {
                        quest.IsCompleted = true;
                        quest.User.ExperiencePoints += quest.Template.RewardExperience;
                    }
                }
            }
            return activeQuests;
        }

        [HttpGet("limits/{userId}")]
        public async Task<IActionResult> GetHabitLimits(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var currentCount = await _context.Habits.CountAsync(h => h.UserId == userId);
            var maxAllowed = GetMaxHabitsAllowed(user.Level);

            return Ok(new
            {
                CurrentCount = currentCount,
                MaxAllowed = maxAllowed,
                UserLevel = user.Level
            });
        }
        [HttpGet("user/{userId}/limit")]
        public async Task<IActionResult> GetHabitLimitInfo(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var currentCount = await _context.Habits.CountAsync(h => h.UserId == userId);
            var maxAllowed = GetMaxHabitsAllowed(user.Level);

            return Ok(new
            {
                currentHabitCount = currentCount,
                maxAllowedHabits = maxAllowed,
                userLevel = user.Level
            });
        }
        [HttpGet("today-count/{habitId}/{userId}")]
public async Task<ActionResult<int>> GetTodayCompletionsCount(int habitId, int userId)
{
    var today = DateTime.UtcNow.Date;
    var count = await _context.HabitDiaries
        .CountAsync(hd => hd.HabitId == habitId && 
                         hd.UserId == userId && 
                         hd.Date.Date == today && 
                         hd.IsCompleted);
        return Ok(count);
}
        private bool IsMorningHabit(Habit habit)
        {
            var now = DateTime.UtcNow;
            return now.Hour >= 5 && now.Hour < 12; 
        }
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetHabitsByUserId([FromQuery] int userId)
        {
            var habits = await _context.Habits
                .Where(h => h.UserId == userId)
                .ToListAsync();
            if (habits == null || !habits.Any())
            {
                return NotFound("No habits found for this user.");
            }
            return Ok(habits);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateHabit(int id, [FromBody] Habit habit)
        {
            if (habit == null || habit.Id != id)
            {
                return BadRequest("Invalid habit data.");
            }
            var existingHabit = await _context.Habits.FindAsync(id);
            if (existingHabit == null)
            {
                return NotFound("Habit not found.");
            }
            existingHabit.StartDate = habit.StartDate.ToUniversalTime();
            if (habit.EndDate.HasValue)
            {
                existingHabit.EndDate = habit.EndDate.Value.ToUniversalTime();
            }
            existingHabit.Name = habit.Name;
            existingHabit.Description = habit.Description;
            existingHabit.Difficulty = habit.Difficulty;
            existingHabit.RepeatInterval = habit.RepeatInterval;
            existingHabit.DaysOfWeek = habit.DaysOfWeek;
            existingHabit.CategoryKey = habit.CategoryKey;
            _context.Habits.Update(existingHabit);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Habit updated successfully." });
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteHabit(int id)
        {
            var habit = await _context.Habits.FindAsync(id);
            if (habit == null)
            {
                return NotFound("Habit not found.");
            }
            _context.Habits.Remove(habit);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Habit deleted successfully." });
        }
        [HttpPut("updateCategory")]
        public async Task<IActionResult> UpdateHabitCategory([FromBody] UpdateCategoryRequest request)
        {
            if (request == null)
                return BadRequest("Request data is null.");
            var habit = await _context.Habits.FindAsync(request.HabitId);
            if (habit == null)
                return NotFound("Habit not found.");
                    habit.CategoryKey = request.CategoryKey;
            _context.Habits.Update(habit);
            var log = new HabitLog
            {
                UserId = habit.UserId,
                HabitId = habit.Id,
                Action = "CategoryUpdated",
                Timestamp = DateTime.UtcNow,
                Notes = $"Категория привычки изменена на '{request.CategoryKey}'"
            };
            _context.HabitLogs.Add(log);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Category updated successfully",
                habitId = habit.Id,
                newCategory = request.CategoryKey
            });
        }
        public class UpdateCategoryRequest
        {
            public int HabitId { get; set; }
            public string CategoryKey { get; set; }
        }
        [HttpGet("getHabit/{id}")]
        public async Task<IActionResult> GetHabitById(int id)
        {
            var habit = await _context.Habits.FindAsync(id);
            if (habit == null)
            {
                return NotFound("Habit not found.");
            }

            return Ok(habit);
        }

        [HttpGet("monthlyStats/{userId}")]
        public async Task<IActionResult> GetMonthlyStats(int userId, int year, int month)
        {
            try
            {
                if (year < 2000 || year > 2100) return BadRequest("Invalid year");
                if (month < 1 || month > 12) return BadRequest("Invalid month");

                var firstDayOfMonth = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // Получаем все привычки пользователя и их даты создания
                var userHabits = await _context.Habits
                    .Where(h => h.UserId == userId)
                    .Select(h => new { h.Id, h.StartDate })
                    .ToListAsync();

                // Если нет привычек - возвращаем 0
                if (!userHabits.Any())
                    return Ok(new { completedDays = 0, missedDays = 0, successRate = 0 });

                // Получаем выполненные дни
                var completedDays = await _context.HabitDiaries
                    .Where(h => h.UserId == userId &&
                               h.Date >= firstDayOfMonth &&
                               h.Date <= lastDayOfMonth &&
                               h.IsCompleted)
                    .Select(h => h.Date.Date)
                    .Distinct()
                    .CountAsync();

                // Считаем активные дни (когда хотя бы одна привычка была активна)
                var activeDays = new HashSet<DateTime>();

                foreach (var habit in userHabits)
                {
                    var habitStart = habit.StartDate.Date;
                    var startDate = habitStart > firstDayOfMonth ? habitStart : firstDayOfMonth;

                    for (var date = startDate; date <= lastDayOfMonth; date = date.AddDays(1))
                    {
                        activeDays.Add(date);
                    }
                }

                var totalActiveDays = activeDays.Count;
                var missedDays = totalActiveDays - completedDays;

                return Ok(new
                {
                    completedDays,
                    missedDays,
                    successRate = totalActiveDays > 0 ? (completedDays * 100) / totalActiveDays : 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("dayDetails")]
        public IActionResult GetDayDetails(int userId, int year, int month, int day)
        {
            try
            {
                var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
                var activeHabits = _context.Habits
                    .Where(h => h.UserId == userId &&
                               h.StartDate <= date &&
                               (h.EndDate == null || h.EndDate >= date) &&
                               (h.DaysOfWeek == null || h.DaysOfWeek.Contains(date.DayOfWeek.ToString())))
                    .ToList();
                var completedHabits = _context.HabitDiaries
                    .Where(h => h.UserId == userId &&
                               h.Date.Date == date.Date &&
                               h.IsCompleted)
                    .Select(h => h.HabitId)
                    .ToList();
                var result = activeHabits.Select(habit => new
                {
                    habit.Id,
                    habit.Name,
                    IsCompleted = completedHabits.Contains(habit.Id),
                    DaysOfWeek = habit.DaysOfWeek?.Split(','),
                    IsScheduled = IsHabitScheduledForDate(habit, date)
                }).Where(x => x.IsScheduled);

                return Ok(new
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Habits = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }
        private bool IsHabitScheduledForDate(Habit habit, DateTime date)
        {
            if (!string.IsNullOrEmpty(habit.DaysOfWeek))
            {
                var dayOfWeek = date.DayOfWeek.ToString();
                return habit.DaysOfWeek.Split(',').Contains(dayOfWeek);
            }
            return true;
        }

        [HttpGet("statsByCategory/{userId}")]
        public async Task<IActionResult> GetStatsByCategory(int userId)
        {
            var stats = await _context.HabitDiaries
                .Where(hd => hd.UserId == userId && hd.IsCompleted)
                .Include(hd => hd.Habit)
                .GroupBy(hd => hd.Habit.CategoryKey ?? "other")
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            return Ok(stats);
        }
        [HttpGet("getAllHabits")]
        public async Task<IActionResult> GetAllHabits()
        {
            var habits = await _context.Habits.ToListAsync();
            if (habits == null || !habits.Any())
            {
                return NotFound("No habits found.");
            }
            return Ok(habits);
        }
    }
}

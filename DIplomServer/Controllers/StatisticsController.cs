using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;

using DIplomServer.Model;
using Microsoft.EntityFrameworkCore;

namespace DIplomServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly HbtContext _context;

        public StatisticsController(HbtContext context)
        {
            _context = context;
        }

        [HttpGet("habit-statistics/{userId}")]
        public async Task<IActionResult> GetHabitStatistics(int userId)
        {
            var now = DateTime.UtcNow;
            var weekAgo = now.AddDays(-7);

            var habits = await _context.HabitDiaries
                .Where(h => h.UserId == userId && h.Date >= weekAgo && h.Date <= now)
                .Select(h => new
                {
                    Date = h.Date,
                    IsCompleted = h.IsCompleted,
                    HabitId = h.HabitId,
                    HabitName = h.Habit.Name, // Название привычки
                    HabitDescription = h.Habit.Description // Описание привычки
                })
                .ToListAsync();

            if (habits == null || !habits.Any())
            {
                return NotFound("No habit statistics found for this user.");
            }

            return Ok(habits);
        }

        [HttpGet("habit-details")]
        public async Task<IActionResult> GetHabitDetails(DateTime date, int userId)
        {
            try
            {
                var habitDetails = await _context.HabitDiaries
                    .Where(h => h.UserId == userId && h.Date.Date == date.Date)
                    .GroupBy(h => h.Date.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        CompletedCount = g.Count(h => h.IsCompleted)
                    })
                    .FirstOrDefaultAsync();

                if (habitDetails == null)
                {
                    return NotFound("No habit details found for this date and user.");
                }

                return Ok(habitDetails);
            }
            catch (Exception ex)
            {
                // Логирование ошибки на сервере
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("habit-summary/{userId}")]
        public async Task<IActionResult> GetHabitSummary(int userId)
        {
            var totalHabits = await _context.Habits.CountAsync(h => h.UserId == userId);
            var completedHabits = await _context.HabitDiaries.CountAsync(h => h.UserId == userId && h.IsCompleted);

            var stats = new
            {
                TotalHabits = totalHabits,
                CompletedHabits = completedHabits,
                CompletionRate = totalHabits > 0 ? (double)completedHabits / totalHabits * 100 : 0
            };

            return Ok(stats);
        }
    }
}

using DIplomServer.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Win32;

using System.Threading.Tasks;

namespace DIplomServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HabitDiaryController : ControllerBase
    {
        private readonly HbtContext _context;

        public HabitDiaryController(HbtContext context)
        {
            _context = context;
        }

        // Получить все записи для конкретного пользователя
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<HabitDiary>>> GetUserHabitDiaries(int userId)
        {
            var habitDiaries = await _context.HabitDiaries
                .Where(hd => hd.UserId == userId)
                .Include(hd => hd.Habit) // Включаем связанные привычки
                .ToListAsync();

            if (habitDiaries == null || !habitDiaries.Any())
            {
                return NotFound("Записи привычек не найдены.");
            }

            return Ok(habitDiaries);
        }

        // Получить запись по ID
        [HttpGet("{id}")]
        public async Task<ActionResult<HabitDiary>> GetHabitDiary(int id)
        {
            var habitDiary = await _context.HabitDiaries
                .Include(hd => hd.Habit)
                .FirstOrDefaultAsync(hd => hd.Id == id);

            if (habitDiary == null)
            {
                return NotFound("Запись не найдена.");
            }

            return Ok(habitDiary);
        }

        // Добавить новую запись
        [HttpPost]
        public async Task<ActionResult<HabitDiary>> CreateHabitDiary([FromBody] HabitDiary habitDiary)
        {
            if (habitDiary == null)
            {
                return BadRequest("Данные записи не могут быть пустыми.");
            }

            _context.HabitDiaries.Add(habitDiary);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHabitDiary", new { id = habitDiary.Id }, habitDiary);
        }

        // Обновить запись
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHabitDiary(int id, [FromBody] HabitDiary habitDiary)
        {
            if (id != habitDiary.Id)
            {
                return BadRequest("Неверный ID записи.");
            }

            _context.Entry(habitDiary).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("check")]
        public async Task<ActionResult<bool>> CheckHabitCompletion(int habitId, int userId)
        {
            var habitCompleted = await _context.HabitDiaries
                .AnyAsync(hd => hd.HabitId == habitId && hd.UserId == userId && hd.IsCompleted);

            return Ok(habitCompleted);

        }
        [HttpGet("countToday")]
        public async Task<ActionResult<object>> GetTodayHabitCount([FromQuery] int habitId, [FromQuery] int userId)
        {
            var today = DateTime.UtcNow.Date;

            var count = await _context.HabitDiaries
                .Where(hd => hd.UserId == userId &&
                             hd.HabitId == habitId &&
                             hd.IsCompleted &&
                             hd.Date.Date == today)
                .CountAsync();

            return Ok(new { count });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHabitDiary(int id)
        {
            var habitDiary = await _context.HabitDiaries.FindAsync(id);
            if (habitDiary == null)
            {
                return NotFound("Запись не найдена.");
            }

            _context.HabitDiaries.Remove(habitDiary);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

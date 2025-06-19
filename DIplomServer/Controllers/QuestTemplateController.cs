using DIplomServer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;


namespace DIplomServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestTemplateController : ControllerBase
    {
        private readonly HbtContext _context;

        public QuestTemplateController(HbtContext context)
        {
            _context = context;
            SeedQuests(); // ← Автоматическая загрузка при первом обращении к контроллеру
        }

        private void SeedQuests()
        {
            if (_context.QuestTemplates.Any()) return;

            var quests = new List<QuestTemplate>
            {
                new QuestTemplate
                {
                    Title = "Железная дисциплина",
                    Description = "Заверши 10 сложных привычек",
                    GoalType = GoalType.COMPLETE_HARD_HABITS,
                    GoalValue = 10,
                    RewardExperience = 150
                },
                new QuestTemplate
                {
                    Title = "Утро начинается с победы",
                    Description = "Заверши 5 утренних привычек",
                    GoalType = GoalType.COMPLETE_MORNING_HABITS,
                    GoalValue = 5,
                    RewardExperience = 100
                },
                new QuestTemplate
                {
                    Title = "30-дневный рывок",
                    Description = "Подряд выполнить привычки 30 дней",
                    GoalType = GoalType.STREAK_DAYS,
                    GoalValue = 30,
                    RewardExperience = 300
                },
                new QuestTemplate
                {
                    Title = "Прогресс не остановить",
                    Description = "Выполни 50 привычек",
                    GoalType = GoalType.COMPLETE_HABITS_TOTAL,
                    GoalValue = 50,
                    RewardExperience = 200
                }
            };

            _context.QuestTemplates.AddRange(quests);
            _context.SaveChanges();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestTemplate>>> GetTemplates()
        {
            return await _context.QuestTemplates.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuestTemplate>> GetTemplate(int id)
        {
            var template = await _context.QuestTemplates.FindAsync(id);
            if (template == null) return NotFound();
            return template;
        }

        [HttpPost]
        public async Task<ActionResult<QuestTemplate>> CreateTemplate(QuestTemplate template)
        {
            _context.QuestTemplates.Add(template);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTemplate(int id, QuestTemplate updatedTemplate)
        {
            if (id != updatedTemplate.Id) return BadRequest();
            _context.Entry(updatedTemplate).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var template = await _context.QuestTemplates.FindAsync(id);
            if (template == null) return NotFound();

            _context.QuestTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

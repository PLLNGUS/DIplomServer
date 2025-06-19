using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

using DIplomServer.Model;
namespace DIplomServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestsController : ControllerBase
    {
        private readonly HbtContext _context;
        private static List<Quest> _currentWeeklyQuests = new();
        private static DateTime _lastUpdateTime = DateTime.MinValue;
        public QuestsController(HbtContext context)
        {
            _context = context;
        }

        [HttpGet("weekly")]
        public async Task<ActionResult<IEnumerable<Quest>>> GetWeeklyQuests(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Пользователь не найден");
            var currentQuests = await _context.Quests
                .Include(q => q.Template)
                .Where(q => q.UserId == userId)
                .ToListAsync();
            var now = DateTime.UtcNow;
            var lastQuestEndDate = currentQuests.OrderByDescending(q => q.EndDate).FirstOrDefault()?.EndDate;
            if (!currentQuests.Any() || lastQuestEndDate < now)
            {
                if (currentQuests.Any())
                {
                    _context.Quests.RemoveRange(currentQuests);
                    await _context.SaveChangesAsync();
                }
                var randomTemplates = await _context.QuestTemplates
                    .OrderBy(t => Guid.NewGuid())
                    .Take(3)
                    .ToListAsync();
                var newQuests = randomTemplates.Select(t => new Quest
                {
                    TemplateId = t.Id,
                    UserId = userId,
                    CurrentProgress = 0,
                    IsCompleted = false,
                    StartDate = now,
                    EndDate = now.AddDays(7),
                    Template = t
                }).ToList();
                await _context.Quests.AddRangeAsync(newQuests);
                await _context.SaveChangesAsync();
                currentQuests = newQuests;
            }
            return Ok(currentQuests);
        }
        private async Task UpdateWeeklyQuests()
        {
            var allUsers = await _context.Users.ToListAsync();
            var oldQuests = _context.Quests.Where(q => q.EndDate < DateTime.UtcNow);
            _context.Quests.RemoveRange(oldQuests);
            await _context.SaveChangesAsync();
            var newTemplates = await _context.QuestTemplates
                .OrderBy(t => Guid.NewGuid())
                .Take(3)
                .ToListAsync();
            var newQuests = new List<Quest>();
            foreach (var user in allUsers)
            {
                foreach (var template in newTemplates)
                {
                    newQuests.Add(new Quest
                    {
                        UserId = user.Id,
                        TemplateId = template.Id,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(7),
                        CurrentProgress = 0,
                        IsCompleted = false
                    });
                }
            }
            await _context.Quests.AddRangeAsync(newQuests);
            await _context.SaveChangesAsync();
            _lastUpdateTime = DateTime.UtcNow;
        }
        [HttpPost("claim-reward")]
        public async Task<IActionResult> ClaimQuestReward(int questId)
        {
            var quest = await _context.Quests
                .Include(q => q.User)
                .Include(q => q.Template)
                .FirstOrDefaultAsync(q => q.Id == questId);

            if (quest == null) return NotFound();
            if (!quest.IsCompleted) return BadRequest("Квест не выполнен");
            if (quest.IsRewardClaimed) return BadRequest("Награда уже получена");

            quest.User.ExperiencePoints += quest.Template.RewardExperience;
            quest.IsRewardClaimed = true;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                newExperience = quest.User.ExperiencePoints,
                reward = quest.Template.RewardExperience
            });
        }
        [HttpGet("progress")]
        public async Task<ActionResult<IEnumerable<UserQuestProgress>>> GetUserProgress([FromQuery] int userId)
        {
            return await _context.Quests
                .Where(q => q.UserId == userId && q.EndDate >= DateTime.UtcNow)
                .Include(q => q.Template)
                .Select(q => new UserQuestProgress
                {
                    QuestId = q.Id,
                    Title = q.Template.Title,
                    Progress = q.CurrentProgress,
                    Goal = q.Template.GoalValue,
                    IsCompleted = q.IsCompleted
                })
                .ToListAsync();
        }
        [HttpGet("user-quests")]
        public async Task<ActionResult<IEnumerable<UserQuest>>> GetUserQuests(int userId)
        {
            var userQuests = await _context.UserQuests
                .Include(uq => uq.Quest)
                    .ThenInclude(q => q.Template)
                .Where(uq => uq.UserId == userId)
                .ToListAsync();

            return Ok(userQuests);
        }
        [HttpPost("update-progress")]
        public async Task<IActionResult> UpdateUserQuestProgress(int userId, int questId, int progressDelta)
        {
            var userQuest = await _context.UserQuests
                .Include(uq => uq.Quest)
                    .ThenInclude(q => q.Template)
                .FirstOrDefaultAsync(uq => uq.UserId == userId && uq.QuestId == questId);

            if (userQuest == null)
                return NotFound("Прогресс по этому квесту не найден.");

            if (userQuest.IsCompleted)
                return BadRequest("Квест уже завершён.");

            userQuest.CurrentProgress += progressDelta;

            if (userQuest.CurrentProgress >= userQuest.TargetProgress)
            {
                userQuest.IsCompleted = true;
                userQuest.DateCompleted = DateTime.UtcNow;
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.ExperiencePoints += userQuest.Quest.Template.RewardExperience;
                }
            }
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Прогресс обновлён.",
                current = userQuest.CurrentProgress,
                isCompleted = userQuest.IsCompleted
            });
        }
        [HttpPost("complete/{questId}")]
        public async Task<IActionResult> CompleteQuest(int questId)
        {
            var quest = await _context.Quests
                .Include(q => q.Template)
                .Include(q => q.User)
                .FirstOrDefaultAsync(q => q.Id == questId);
            if (quest == null)
                return NotFound("Квест не найден.");
            if (quest.IsCompleted)
                return BadRequest("Квест уже завершён.");
            if (quest.CurrentProgress < quest.Template.GoalValue)
                return BadRequest("Цель квеста ещё не достигнута.");
            quest.User.ExperiencePoints += quest.Template.RewardExperience;
            quest.IsCompleted = true;
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Квест завершён, опыт начислен.",
                experienceAwarded = quest.Template.RewardExperience,
                totalExperience = quest.User.ExperiencePoints
            });
        }
    }
    public class UserQuestProgress
    {
        public int QuestId { get; set; }
        public string Title { get; set; }
        public int Progress { get; set; }
        public int Goal { get; set; }
        public bool IsCompleted { get; set; }
    }
}
using DIplomServer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Win32;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace DIplomServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HabitLogController : ControllerBase
    {
        private readonly HbtContext _context;

        public HabitLogController(HbtContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<HabitLog>> AddLog([FromBody] HabitLog log)
        {
            log.Timestamp = DateTime.UtcNow;
            _context.HabitLogs.Add(log);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLog), new { id = log.Id }, log);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<HabitLog>>> GetUserLogs(int userId)
        {
            return await _context.HabitLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        

        [HttpGet("{id}")]
        public async Task<ActionResult<HabitLog>> GetLog(int id)
        {
            var log = await _context.HabitLogs.FindAsync(id);
            if (log == null)
                return NotFound("Запись лога не найдена.");

            return Ok(log);
        }
    }
}

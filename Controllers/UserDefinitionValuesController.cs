using diji_card_alt.Data;
using diji_card_alt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace diji_card_alt_full.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDefinitionValuesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserDefinitionValuesController(AppDbContext context) => _context = context;

        // GET: api/userdefinitionvalues/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserLinks(string userId)
        {
            var links = await _context.UserDefinitionValues
                .Include(x => x.Definition)
                .Where(x => x.UserId == userId)
                .ToListAsync();

            return Ok(links);
        }

        // GET: api/userdefinitionvalues/{userId}/{definitionId}
        [HttpGet("{userId}/{definitionId}")]
        public async Task<IActionResult> GetById(string userId, int definitionId)
        {
            var link = await _context.UserDefinitionValues
                .Include(x => x.Definition)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.DefinitionId == definitionId);

            return link is null ? NotFound() : Ok(link);
        }

        // POST: api/userdefinitionvalues
        [HttpPost]
        public async Task<IActionResult> AddUserLink([FromBody] UserDefinitionValue dto)
        {
            var exists = await _context.UserDefinitionValues
                .AnyAsync(x => x.UserId == dto.UserId && x.DefinitionId == dto.DefinitionId);

            if (exists)
                return Conflict("Bu tanım zaten eklenmiş."); // 409

            _context.UserDefinitionValues.Add(dto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { userId = dto.UserId, definitionId = dto.DefinitionId }, dto);
        }


        // DELETE: api/userdefinitionvalues/{userId}/{definitionId}
        [HttpDelete("{userId}/{definitionId}")]
        public async Task<IActionResult> DeleteUserLink(string userId, int definitionId)
        {
            // Composite PK ile sorgula
            var entity = await _context.UserDefinitionValues
                .FindAsync(userId, definitionId);

            if (entity is null)
                return NotFound();

            _context.UserDefinitionValues.Remove(entity);
            await _context.SaveChangesAsync();

            // Başarıyla silindi: 204 No Content
            return NoContent();
        }


        // PUT: api/userdefinitionvalues/{userId}/{definitionId}
        [HttpPut("{userId}/{definitionId}")]
        public async Task<IActionResult> UpdateUserLink(
            string userId,
            int definitionId,
            [FromBody] UserDefinitionValue dto)
        {
            if (dto.UserId != userId || dto.DefinitionId != definitionId)
                return BadRequest();

            var entity = await _context.UserDefinitionValues
                .FindAsync(userId, definitionId);

            if (entity == null)
                return NotFound();

            entity.Value = dto.Value;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/userdefinitionvalues/sort
        [HttpPut("sort")]
        public async Task<IActionResult> UpdateSortOrder([FromBody] List<UserDefinitionValue> updatedValues)
        {
            foreach (var updatedValue in updatedValues)
            {
                var entity = await _context.UserDefinitionValues.FindAsync(updatedValue.UserId, updatedValue.DefinitionId);
                if (entity != null)
                {
                    entity.SortId = updatedValue.SortId;
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}

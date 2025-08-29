using diji_card_alt.Data;
using diji_card_alt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

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
            var links = await (from udv in _context.UserDefinitionValues
                              join d in _context.Definitions on udv.DefinitionId equals d.DefinitionId
                              where udv.UserId == userId
                              orderby udv.SortId
                              select new
                              {
                                  udv.Id,
                                  udv.UserId,
                                  udv.DefinitionId,
                                  udv.Value,
                                  udv.SortId,
                                  udv.CustomDefinitionName,
                                  DisplayName = udv.DefinitionId == 11 ? 
                                      (udv.CustomDefinitionName ?? "Custom") : 
                                      d.DefinitionName,
                                  IsCustom = udv.DefinitionId == 11
                              }).ToListAsync();

            return Ok(links);
        }

        // GET: api/userdefinitionvalues/{userId}/{definitionId}
        [HttpGet("{userId}/{definitionId}")]
        public async Task<IActionResult> GetById(string userId, int definitionId)
        {
            var links = await (from udv in _context.UserDefinitionValues
                              join d in _context.Definitions on udv.DefinitionId equals d.DefinitionId
                              where udv.UserId == userId && udv.DefinitionId == definitionId
                              orderby udv.SortId
                              select new
                              {
                                  udv.Id,
                                  udv.UserId,
                                  udv.DefinitionId,
                                  udv.Value,
                                  udv.SortId,
                                  udv.CustomDefinitionName,
                                  DisplayName = udv.DefinitionId == 11 ? 
                                      (udv.CustomDefinitionName ?? "Custom") : 
                                      d.DefinitionName,
                                  IsCustom = udv.DefinitionId == 11
                              }).ToListAsync();

            return Ok(links);
        }

        // GET: api/userdefinitionvalues/byid/{id}
        [HttpGet("byid/{id}")]
        public async Task<IActionResult> GetByAutoId(int id)
        {
            var link = await (from udv in _context.UserDefinitionValues
                             join d in _context.Definitions on udv.DefinitionId equals d.DefinitionId
                             where udv.Id == id
                             select new
                             {
                                 udv.Id,
                                 udv.UserId,
                                 udv.DefinitionId,
                                 udv.Value,
                                 udv.SortId,
                                 udv.CustomDefinitionName,
                                 DisplayName = udv.DefinitionId == 11 ? 
                                     (udv.CustomDefinitionName ?? "Custom") : 
                                     d.DefinitionName,
                                 IsCustom = udv.DefinitionId == 11
                             }).FirstOrDefaultAsync();

            return link is null ? NotFound() : Ok(link);
        }

        private bool IsOwner(string targetUserId)
        {
            // Extract userId from JWT (uid or sub)
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ")) return false;
            var token = authHeader.Substring("Bearer ".Length).Trim();
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var userId = jwt.Claims.FirstOrDefault(c => c.Type == "uid" || c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                return userId == targetUserId;
            }
            catch { return false; }
        }

        // POST: api/userdefinitionvalues
        [HttpPost]
        public async Task<IActionResult> AddUserLink([FromBody] UserDefinitionValue dto)
        {
            if (!IsOwner(dto.UserId)) return Unauthorized(new { Message = "CanEdit=false" });
            // For regular definitions (non-custom), check if already exists
            if (dto.DefinitionId != 11 && string.IsNullOrEmpty(dto.CustomDefinitionName))
            {
                var exists = await _context.UserDefinitionValues
                    .AnyAsync(x => x.UserId == dto.UserId && x.DefinitionId == dto.DefinitionId && x.CustomDefinitionName == null);

                if (exists)
                    return Conflict("Bu tanım zaten eklenmiş."); // 409
            }
            // For custom definitions (DefinitionId = 11), check if custom name already exists
            else if (dto.DefinitionId == 11 && !string.IsNullOrEmpty(dto.CustomDefinitionName))
            {
                var exists = await _context.UserDefinitionValues
                    .AnyAsync(x => x.UserId == dto.UserId && x.DefinitionId == 11 && x.CustomDefinitionName == dto.CustomDefinitionName);

                if (exists)
                    return Conflict("Bu custom tanım ismi zaten kullanılıyor."); // 409
            }

            _context.UserDefinitionValues.Add(dto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByAutoId),
                new { id = dto.Id }, dto);
        }

        // POST: api/userdefinitionvalues/custom
        [HttpPost("custom")]
        public async Task<IActionResult> AddCustomUserLink([FromBody] AddCustomDefinitionRequest request)
        {
            try
            {
                if (!IsOwner(request.UserId)) return Unauthorized(new { Message = "CanEdit=false" });
                if (request == null)
                {
                    return BadRequest(new { Success = false, Message = "Request boş olamaz" });
                }

                if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.CustomDefinitionName) || string.IsNullOrEmpty(request.Value))
                {
                    return BadRequest(new { Success = false, Message = "Gerekli alanlar boş olamaz" });
                }

                // Custom definition entry oluştur (DefinitionId = 11 kullan)
                var customEntry = new UserDefinitionValue
                {
                    UserId = request.UserId,
                    DefinitionId = 11, // Custom definition'lar için sabit ID
                    CustomDefinitionName = request.CustomDefinitionName,
                    Value = request.Value,
                    SortId = request.SortId
                };

                _context.UserDefinitionValues.Add(customEntry);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    Success = true, 
                    Message = "Custom definition eklendi",
                    DefinitionId = 11,
                    CustomEntry = customEntry 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Hata: {ex.Message}", StackTrace = ex.StackTrace });
            }
        }


        // DELETE: api/userdefinitionvalues/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserLink(int id)
        {
            // Use auto-increment ID
            var entity = await _context.UserDefinitionValues
                .FindAsync(id);

            if (entity is null)
                return NotFound();

            if (!IsOwner(entity.UserId)) return Unauthorized(new { Message = "CanEdit=false" });

            _context.UserDefinitionValues.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/userdefinitionvalues/{userId}/{definitionId} (Backward compatibility)
        [HttpDelete("{userId}/{definitionId}")]
        public async Task<IActionResult> DeleteUserLinkByUserAndDefinition(string userId, int definitionId)
        {
            var entity = await _context.UserDefinitionValues
                .FirstOrDefaultAsync(x => x.UserId == userId && x.DefinitionId == definitionId);

            if (entity is null)
                return NotFound();

            if (!IsOwner(entity.UserId)) return Unauthorized(new { Message = "CanEdit=false" });

            _context.UserDefinitionValues.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // PUT: api/userdefinitionvalues/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserLink(
            int id,
            [FromBody] UserDefinitionValue dto)
        {
            if (dto.Id != id)
                return BadRequest();

            // Use auto-increment ID
            var entity = await _context.UserDefinitionValues
                .FindAsync(id);

            if (entity == null)
                return NotFound();

            if (!IsOwner(entity.UserId)) return Unauthorized(new { Message = "CanEdit=false" });

            entity.Value = dto.Value;
            entity.CustomDefinitionName = dto.CustomDefinitionName; // Update custom name if provided
            entity.SortId = dto.SortId;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/userdefinitionvalues/byid/{id} (For custom definitions by auto-increment ID only)
        [HttpPut("byid/{id}")]
        public async Task<IActionResult> UpdateUserLinkByIdOnly(
            int id,
            [FromBody] UpdateLinkRequest dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Value))
                return BadRequest("Value is required");

            var entity = await _context.UserDefinitionValues
                .FindAsync(id);

            if (entity == null)
                return NotFound();

            if (!IsOwner(entity.UserId)) return Unauthorized(new { Message = "CanEdit=false" });

            entity.Value = dto.Value;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/userdefinitionvalues/{userId}/{definitionId} (Backward compatibility)
        [HttpPut("{userId}/{definitionId}")]
        public async Task<IActionResult> UpdateUserLinkByUserAndDefinition(
            string userId,
            int definitionId,
            [FromBody] UpdateLinkRequest dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Value))
                return BadRequest("Value is required");

            var entity = await _context.UserDefinitionValues
                .FirstOrDefaultAsync(x => x.UserId == userId && x.DefinitionId == definitionId);

            if (entity == null)
                return NotFound();

            if (!IsOwner(entity.UserId)) return Unauthorized(new { Message = "CanEdit=false" });

            entity.Value = dto.Value;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/userdefinitionvalues/sort
        [HttpPut("sort")]
        public async Task<IActionResult> UpdateSortOrder([FromBody] List<UserDefinitionValue> updatedValues)
        {
            if (updatedValues.Any() && !IsOwner(updatedValues.First().UserId)) return Unauthorized(new { Message = "CanEdit=false" });
            foreach (var updatedValue in updatedValues)
            {
                // Use auto-increment ID
                var entity = await _context.UserDefinitionValues
                    .FindAsync(updatedValue.Id);
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

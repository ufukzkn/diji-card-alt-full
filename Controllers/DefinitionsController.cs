using diji_card_alt.Data;
using diji_card_alt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace diji_card_alt_full.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DefinitionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DefinitionsController(AppDbContext context)
        {
            _context = context;
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<Definition>> GetDefinitionById(int id)
        {
            var definition = await _context.Definitions.FindAsync(id);
            if (definition == null)
                return NotFound();

            return definition;
        }


        // GET: api/definitions
        [HttpGet]
        public async Task<IActionResult> GetDefinitions()
        {
            var defs = await _context.Definitions.ToListAsync();
            return Ok(defs);
        }

        // POST: api/definitions
        [HttpPost]
        public async Task<IActionResult> AddDefinition([FromBody] Definition definition)
        {
            var entity = new Definition
            {
                DefinitionName = definition.DefinitionName
            };

            _context.Definitions.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDefinitionById), new { id = entity.DefinitionId }, entity);
        }

        // DELETE: api/definitions/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDefinition(int id)
        {
            var definition = await _context.Definitions.FindAsync(id);
            
            if (definition == null)
            {
                return NotFound();
            }

            // İlgili UserDefinitionValue'ları da sil
            var relatedValues = await _context.UserDefinitionValues
                .Where(udv => udv.DefinitionId == id)
                .ToListAsync();
                
            _context.UserDefinitionValues.RemoveRange(relatedValues);
            _context.Definitions.Remove(definition);

            // Sequence'i güncelle
            await _context.Database.ExecuteSqlRawAsync(
                "SELECT setval('\"Definitions_DefinitionId_seq\"', (SELECT COALESCE(MAX(\"DefinitionId\"), 0) FROM \"Definitions\"));"
            );
            
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}

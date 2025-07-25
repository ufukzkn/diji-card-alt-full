using Microsoft.AspNetCore.Mvc;
using diji_card_alt.Models;
using diji_card_alt.Data;

namespace diji_card_alt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public IActionResult GetUserById(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] User newUser)
        {
            if (_context.Users.Any(u => u.UserId == newUser.UserId))
                return Conflict("Bu kullanıcı zaten var.");

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetUserById), new { userId = newUser.UserId }, newUser);
        }
        [HttpDelete("{userId}")]
        public IActionResult DeleteUser(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpPut("{userId}")]
        public IActionResult UpdateUser(string userId, [FromBody] User updatedUser)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (existingUser == null)
                return NotFound();

            existingUser.FullName = updatedUser.FullName;
            existingUser.Email = updatedUser.Email;
            existingUser.PhoneNumber = updatedUser.PhoneNumber;
            
            existingUser.JobTitle = updatedUser.JobTitle;
            existingUser.Company = updatedUser.Company;
            

            _context.SaveChanges();

            return Ok(existingUser);
        }


        [HttpGet("search")]
        public IActionResult SearchUsersByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("İsim sorgusu boş olamaz.");

            var users = _context.Users
                .Where(u => u.FullName.Contains(name))
                .ToList();

            return Ok(users);
        }




    }
}

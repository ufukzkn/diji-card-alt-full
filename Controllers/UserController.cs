using Microsoft.AspNetCore.Mvc;
using diji_card_alt.Models;
using diji_card_alt.Data;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;

namespace diji_card_alt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UserController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private string? GetTokenUserId()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ")) return null;
            var token = authHeader.Substring("Bearer ".Length).Trim();
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                return jwt.Claims.FirstOrDefault(c => c.Type == "uid" || c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            }
            catch { return null; }
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
            var caller = GetTokenUserId();
            
            if (string.IsNullOrEmpty(caller))
            {
                // Sadece public profilleri göster
                var publicUsers = from u in _context.Users
                                 where u.IsPublic
                                 select u;
                return Ok(publicUsers.ToList());
            }
            else
            {
                // Public profiller + kendi profili
                var visibleUsers = from u in _context.Users
                                  where u.IsPublic || u.UserId == caller
                                  select u;
                return Ok(visibleUsers.ToList());
            }
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
            var caller = GetTokenUserId();
            
            if (string.IsNullOrEmpty(caller))
            {
                // Sadece public profilleri ara
                var publicUsers = from u in _context.Users
                                 where u.IsPublic && u.FullName.Contains(name)
                                 select u;
                return Ok(publicUsers.ToList());
            }
            else
            {
                // Public profiller + kendi profili
                var visibleUsers = from u in _context.Users
                                  where (u.IsPublic || u.UserId == caller) && u.FullName.Contains(name)
                                  select u;
                return Ok(visibleUsers.ToList());
            }
        }

        [HttpPost("{userId}/profile-photo")]
        public async Task<IActionResult> UploadProfilePhoto(string userId, IFormFile photo)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            if (photo == null || photo.Length == 0)
                return BadRequest("No file uploaded");

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(photo.ContentType.ToLower()))
                return BadRequest("Invalid file type. Only JPEG, PNG and GIF are allowed.");

            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var fileName = $"{userId}_{DateTime.Now.Ticks}{Path.GetExtension(photo.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(fileStream);
            }

            // Update user profile
            user.ProfilePhotoUrl = $"/uploads/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { profilePhotoUrl = user.ProfilePhotoUrl });
        }

        [HttpPut("{userId}/theme")]
        public async Task<IActionResult> UpdateTheme(string userId, [FromBody] ThemeUpdateModel theme)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            // Theme ayarları kaldırıldı - şimdilik sadece kullanıcı bilgilerini güncelliyoruz
            // user.ThemeColor = theme.ThemeColor;
            // user.FontFamily = theme.FontFamily;
            // user.CardLayout = theme.CardLayout;

            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPut("{userId}/preferences")]
        public async Task<IActionResult> UpdatePreferences(string userId, [FromBody] PreferencesUpdateModel preferences)
        {
            var caller = GetTokenUserId();
            if (caller != userId)
                return Forbid("Bu işlem için yetkiniz yok.");

            var userPreferences = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (userPreferences == null)
            {
                // Preferences yoksa oluştur
                userPreferences = new DigitalBusinessCard.Models.UserPreferences
                {
                    UserId = userId,
                    ViewMode = "list",
                    GridColumns = 3,
                    ThemeColor = "orange",
                    FontFamily = "Inter"
                };
                _context.UserPreferences.Add(userPreferences);
            }

            // User'ı bul ve IsPublic'i orada güncelle
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Güncelle
            if (preferences.IsPublic.HasValue)
                user.IsPublic = preferences.IsPublic.Value;
            if (!string.IsNullOrEmpty(preferences.ViewMode))
                userPreferences.ViewMode = preferences.ViewMode;
            if (preferences.GridColumns.HasValue && preferences.GridColumns >= 3 && preferences.GridColumns <= 5)
                userPreferences.GridColumns = preferences.GridColumns.Value;
            if (!string.IsNullOrEmpty(preferences.ThemeColor))
                userPreferences.ThemeColor = preferences.ThemeColor;
            if (!string.IsNullOrEmpty(preferences.FontFamily))
                userPreferences.FontFamily = preferences.FontFamily;

            await _context.SaveChangesAsync();

            return Ok(userPreferences);
        }

        [HttpGet("{userId}/preferences")]
        public async Task<IActionResult> GetPreferences(string userId)
        {
            try
            {
                var preferences = await _context.UserPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId);
                    
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound();

                if (preferences == null)
                {
                    // Default preferences döndür
                    return Ok(new PreferencesUpdateModel
                    {
                        IsPublic = user.IsPublic,
                        ViewMode = "list",
                        GridColumns = 3,
                        ThemeColor = "#007bff",
                        FontFamily = "Inter"
                    });
                }

                return Ok(new PreferencesUpdateModel
                {
                    IsPublic = user.IsPublic,
                    ViewMode = preferences.ViewMode,
                    GridColumns = preferences.GridColumns,
                    ThemeColor = preferences.ThemeColor,
                    FontFamily = preferences.FontFamily
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Preferences alınırken hata oluştu.", error = ex.Message });
            }
        }
    }

    public class ThemeUpdateModel
    {
        public string? ThemeColor { get; set; }
        public string? FontFamily { get; set; }
        public string? CardLayout { get; set; }
    }

    public class PreferencesUpdateModel
    {
        public bool? IsPublic { get; set; }
        public string? ViewMode { get; set; }
        public int? GridColumns { get; set; }
        public string? ThemeColor { get; set; }
        public string? FontFamily { get; set; }
    }
}

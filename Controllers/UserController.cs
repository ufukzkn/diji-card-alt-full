using Microsoft.AspNetCore.Mvc;
using diji_card_alt.Models;
using diji_card_alt.Data;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Buffers;

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
            // Prefer validated principal if authenticated
            if (User?.Identity?.IsAuthenticated == true)
            {
                return User.FindFirst("uid")?.Value
                       ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                       ?? User.FindFirst("sub")?.Value;
            }
            // Fallback legacy manual parsing for backward compatibility
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

        private static UserDto ToDto(User u) => new(
            u.UserId,
            u.FullName,
            u.PhoneNumber,
            u.Email,
            u.JobTitle,
            u.Company,
            u.IsPublic,
            u.ProfilePhotoUrl
        );

        private static bool IsValidLength(string? value, int max) => string.IsNullOrWhiteSpace(value) || value.Length <= max;

        private string? ValidateUserInput(User user)
        {
            if (!IsValidLength(user.FullName, 100)) return "FullName çok uzun";
            if (!IsValidLength(user.Company, 100)) return "Company çok uzun";
            if (!IsValidLength(user.JobTitle, 100)) return "JobTitle çok uzun";
            if (!IsValidLength(user.Email, 150)) return "Email çok uzun";
            if (!IsValidLength(user.PhoneNumber, 40)) return "PhoneNumber çok uzun";
            return null;
        }

        private static bool IsAllowedImageSignature(ReadOnlySpan<byte> header, string contentType)
        {
            // JPEG
            if (contentType.Contains("jpeg") || contentType.Contains("jpg"))
            {
                if (header.Length >= 2 && header[0] == 0xFF && header[1] == 0xD8) return true;
            }
            // PNG
            if (contentType.Contains("png"))
            {
                byte[] sig = new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A};
                if (header.Length >= sig.Length && header.Slice(0, sig.Length).SequenceEqual(sig)) return true;
            }
            // GIF
            if (contentType.Contains("gif"))
            {
                if (header.Length >= 6)
                {
                    var s = Encoding.ASCII.GetString(header.Slice(0,6));
                    if (s == "GIF87a" || s == "GIF89a") return true;
                }
            }
            return false;
        }

        [HttpGet("{userId}")]
        public IActionResult GetUserById(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound();
            return Ok(ToDto(user));
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
                return Ok(publicUsers.Select(ToDto).ToList());
            }
            else
            {
                // Public profiller + kendi profili
                var visibleUsers = from u in _context.Users
                                  where u.IsPublic || u.UserId == caller
                                  select u;
                return Ok(visibleUsers.Select(ToDto).ToList());
            }
        }

    [HttpPost]
    [AllowAnonymous]
    // Registration endpoint intentionally left unauthenticated so new users can be created.
        public IActionResult CreateUser([FromBody] User newUser)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Message = "Validation failed", Errors = errors });
            }
            if (_context.Users.Any(u => u.UserId == newUser.UserId))
                return Conflict("Bu kullanıcı zaten var.");

            var validationError = ValidateUserInput(newUser);
            if (validationError != null) return BadRequest(new { Message = validationError });

            _context.Users.Add(newUser);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetUserById), new { userId = newUser.UserId }, ToDto(newUser));
        }
        [HttpDelete("{userId}")]
        [Authorize]
        public IActionResult DeleteUser(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound();

            var caller = GetTokenUserId();
            if (caller != userId) return Forbid();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpPut("{userId}")]
        [Authorize]
        public IActionResult UpdateUser(string userId, [FromBody] User updatedUser)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (existingUser == null)
                return NotFound();

            var caller = GetTokenUserId();
            if (caller != userId) return Forbid();

            var validationError = ValidateUserInput(updatedUser);
            if (validationError != null) return BadRequest(new { Message = validationError });

            existingUser.FullName = updatedUser.FullName;
            existingUser.Email = updatedUser.Email;
            existingUser.PhoneNumber = updatedUser.PhoneNumber;
            
            existingUser.JobTitle = updatedUser.JobTitle;
            existingUser.Company = updatedUser.Company;
            

            _context.SaveChanges();

            return Ok(ToDto(existingUser));
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
                return Ok(publicUsers.Select(ToDto).ToList());
            }
            else
            {
                // Public profiller + kendi profili
                var visibleUsers = from u in _context.Users
                                  where (u.IsPublic || u.UserId == caller) && u.FullName.Contains(name)
                                  select u;
                return Ok(visibleUsers.Select(ToDto).ToList());
            }
        }

        [HttpPost("{userId}/profile-photo")]
        [RequestSizeLimit(5_242_880)] // ~5MB
        [Authorize]
        public async Task<IActionResult> UploadProfilePhoto(string userId, IFormFile photo)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            var caller = GetTokenUserId();
            if (caller != userId) return Forbid();

            if (photo == null || photo.Length == 0)
                return BadRequest("No file uploaded");

            // Validate file type + signature
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            var contentType = photo.ContentType.ToLower();
            if (!allowedTypes.Contains(contentType))
                return BadRequest("Invalid file type. Only JPEG, PNG and GIF are allowed.");

            // Read first bytes for signature
            byte[] header = ArrayPool<byte>.Shared.Rent(16);
            try
            {
                using var hs = photo.OpenReadStream();
                int read = await hs.ReadAsync(header, 0, 16);
                if (!IsAllowedImageSignature(new ReadOnlySpan<byte>(header,0,read), contentType))
                    return BadRequest("Invalid image signature.");
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(header);
            }

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
        [Authorize]
        public async Task<IActionResult> UpdateTheme(string userId, [FromBody] ThemeUpdateModel theme)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            var caller = GetTokenUserId();
            if (caller != userId) return Forbid();

            // Theme ayarları kaldırıldı - şimdilik sadece kullanıcı bilgilerini güncelliyoruz
            // user.ThemeColor = theme.ThemeColor;
            // user.FontFamily = theme.FontFamily;
            // user.CardLayout = theme.CardLayout;

            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPut("{userId}/preferences")]
    [Authorize]
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

            return Ok(new {
                userPreferences.UserId,
                userPreferences.ViewMode,
                userPreferences.GridColumns,
                userPreferences.ThemeColor,
                userPreferences.FontFamily,
                IsPublic = user.IsPublic
            });
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

                return Ok(new {
                    IsPublic = user.IsPublic,
                    preferences.ViewMode,
                    preferences.GridColumns,
                    preferences.ThemeColor,
                    preferences.FontFamily
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

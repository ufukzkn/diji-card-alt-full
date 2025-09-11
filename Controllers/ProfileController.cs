using diji_card_alt.Data;
using diji_card_alt.Models;
using diji_card_alt_full.Dtos;
using DigitalBusinessCard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;

namespace diji_card_alt_full.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IConfiguration _config;
    public ProfileController(AppDbContext ctx, IConfiguration config)
    {
        _ctx = ctx;
        _config = config;
    }

    private string? GetCallerUserId()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return User.FindFirst("userId")?.Value
                ?? User.FindFirst("uid")?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst("nameid")?.Value;
        }
        return null;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<PrivateProfileResponse>> GetProfile(string userId)
    {
        // 1) Kullanıcının sabit alanları (Users tablosu)
        var user = await _ctx.Users.FindAsync(userId);
        if (user is null) return NotFound();

        // 2) Privacy kontrolü - User'dan IsPublic kontrol et  
    var caller = GetCallerUserId(); // Doğrulanmış principal
        
        Console.WriteLine($"[DEBUG GetProfile] userId: {userId}, caller: {caller}, IsPublic: {user.IsPublic}");
        
        // Eğer profil private ise ve caller kendisi değilse private response döndür
        if (!user.IsPublic && (string.IsNullOrEmpty(caller) || caller != userId))
        {
            return Ok(new PrivateProfileResponse
            {
                IsPublic = false,
                AccessGranted = false,
                Message = "Bu profil özeldir. Erişim için şifre gereklidir.",
                ProfileData = null
            });
        }

        // 2) Dinamik linkler (UserDefinitionValues) + DefinitionName
        // DefinitionId = 11 ise CustomDefinitionName kullan, değilse Definition.DefinitionName kullan
        var linksFromUdvs = await (from udv in _ctx.UserDefinitionValues
                                  join d in _ctx.Definitions on udv.DefinitionId equals d.DefinitionId
                                  where udv.UserId == userId
                                  select new LinkDto(
                                      udv.DefinitionId == 11 ? udv.CustomDefinitionName ?? "Custom" : d.DefinitionName, 
                                      udv.Value, 
                                      udv.SortId))
                                  .ToListAsync();

        // 3) Sabit alanları DefinitionName’leriyle birlikte DTO’ya ekle
        var defaultLinks = new List<LinkDto>
        {
            new("Full Name", user.FullName  ?? string.Empty, 0),
            new("Company"  , user.Company   ?? string.Empty, 1),
            new("E-mail"   , user.Email     ?? string.Empty, 2),
            new("Phone"    , user.PhoneNumber ?? string.Empty, 3)
        };

        // 4) Hepsini birleştir
        var allLinks = defaultLinks.Concat(linksFromUdvs).ToList();

    // Default profile photo fallback
    var photoUrl = string.IsNullOrEmpty(user.ProfilePhotoUrl) ? "/profile-photos/default.png" : user.ProfilePhotoUrl;
    var profileDto = new UserProfileDto(userId, allLinks, photoUrl);
        return Ok(new PrivateProfileResponse
        {
            IsPublic = user.IsPublic,
            AccessGranted = true,
            Message = "Erişim başarılı.",
            ProfileData = profileDto
        });
    }

    [HttpPost("{userId}/photo")]
    [RequestSizeLimit(5_242_880)] // ~5MB
    [Authorize]
    public async Task<IActionResult> UploadProfilePhoto(string userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya seçilmedi.");

        // Sunucu tarafı maksimum boyut kontrolü (5MB)
        const long maxBytes = 5L * 1024 * 1024; // 5MB
        if (file.Length > maxBytes)
            return BadRequest("Dosya boyutu 5MB sınırını aşıyor.");

        // Sadece resim dosyalarına izin ver
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest("Sadece JPEG, PNG ve GIF dosyalarına izin verilir.");

    var caller = GetCallerUserId();
    if (caller != userId) return Forbid();
    var user = await _ctx.Users.FindAsync(userId);
        if (user == null)
            return NotFound();

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profile-photos");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // Eski fotoğrafı sil
        if (!string.IsNullOrEmpty(user.ProfilePhotoUrl))
        {
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePhotoUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        var fileExt = Path.GetExtension(file.FileName);
        var fileName = $"{userId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{fileExt}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        user.ProfilePhotoUrl = $"/profile-photos/{fileName}";
        await _ctx.SaveChangesAsync();

        return Ok(new { photoUrl = user.ProfilePhotoUrl });
    }

    [HttpDelete("{userId}/photo")]
    [Authorize]
    public async Task<IActionResult> DeleteProfilePhoto(string userId)
    {
        var caller = GetCallerUserId();
        if (caller != userId) return Forbid();
        var user = await _ctx.Users.FindAsync(userId);
        if (user == null)
            return NotFound();
        if (!string.IsNullOrEmpty(user.ProfilePhotoUrl))
        {
            var photoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePhotoUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (System.IO.File.Exists(photoPath))
                System.IO.File.Delete(photoPath);
            user.ProfilePhotoUrl = null;
            await _ctx.SaveChangesAsync();
        }
        return NoContent();
    }

    // GET: api/profile/{userId}/custom-definitions
    [HttpGet("{userId}/custom-definitions")]
    public async Task<ActionResult> GetUserCustomDefinitions(string userId)
    {
        var user = await _ctx.Users.FindAsync(userId);
        if (user is null) return NotFound();

        // Private profil ise ve caller sahibi değilse boş liste dön
        var caller = GetCallerUserId();
        if (!user.IsPublic && caller != userId)
        {
            return Ok(Array.Empty<object>());
        }

        var customDefinitions = await _ctx.UserDefinitionValues
            .Where(x => x.UserId == userId && x.DefinitionId == 11)
            .Select(x => new 
            {
                UserId = x.UserId,
                DefinitionId = x.DefinitionId,
                Value = x.Value,
                SortId = x.SortId,
                DefinitionName = x.CustomDefinitionName
            })
            .ToListAsync();

        return Ok(customDefinitions);
    }

    // GET: api/profile/{userId}/custom-definition-names
    [HttpGet("{userId}/custom-definition-names")]
    public async Task<ActionResult<List<string>>> GetUserCustomDefinitionNames(string userId)
    {
        var user = await _ctx.Users.FindAsync(userId);
        if (user is null) return NotFound();

        var caller = GetCallerUserId();
        if (!user.IsPublic && caller != userId)
        {
            return Ok(new List<string>()); // boş
        }

        var customDefinitionNames = await _ctx.UserDefinitionValues
            .Where(x => x.UserId == userId && x.DefinitionId == 11 && !string.IsNullOrEmpty(x.CustomDefinitionName))
            .Select(x => x.CustomDefinitionName!)
            .Distinct()
            .ToListAsync();

        return Ok(customDefinitionNames);
    }

    [HttpPost("{userId}/verify-access")]
    public async Task<ActionResult<PrivateProfileResponse>> VerifyPrivateAccess(string userId, [FromBody] PrivateProfileAccessRequest request)
    {
        var user = await _ctx.Users.FindAsync(userId);
        if (user is null) return NotFound();

        // Eğer profil zaten public ise direkt erişim ver
        if (user.IsPublic)
        {
            return await GetBasicInfoData(userId, user, user.IsPublic);
        }

        // Özel access token kontrolü (URL'den gelen)
        if (!string.IsNullOrEmpty(request.AccessToken))
        {
            var accessRecord = await _ctx.PrivateProfileAccesses
                .FirstOrDefaultAsync(p => p.UserId == userId && 
                                         p.AccessToken == request.AccessToken && 
                                         p.IsActive &&
                                         (p.ExpiryDate == null || p.ExpiryDate > DateTime.UtcNow));

            if (accessRecord != null)
            {
                return await GetFullProfileData(userId, user, false); // Private profil ama erişim var - TAM PROFİL
            }
        }

        // Şifre kontrolü
        if (!string.IsNullOrEmpty(request.Password))
        {
            if (user.PrivateAccessPassword == request.Password)
            {
                return await GetBasicInfoData(userId, user, false); // Private profil ama erişim var
            }
            else
            {
                return Ok(new PrivateProfileResponse
                {
                    IsPublic = false,
                    AccessGranted = false,
                    Message = "Yanlış şifre girdiniz.",
                    ProfileData = null
                });
            }
        }

        return Ok(new PrivateProfileResponse
        {
            IsPublic = false,
            AccessGranted = false,
            Message = "Bu profil özeldir. Şifre veya özel link gereklidir.",
            ProfileData = null
        });
    }

    private Task<PrivateProfileResponse> GetBasicInfoData(string userId, User user, bool isPublic)
    {
        // Basic info döndür (links değil)
        var basicInfo = new
        {
            userId = userId,
            fullName = user.FullName,
            company = user.Company,
            jobTitle = user.JobTitle,
            email = user.Email,
            phoneNumber = user.PhoneNumber,
            profilePhotoUrl = string.IsNullOrEmpty(user.ProfilePhotoUrl) ? "/profile-photos/default.png" : user.ProfilePhotoUrl
        };

        return Task.FromResult(new PrivateProfileResponse
        {
            IsPublic = isPublic,
            AccessGranted = true,
            Message = "Erişim başarılı.",
            ProfileData = basicInfo
        });
    }

    // Şifre doğrulandıktan sonra tam profil data (basic + links) döndür
    [HttpPost("{userId}/verify-access-full")]
    public async Task<ActionResult<PrivateProfileResponse>> VerifyPrivateAccessFull(string userId, [FromBody] PrivateProfileAccessRequest request)
    {
        var user = await _ctx.Users.FindAsync(userId);
        if (user is null) return NotFound();

        // Eğer profil zaten public ise direkt erişim ver
        if (user.IsPublic)
        {
            return await GetFullProfileData(userId, user, user.IsPublic);
        }

        // Şifre kontrolü
        if (!string.IsNullOrEmpty(request.Password))
        {
            if (user.PrivateAccessPassword == request.Password)
            {
                return await GetFullProfileData(userId, user, false); // Private profil ama erişim var - TAM PROFİL
            }
            else
            {
                return Ok(new PrivateProfileResponse
                {
                    IsPublic = false,
                    AccessGranted = false,
                    Message = "Yanlış şifre girdiniz.",
                    ProfileData = null
                });
            }
        }

        return Ok(new PrivateProfileResponse
        {
            IsPublic = false,
            AccessGranted = false,
            Message = "Bu profil özeldir. Şifre gereklidir.",
            ProfileData = null
        });
    }

    private async Task<PrivateProfileResponse> GetFullProfileData(string userId, User user, bool isPublic)
    {
        // Dinamik linkler
        var linksFromUdvs = await (from udv in _ctx.UserDefinitionValues
                                  join d in _ctx.Definitions on udv.DefinitionId equals d.DefinitionId
                                  where udv.UserId == userId
                                  select new LinkDto(
                                      udv.DefinitionId == 11 ? udv.CustomDefinitionName ?? "Custom" : d.DefinitionName, 
                                      udv.Value, 
                                      udv.SortId))
                                  .ToListAsync();

        // Sabit alanları DefinitionName'leriyle birlikte DTO'ya ekle
        var defaultLinks = new List<LinkDto>
        {
            new("Full Name", user.FullName  ?? string.Empty, 0),
            new("Company"  , user.Company   ?? string.Empty, 1),
            new("E-mail"   , user.Email     ?? string.Empty, 2),
            new("Phone"    , user.PhoneNumber ?? string.Empty, 3)
        };

        // Hepsini birleştir
        var allLinks = defaultLinks.Concat(linksFromUdvs).ToList();

        // Hem basic info hem links içeren combined response
        var fullProfileData = new
        {
            userId = userId,
            fullName = user.FullName,
            company = user.Company,
            jobTitle = user.JobTitle,
            email = user.Email,
            phoneNumber = user.PhoneNumber,
            profilePhotoUrl = string.IsNullOrEmpty(user.ProfilePhotoUrl) ? "/profile-photos/default.png" : user.ProfilePhotoUrl,
            links = allLinks // Links'i de ekle
        };

        return new PrivateProfileResponse
        {
            IsPublic = isPublic,
            AccessGranted = true,
            Message = "Erişim başarılı.",
            ProfileData = fullProfileData
        };
    }

    private async Task<PrivateProfileResponse> GetProfileData(string userId, User user, bool isPublic)
    {
        // Dinamik linkler
        var linksFromUdvs = await (from udv in _ctx.UserDefinitionValues
                                  join d in _ctx.Definitions on udv.DefinitionId equals d.DefinitionId
                                  where udv.UserId == userId
                                  select new LinkDto(
                                      udv.DefinitionId == 11 ? udv.CustomDefinitionName ?? "Custom" : d.DefinitionName, 
                                      udv.Value, 
                                      udv.SortId))
                                  .ToListAsync();

        // Sabit alanlar
        var defaultLinks = new List<LinkDto>
        {
            new("Full Name", user.FullName  ?? string.Empty, 0),
            new("Company"  , user.Company   ?? string.Empty, 1),
            new("E-mail"   , user.Email     ?? string.Empty, 2),
            new("Phone"    , user.PhoneNumber ?? string.Empty, 3)
        };

        var allLinks = defaultLinks.Concat(linksFromUdvs).ToList();
        var profileDto = new UserProfileDto(userId, allLinks, user.ProfilePhotoUrl);

        return new PrivateProfileResponse
        {
            IsPublic = isPublic,
            AccessGranted = true,
            Message = "Erişim başarılı.",
            ProfileData = profileDto
        };
    }

    [HttpPut("{userId}/privacy-settings")]
    [Authorize]
    public async Task<ActionResult> UpdatePrivacySettings(string userId, [FromBody] UpdatePrivacySettingsRequest request)
    {
        var caller = GetCallerUserId();
        if (caller != userId) return Forbid("Sadece kendi profil ayarlarınızı değiştirebilirsiniz.");

        var user = await _ctx.Users.FindAsync(userId);
        if (user is null) return NotFound();

        // User'da IsPublic'i güncelle
        user.IsPublic = request.IsPublic;
        
        // Password sadece gönderildiğinde güncelle (null/empty değilse)
        if (!string.IsNullOrEmpty(request.PrivateAccessPassword))
        {
            user.PrivateAccessPassword = request.PrivateAccessPassword;
        }

        await _ctx.SaveChangesAsync();

        return Ok(new { Message = "Gizlilik ayarları güncellendi." });
    }

    [HttpPost("{userId}/create-special-link")]
    [EnableRateLimiting("SpecialLinkCreate")]
    [Authorize]
    public async Task<ActionResult> CreateSpecialLink(string userId, [FromBody] CreateSpecialLinkRequest request)
    {
        var caller = GetCallerUserId();
        if (caller != userId) return Forbid("Sadece kendi profiliniz için özel link oluşturabilirsiniz.");

        var user = await _ctx.Users.FindAsync(userId);
        if (user is null) return NotFound();

        // Random access token oluştur
        var accessToken = Guid.NewGuid().ToString("N");

        var specialAccess = new PrivateProfileAccess
        {
            UserId = userId,
            AccessToken = accessToken,
            CreatedDate = DateTime.UtcNow,
            ExpiryDate = request.ExpiryDate,
            IsActive = true,
            Description = request.Description ?? "Özel erişim linki"
        };

        _ctx.PrivateProfileAccesses.Add(specialAccess);
        await _ctx.SaveChangesAsync();

    var baseUrl = _config["PublicFrontendBaseUrl"] ?? "http://localhost:4200";
    var specialUrl = $"{baseUrl.TrimEnd('/')}/profil/{userId}?access={accessToken}";

        return Ok(new 
        { 
            AccessToken = accessToken,
            SpecialUrl = specialUrl,
            ExpiryDate = request.ExpiryDate,
            Description = specialAccess.Description
        });
    }

    [HttpGet("{userId}/special-links")]
    [Authorize]
    public async Task<ActionResult> GetSpecialLinks(string userId)
    {
        var caller = GetCallerUserId();
        if (caller != userId) return Forbid("Sadece kendi özel linklerinizi görebilirsiniz.");

        var links = await _ctx.PrivateProfileAccesses
            .Where(p => p.UserId == userId && p.IsActive)
            .Select(p => new 
            {
                Id = p.Id,
                AccessToken = p.AccessToken,
                CreatedDate = p.CreatedDate,
                ExpiryDate = p.ExpiryDate,
                Description = p.Description,
                SpecialUrl = $"{(_config["PublicFrontendBaseUrl"] ?? "http://localhost:4200").TrimEnd('/')}/profil/{userId}?access={p.AccessToken}"
            })
            .ToListAsync();

        return Ok(links);
    }

    [HttpDelete("{userId}/special-links/{linkId}")]
    [Authorize]
    public async Task<ActionResult> DeleteSpecialLink(string userId, int linkId)
    {
        var caller = GetCallerUserId();
        if (caller != userId) return Forbid("Sadece kendi özel linklerinizi silebilirsiniz.");

        var link = await _ctx.PrivateProfileAccesses.FindAsync(linkId);
        if (link is null || link.UserId != userId) return NotFound();

        link.IsActive = false; // Soft delete
        await _ctx.SaveChangesAsync();

        return Ok(new { Message = "Özel link silindi." });
    }

    [HttpGet("{userId}/basic-info")]
    public async Task<ActionResult<PrivateProfileResponse>> GetBasicInfo(string userId)
    {
        // 1) Kullanıcının sabit alanları (Users tablosu)
        var user = await _ctx.Users.FindAsync(userId);
        if (user is null) return NotFound();

        // 2) Privacy kontrolü - User'dan IsPublic kontrol et  
    var caller = GetCallerUserId(); // Doğrulanmış principal
        
        Console.WriteLine($"[DEBUG] userId: {userId}, caller: {caller}, IsPublic: {user.IsPublic}");
        
        // Eğer profil private ise ve caller kendisi değilse private response döndür
        // DÜZELTME: caller null ise veya caller kendisi değilse erişim reddet
        if (!user.IsPublic && (string.IsNullOrEmpty(caller) || caller != userId))
        {
            return Ok(new PrivateProfileResponse
            {
                IsPublic = false,
                AccessGranted = false,
                Message = "Bu profil özeldir. Erişim için şifre gereklidir.",
                ProfileData = null
            });
        }

        // 3) Erişim var ise basic info'yu döndür (sadece temel bilgiler)
        var basicInfo = new
        {
            userId = userId,
            fullName = user.FullName,
            company = user.Company,
            jobTitle = user.JobTitle,
            email = user.Email,
            phoneNumber = user.PhoneNumber,
            profilePhotoUrl = string.IsNullOrEmpty(user.ProfilePhotoUrl) ? "/profile-photos/default.png" : user.ProfilePhotoUrl
        };

        return Ok(new PrivateProfileResponse
        {
            IsPublic = user.IsPublic,
            AccessGranted = true,
            Message = "Erişim başarılı.",
            ProfileData = basicInfo
        });
    }
}

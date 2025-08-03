using diji_card_alt.Data;
using diji_card_alt.Models;
using diji_card_alt_full.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace diji_card_alt_full.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public ProfileController(AppDbContext ctx) => _ctx = ctx;

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserProfileDto>> GetProfile(string userId)
    {
        // 1) Kullanıcının sabit alanları (Users tablosu)
        var user = await _ctx.Users.FindAsync(userId);
        if (user is null) return NotFound();

        // 2) Dinamik linkler (UserDefinitionValues) + DefinitionName
        // DefinitionId = 11 ise CustomDefinitionName kullan, değilse Definition.DefinitionName kullan
        var linksFromUdvs = await _ctx.UserDefinitionValues
            .Where(x => x.UserId == userId)
            .Include(x => x.Definition)
            .Select(x => new LinkDto(
                x.DefinitionId == 11 ? x.CustomDefinitionName ?? "Custom" : x.Definition!.DefinitionName, 
                x.Value, 
                x.SortId))
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

        return new UserProfileDto(userId, allLinks, user.ProfilePhotoUrl);
    }

    [HttpPost("{userId}/photo")]
    public async Task<IActionResult> UploadProfilePhoto(string userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya seçilmedi.");

        // Sadece resim dosyalarına izin ver
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest("Sadece JPEG, PNG ve GIF dosyalarına izin verilir.");

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
    public async Task<IActionResult> DeleteProfilePhoto(string userId)
    {
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

        var customDefinitionNames = await _ctx.UserDefinitionValues
            .Where(x => x.UserId == userId && x.DefinitionId == 11 && !string.IsNullOrEmpty(x.CustomDefinitionName))
            .Select(x => x.CustomDefinitionName!)
            .Distinct()
            .ToListAsync();

        return Ok(customDefinitionNames);
    }
}

using diji_card_alt.Data;
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

    // ‼️ Kendi projende aşağıdaki ID’leri Definitions tablosuna bakarak ayarla
    private const int DefFullName = 1;
    private const int DefCompany = 2;
    private const int DefEmail = 3;
    private const int DefPhone = 4;

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserProfileDto>> GetProfile(string userId)
    {
        // 1) Kullanıcının sabit alanları (Users tablosu)
        var user = await _ctx.Users.FindAsync(userId);
        if (user is null) return NotFound();

        // 2) Dinamik linkler (UserDefinitionValues) + DefinitionName
        var linksFromUdvs = await _ctx.UserDefinitionValues
            .Where(x => x.UserId == userId)
            .Include(x => x.Definition)
            .Select(x => new LinkDto(x.Definition.DefinitionName, x.Value))
            .ToListAsync();

        // 3) Sabit alanları DefinitionName’leriyle birlikte DTO’ya ekle
        var defaultLinks = new List<LinkDto>
        {
            new("Full Name", user.FullName  ?? string.Empty),
            new("Company"  , user.Company   ?? string.Empty),
            new("E-mail"   , user.Email     ?? string.Empty),
            new("Phone"    , user.PhoneNumber ?? string.Empty)
        };

        // 4) Hepsini birleştir
        var allLinks = defaultLinks.Concat(linksFromUdvs).ToList();

        return new UserProfileDto(userId, allLinks);
    }
}

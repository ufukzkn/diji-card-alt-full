using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using diji_card_alt.Data;
using diji_card_alt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace diji_card_alt_full.Controllers;

[ApiController]
[Route("api/profile-visits")] 
public class ProfileVisitController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public ProfileVisitController(AppDbContext ctx) => _ctx = ctx;

    public class TrackVisitRequest
    {
        public string? ProfileUserId { get; set; }
    public bool? IsSpecialAccess { get; set; }
    }

    private string? GetTokenUserId()
    {
        // Sadece doğrulanmış ClaimsPrincipal üzerinden al; manuel JWT parse yok
        if (User?.Identity?.IsAuthenticated != true) return null;
        return User.FindFirst("userId")?.Value
            ?? User.FindFirst("uid")?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("nameid")?.Value;
    }

    private static string? Hash(string? input)
    {
        if (string.IsNullOrEmpty(input)) return null;
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    [HttpPost]
    public async Task<IActionResult> Track([FromBody] TrackVisitRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProfileUserId))
            return BadRequest(new { success = false, message = "ProfileUserId gereklidir" });

        // Kullanıcı var mı kontrol et (yoksa 404 dön)
        var profileUserExists = await _ctx.Users.AnyAsync(u => u.UserId == request.ProfileUserId);
        if (!profileUserExists)
            return NotFound(new { success = false, message = "Profil bulunamadı" });

        var visitorUserId = GetTokenUserId(); // anonim olabilir
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].FirstOrDefault();

        var visit = new ProfileVisit
        {
            ProfileUserId = request.ProfileUserId!,
            VisitorUserId = visitorUserId, // anonim ise null kalır
            VisitedAtUtc = DateTime.UtcNow,
            VisitorIpHash = Hash(remoteIp),
            UserAgentHash = Hash(userAgent),
            IsSpecialAccess = request.IsSpecialAccess ?? false
        };

        _ctx.ProfileVisits.Add(visit);
        await _ctx.SaveChangesAsync();

        return Ok(new { success = true, visitId = visit.Id });
    }

    // Özet: toplam, son 7 gün, bugün, özel link oranı, unique (IP hash + UA hash kombinasyonu) yaklaşık
    [HttpGet("{userId}/summary")]
    [Authorize]
    public async Task<IActionResult> GetSummary(string userId)
    {
        var caller = GetTokenUserId();
        if (caller != userId) return Forbid();
        var now = DateTime.UtcNow;
        var sevenDays = now.AddDays(-7);
        var todayStart = new DateTime(now.Year, now.Month, now.Day, 0,0,0, DateTimeKind.Utc);

        var query = _ctx.ProfileVisits.Where(v => v.ProfileUserId == userId);
        var total = await query.CountAsync();
        var last7 = await query.Where(v => v.VisitedAtUtc >= sevenDays).CountAsync();
        var today = await query.Where(v => v.VisitedAtUtc >= todayStart).CountAsync();
        var special = await query.Where(v => v.IsSpecialAccess).CountAsync();
        var uniqueFingerprints = await query
            .Select(v => new { v.VisitorIpHash, v.UserAgentHash })
            .Distinct()
            .CountAsync();

        return Ok(new { total, last7, today, special, specialRatio = total==0?0: (double)special/total, unique = uniqueFingerprints });
    }

    // Günlük dağılım (varsayılan 7 gün)
    [HttpGet("{userId}/daily")]
    [Authorize]
    public async Task<IActionResult> GetDaily(string userId, [FromQuery] int days = 7)
    {
        var caller = GetTokenUserId();
        if (caller != userId) return Forbid();
        if (days < 1) days = 1; if (days > 60) days = 60;
        var start = DateTime.UtcNow.Date.AddDays(-days + 1);
        var data = await _ctx.ProfileVisits
            .Where(v => v.ProfileUserId == userId && v.VisitedAtUtc >= start)
            .GroupBy(v => v.VisitedAtUtc.Date)
            .Select(g => new { date = g.Key, count = g.Count() })
            .OrderBy(x => x.date)
            .ToListAsync();
        // Boş günleri doldur
        var map = data.ToDictionary(x => x.date, x => x.count);
        var list = new List<object>();
        for (int i=0;i<days;i++)
        {
            var d = start.AddDays(i).Date;
            map.TryGetValue(d, out var c);
            list.Add(new { date = d, count = c });
        }
        return Ok(list);
    }

    // Özel link vs normal (son N gün)
    [HttpGet("{userId}/special-vs-normal")]
    [Authorize]
    public async Task<IActionResult> GetSpecialVsNormal(string userId, [FromQuery] int days = 7)
    {
        var caller = GetTokenUserId();
        if (caller != userId) return Forbid();
        if (days < 1) days = 1; if (days > 60) days = 60;
        var start = DateTime.UtcNow.AddDays(-days);
        var query = _ctx.ProfileVisits.Where(v => v.ProfileUserId == userId && v.VisitedAtUtc >= start);
        var special = await query.Where(v => v.IsSpecialAccess).CountAsync();
        var normal = await query.Where(v => !v.IsSpecialAccess).CountAsync();
        return Ok(new { days, special, normal });
    }
}

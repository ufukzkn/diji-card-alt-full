using Microsoft.AspNetCore.Mvc;

namespace diji_card_alt_full.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "OK", timeUtc = DateTime.UtcNow });
}

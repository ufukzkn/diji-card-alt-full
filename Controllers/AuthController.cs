using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using digital_business_card.Models;
using diji_card_alt.Data;
using diji_card_alt.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Collections.Concurrent;

namespace digital_business_card.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AppDbContext _context;
    private static readonly ConcurrentDictionary<string, string> _activeTokens = new(); // RequestId -> UserId
    private static readonly ConcurrentDictionary<string, DateTime> _tokenExpiryTimes = new(); // AuthToken -> Expiry time
    private static readonly ConcurrentDictionary<string, (string UserId, DateTime ExpiresAt)> _refreshTokens = new(); // refreshToken -> data
        private readonly IConfiguration _config;

        public AuthController(ILogger<AuthController> logger, AppDbContext context, IConfiguration config)
        {
            _logger = logger;
            _context = context;
            _config = config;
        }

    [HttpPost("KullaniciGirisYap")]
    [EnableRateLimiting("Login")]
        public async Task<IActionResult> KullaniciGirisYap([FromBody] LoginRequest request)
        {
            try
            {
                // Normalize incoming credentials (trim whitespace / trailing punctuation accidentally entered)
                request.KullaniciAdi = (request.KullaniciAdi ?? string.Empty).Trim().TrimEnd(',');
                request.Sifre = (request.Sifre ?? string.Empty).Trim();

                _logger.LogInformation("Login attempt for user: {KullaniciAdi}", request.KullaniciAdi);

                // Basit doğrulama
                if (string.IsNullOrEmpty(request.KullaniciAdi) || string.IsNullOrEmpty(request.Sifre))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Kullanıcı adı ve şifre boş olamaz."
                    });
                }

                // Database'den kullanıcıyı kontrol et (Email veya UserId ile)
                var userNameOrEmail = request.KullaniciAdi;
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == userNameOrEmail || u.UserId == userNameOrEmail);

                if (user != null && user.Password == request.Sifre)
                {
                    var requestId = !string.IsNullOrEmpty(request.RequestId)
                        ? request.RequestId
                        : Guid.NewGuid().ToString();

                    var authToken = Guid.NewGuid().ToString();
                    _activeTokens[requestId] = user.UserId;
                    _tokenExpiryTimes[authToken] = DateTime.UtcNow.AddMinutes(10);

                    // DOĞRUDAN JWT üret (frontend artık ikinci adım olmadan kullanabilir)
                    var accessJwt = await GenerateJwtTokenAsync(user.UserId);

                    _logger.LogInformation("Successful login for user: {UserId}", user.UserId);

                    return Ok(new LoginResponse
                    {
                        Success = true,
                        Message = "Giriş başarılı",
                        RequestId = requestId,
                        AuthToken = authToken, // legacy flow (OAuthToken endpoint'i isteyenler için)
                        AccessToken = accessJwt, // yeni direkt JWT
                        RedirectUrl = $"/profil/{user.UserId}"
                    });
                }

                _logger.LogWarning("Failed login attempt for user: {KullaniciAdi}", request.KullaniciAdi);
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Kullanıcı adı veya şifre hatalı."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for user: {KullaniciAdi}", request.KullaniciAdi);
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Sunucu hatası oluştu."
                });
            }
        }

    [HttpPost("OAuthToken")]
    [EnableRateLimiting("TokenExchange")]
        public async Task<IActionResult> OAuthToken([FromBody] OAuthTokenRequest request)
        {
            try
            {
                _logger.LogInformation("OAuth token request for RequestId: {RequestId}", request.RequestId);

                if (string.IsNullOrEmpty(request.RequestId) || string.IsNullOrEmpty(request.AuthToken))
                {
                    return BadRequest(new TokenResponse
                    {
                        Success = false,
                        Message = "RequestId ve AuthToken gereklidir."
                    });
                }

                // AuthToken'ın geçerli GUID formatında olup olmadığını kontrol et
                if (Guid.TryParse(request.AuthToken, out _) && _activeTokens.ContainsKey(request.RequestId))
                {
                    // Token'ın süresinin dolup dolmadığını kontrol et
                    if (_tokenExpiryTimes.ContainsKey(request.AuthToken) && 
                        _tokenExpiryTimes[request.AuthToken] > DateTime.UtcNow)
                    {
                        var userId = _activeTokens[request.RequestId];
                        
                        var accessToken = await GenerateJwtTokenAsync(userId);
                        var refreshToken = Guid.NewGuid().ToString();
                        var refreshExpiry = DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:RefreshTokenDays", 7));
                        _refreshTokens[refreshToken] = (userId, refreshExpiry);

                        return Ok(new TokenResponse
                        {
                            Success = true,
                            Message = "Token başarıyla oluşturuldu",
                            AccessToken = accessToken,
                            RefreshToken = refreshToken,
                            ExpiresAt = _tokenExpiryTimes[request.AuthToken], // 10 dakikalık süre (legacy for now)
                            TokenType = "Bearer"
                        });
                    }
                    else
                    {
                        // Token süresi dolmuş, temizle
                        _activeTokens.TryRemove(request.RequestId, out _);
                        _tokenExpiryTimes.TryRemove(request.AuthToken, out _);
                        
                        return Unauthorized(new TokenResponse
                        {
                            Success = false,
                            Message = "Token süresi dolmuş. Lütfen tekrar giriş yapın."
                        });
                    }
                }

                return Unauthorized(new TokenResponse
                {
                    Success = false,
                    Message = "Geçersiz auth token."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OAuth token error for RequestId: {RequestId}", request.RequestId);
                return StatusCode(500, new TokenResponse
                {
                    Success = false,
                    Message = "Sunucu hatası oluştu."
                });
            }
        }

        private async Task<string> GenerateJwtTokenAsync(string userId)
        {
            await Task.Delay(1); // async signature
            var key = _config["Jwt:Key"] ?? "dev-secret-key-change-me-32chars";
            var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(10); // 10 dakika
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim("uid", userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken(
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("Refresh")] // body: { refreshToken }
        [EnableRateLimiting("TokenExchange")]
        public IActionResult Refresh([FromBody] dynamic body)
        {
            string? refreshToken = body?.refreshToken;
            if (string.IsNullOrEmpty(refreshToken) || !_refreshTokens.ContainsKey(refreshToken))
            {
                return Unauthorized(new { Success = false, Message = "Geçersiz refresh token" });
            }
            var (userId, expiresAt) = _refreshTokens[refreshToken];
            if (expiresAt <= DateTime.UtcNow)
            {
                _refreshTokens.TryRemove(refreshToken, out _);
                return Unauthorized(new { Success = false, Message = "Refresh token süresi dolmuş" });
            }
            var accessTask = GenerateJwtTokenAsync(userId);
            var newRefreshToken = Guid.NewGuid().ToString();
            var newRefreshExpiry = DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:RefreshTokenDays", 7));
            _refreshTokens[newRefreshToken] = (userId, newRefreshExpiry);
            // eski refresh token'ı isteğe bağlı sil
            _refreshTokens.TryRemove(refreshToken, out _);
            return Ok(new {
                Success = true,
                AccessToken = accessTask.Result,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10) // 10 dakika
            });
        }

        [HttpPost("ValidateToken")]
        public IActionResult ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.AccessToken))
                {
                    return BadRequest(new { Success = false, Message = "Access token gereklidir." });
                }

                var handler = new JwtSecurityTokenHandler();
                try
                {
                    var key = _config["Jwt:Key"] ?? "dev-secret-key-change-me-32chars";
                    var principal = handler.ValidateToken(request.AccessToken, new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ClockSkew = TimeSpan.Zero
                    }, out var validatedToken);

                    var userId = principal.Claims.FirstOrDefault(c => c.Type == "uid" || c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                    if (userId == null)
                    {
                        _logger.LogInformation("Token userId claim missing");
                        return Unauthorized(new { Success = false, Message = "Geçersiz token" });
                    }
                    var jwt = (JwtSecurityToken)validatedToken;
                    var expiryDate = jwt.ValidTo;
                    if (expiryDate <= DateTime.UtcNow)
                        return Unauthorized(new { Success = false, Message = "Oturumunuz zaman aşımına uğradı" });

                    bool canEdit = request.RequestedUserId == userId;
                    return Ok(new {
                        Success = true,
                        Message = "Token geçerli",
                        UserId = userId,
                        ExpiresAt = expiryDate,
                        CanEdit = canEdit
                    });
                }
                catch (SecurityTokenExpiredException)
                {
                    return Unauthorized(new { Success = false, Message = "Oturumunuz zaman aşımına uğradı" });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Token validation failed");
                    return Unauthorized(new { Success = false, Message = "Geçersiz token" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation error");
                return StatusCode(500, new { Success = false, Message = "Sunucu hatası" });
            }
        }
    }
}

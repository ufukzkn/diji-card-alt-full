using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using digital_business_card.Models;
using diji_card_alt.Data;
using diji_card_alt.Models;

namespace digital_business_card.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AppDbContext _context;
        private static readonly Dictionary<string, string> _activeTokens = new(); // RequestId -> UserId mapping
        private static readonly Dictionary<string, DateTime> _tokenExpiryTimes = new(); // AuthToken -> Expiry time mapping

        public AuthController(ILogger<AuthController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost("KullaniciGirisYap")]
        public async Task<IActionResult> KullaniciGirisYap([FromBody] LoginRequest request)
        {
            try
            {
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
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.KullaniciAdi || u.UserId == request.KullaniciAdi);

                if (user != null && user.Password == request.Sifre)
                {
                    var requestId = !string.IsNullOrEmpty(request.RequestId) 
                        ? request.RequestId 
                        : Guid.NewGuid().ToString();

                    var authToken = Guid.NewGuid().ToString();

                    // RequestId ile UserId'yi eşleştir
                    _activeTokens[requestId] = user.UserId;
                    
                    // AuthToken'ın 10 dakikalık timeout süresini ayarla
                    _tokenExpiryTimes[authToken] = DateTime.UtcNow.AddMinutes(10);

                    _logger.LogInformation("Successful login for user: {UserId}", user.UserId);

                    return Ok(new LoginResponse
                    {
                        Success = true,
                        Message = "Giriş başarılı",
                        RequestId = requestId,
                        AuthToken = authToken,
                        RedirectUrl = $"/profil/{user.UserId}" // Kullanıcının kendi profiline yönlendir
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
                        
                        return Ok(new TokenResponse
                        {
                            Success = true,
                            Message = "Token başarıyla oluşturuldu",
                            AccessToken = await GenerateJwtTokenAsync(userId),
                            RefreshToken = Guid.NewGuid().ToString(),
                            ExpiresAt = _tokenExpiryTimes[request.AuthToken], // 10 dakikalık süre
                            TokenType = "Bearer"
                        });
                    }
                    else
                    {
                        // Token süresi dolmuş, temizle
                        _activeTokens.Remove(request.RequestId);
                        _tokenExpiryTimes.Remove(request.AuthToken);
                        
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
            // Basit token oluşturma (gerçek projede JWT kütüphanesi kullanılır)
            await Task.Delay(1); // async method yapmak için
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                $"{userId}:expires:{DateTime.UtcNow.AddMinutes(10):yyyy-MM-dd HH:mm:ss}:timestamp:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
            ));
            return token;
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

                // Token'ı decode et
                try
                {
                    var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(request.AccessToken));
                    var parts = decoded.Split(':');

                    if (parts.Length >= 4)
                    {
                        var userId = parts[0];
                        var expiryDateStr = parts[2];
                        
                        if (DateTime.TryParseExact(expiryDateStr, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out var expiryDate))
                        {
                            if (expiryDate > DateTime.UtcNow)
                            {
                                // Token geçerli
                                return Ok(new { 
                                    Success = true, 
                                    Message = "Token geçerli",
                                    UserId = userId,
                                    ExpiresAt = expiryDate,
                                    CanEdit = request.RequestedUserId == userId // Sadece kendi profilini düzenleyebilir
                                });
                            }
                            else
                            {
                                return Unauthorized(new { Success = false, Message = "Token süresi dolmuş" });
                            }
                        }
                    }
                }
                catch
                {
                    return Unauthorized(new { Success = false, Message = "Geçersiz token formatı" });
                }

                return Unauthorized(new { Success = false, Message = "Geçersiz token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation error");
                return StatusCode(500, new { Success = false, Message = "Sunucu hatası" });
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SignalRChatApp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(UserService userService, JwtSettings jwtSettings)
    {
        _userService = userService;
        _jwtSettings = jwtSettings;
    }

    // =================== REGISTER ===================
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = await _userService.Register(
            request.Username,
            request.Email,
            request.Password
        );

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            Token = token
        });
    }

    // =================== LOGIN ===================
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userService.Login(
            request.Email,
            request.Password
        );

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            Token = token
        });
    }

    // =================== JWT GENERATOR ===================
    private string GenerateJwtToken(ChatUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

using HorsesForCourses.Domain.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HorsesForCourses.Api.Accounts;

[ApiController]
[Route("auth")]
public class AccountsController(IConfiguration cfg) : ControllerBase
{
    [HttpPost("token")]
    public async Task<IActionResult> Token(LoginRequest dto)
    {
        // var user = await _users.ValidateAsync(dto.Email, dto.Password);
        // if (user is null) return Unauthorized();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user.Name"),
            new Claim(ClaimTypes.Name, "user.Email"),
            new Claim(ClaimTypes.Role, ApplicationUser.AdminRole),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Auth:JwtKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: cfg["Auth:Issuer"],
            audience: cfg["Auth:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return Ok(new { access_token = token, token_type = "Bearer", expires_in = 3600 });
    }

    [Authorize]
    [HttpGet("/secret")]
    public IActionResult Secret() => Ok("shh");
}

public record LoginRequest(string Email, string Password);
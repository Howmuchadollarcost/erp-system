using Erp.Api.Auth;
using Erp.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Erp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtSettings _jwtSettings;

    public AuthController(AppDbContext db, IOptions<JwtSettings> jwtOptions)
    {
        _db = db;
        _jwtSettings = jwtOptions.Value;
    }

    public record LoginRequest(string Username, string Password);
    public record LoginResponse(string Token, string Role, int? Rank, Guid UserId, string Username);

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var token = JwtTokenService.CreateToken(user.Id.ToString(), user.Username, user.Role.ToString(), user.SupervisorRank, _jwtSettings);
        return new LoginResponse(token, user.Role.ToString(), user.SupervisorRank, user.Id, user.Username);
    }
}
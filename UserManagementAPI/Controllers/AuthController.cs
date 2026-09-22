using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.TokenServices;


namespace UserManagementAPI.AuthControllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _repository;
    private readonly TokenService _tokenService;

    public AuthController(IUserRepository repository, TokenService tokenService)
    {
        _repository = repository;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
    {
        var user = await _repository.GetUserByMailAsync(request.Mail);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {   
            return Unauthorized(new { Error = "Credenciales inválidas" });
        }
        var token = _tokenService.GenerateToken(user);
        return Ok(new { Token = token });
    }
}

public record LoginRequest(string Mail, string Password);
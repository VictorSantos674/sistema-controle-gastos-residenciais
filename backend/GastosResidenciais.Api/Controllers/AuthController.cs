using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GastosResidenciais.Api.Controllers;

/// <summary>
/// Controller de autenticação — endpoints públicos de login, registro, refresh e logout.
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    /// <summary>
    /// POST /api/auth/registrar — Cria uma nova conta, retorna access token e grava refresh token em cookie HttpOnly.
    /// </summary>
    [HttpPost("registrar")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarDto dto)
    {
        var (resultado, erro) = await _service.RegistrarAsync(dto);
        if (erro is not null) return BadRequest(new { mensagem = erro });

        EmitirRefreshToken(resultado!.RefreshToken);
        return StatusCode(201, ToTokenDto(resultado));
    }

    /// <summary>
    /// POST /api/auth/login — Autentica um usuário, retorna access token e grava refresh token em cookie HttpOnly.
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var (resultado, erro) = await _service.LoginAsync(dto);
        if (erro is not null) return Unauthorized(new { mensagem = erro });

        EmitirRefreshToken(resultado!.RefreshToken);
        return Ok(ToTokenDto(resultado));
    }

    /// <summary>
    /// POST /api/auth/refresh — Rotaciona o refresh token e retorna um novo access token.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        var (resultado, erro) = await _service.RefreshAsync(refreshToken);
        if (erro is not null) return Unauthorized(new { mensagem = erro });

        EmitirRefreshToken(resultado!.RefreshToken);
        return Ok(ToTokenDto(resultado));
    }

    /// <summary>
    /// POST /api/auth/logout — Invalida o refresh token atual e expira o cookie.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        await _service.LogoutAsync(refreshToken);
        ExpirarRefreshToken();
        return NoContent();
    }

    private void EmitirRefreshToken(string refreshToken)
    {
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth",
            MaxAge = RefreshTokenLifetime
        });
    }

    private void ExpirarRefreshToken()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth"
        });
    }

    private static TokenDto ToTokenDto(AuthResultDto resultado) =>
        new()
        {
            Token = resultado.Token,
            Login = resultado.Login
        };
}

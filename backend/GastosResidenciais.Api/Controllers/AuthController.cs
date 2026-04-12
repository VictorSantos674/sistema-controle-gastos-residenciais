using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GastosResidenciais.Api.Controllers;

/// <summary>
/// Controller de autenticação — endpoints públicos (sem necessidade de token).
///
/// <c>[AllowAnonymous]</c> sobrepõe a política global de <c>[Authorize]</c>
/// configurada no Program.cs, permitindo que qualquer cliente acesse
/// /api/auth/registrar e /api/auth/login sem um JWT válido.
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    /// <summary>
    /// POST /api/auth/registrar — Cria uma nova conta.
    /// Retorna 201 com o JWT em caso de sucesso ou 400 se o login já existir.
    /// </summary>
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarDto dto)
    {
        var (resultado, erro) = await _service.RegistrarAsync(dto);
        if (erro is not null) return BadRequest(new { mensagem = erro });
        return StatusCode(201, resultado);
    }

    /// <summary>
    /// POST /api/auth/login — Autentica um usuário existente.
    /// Retorna 200 com o JWT ou 401 se as credenciais forem inválidas.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var (resultado, erro) = await _service.LoginAsync(dto);
        if (erro is not null) return Unauthorized(new { mensagem = erro });
        return Ok(resultado);
    }
}

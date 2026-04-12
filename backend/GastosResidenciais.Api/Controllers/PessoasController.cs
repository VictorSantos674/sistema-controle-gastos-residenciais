using System.Security.Claims;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GastosResidenciais.Api.Controllers;

/// <summary>
/// Controller responsável pelo CRUD completo de Pessoas.
/// Todas as operações são filtradas pelo usuário autenticado via JWT.
/// Rota base: <c>/api/pessoas</c>
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PessoasController : ControllerBase
{
    private readonly IPessoaService _service;

    public PessoasController(IPessoaService service)
    {
        _service = service;
    }

    /// <summary>Extrai o ID do usuário autenticado a partir do claim JWT.</summary>
    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var resultado = await _service.ListarAsync(GetUserId());
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var resultado = await _service.ObterPorIdAsync(id, GetUserId());
        if (resultado is null) return NotFound();
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] PessoaInputDto dto)
    {
        var resultado = await _service.CriarAsync(dto, GetUserId());
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, [FromBody] PessoaInputDto dto)
    {
        var resultado = await _service.EditarAsync(id, dto, GetUserId());
        if (resultado is null) return NotFound();
        return Ok(resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deletar(int id)
    {
        var deletado = await _service.DeletarAsync(id, GetUserId());
        if (!deletado) return NotFound();
        return NoContent();
    }
}

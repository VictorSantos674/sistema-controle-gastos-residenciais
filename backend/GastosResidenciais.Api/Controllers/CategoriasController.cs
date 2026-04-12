using System.Security.Claims;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GastosResidenciais.Api.Controllers;

/// <summary>
/// Controller responsável pelo gerenciamento de Categorias.
/// Todas as operações são filtradas pelo usuário autenticado via JWT.
/// Rota base: <c>/api/categorias</c>
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _service;

    public CategoriasController(ICategoriaService service)
    {
        _service = service;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var resultado = await _service.ListarAsync(GetUserId());
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CategoriaInputDto dto)
    {
        var resultado = await _service.CriarAsync(dto, GetUserId());
        return StatusCode(201, resultado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id)
    {
        var erro = await _service.DeletarAsync(id, GetUserId());
        if (erro is not null) return BadRequest(new { mensagem = erro });
        return NoContent();
    }
}

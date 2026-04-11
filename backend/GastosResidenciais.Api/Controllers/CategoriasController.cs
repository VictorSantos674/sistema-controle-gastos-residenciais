using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GastosResidenciais.Api.Controllers;

/// <summary>
/// Controller responsável pelo gerenciamento de Categorias.
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

    /// <summary>GET /api/categorias — Lista todas as categorias ordenadas por descrição.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var resultado = await _service.ListarAsync();
        return Ok(resultado);
    }

    /// <summary>POST /api/categorias — Cria uma nova categoria.</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CategoriaInputDto dto)
    {
        var resultado = await _service.CriarAsync(dto);
        return StatusCode(201, resultado);
    }

    /// <summary>
    /// DELETE /api/categorias/{id} — Deleta uma categoria.
    ///
    /// Retorna <c>400 Bad Request</c> se:
    /// <list type="bullet">
    ///   <item>A categoria possuir transações vinculadas.</item>
    ///   <item>A categoria não existir (mensagem descritiva no corpo).</item>
    /// </list>
    ///
    /// Nota: usamos 400 em vez de 404 para o caso "não existe" porque o serviço
    /// retorna sempre uma string de erro — o controller não distingue os casos,
    /// preferindo manter a implementação simples.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id)
    {
        var erro = await _service.DeletarAsync(id);
        if (erro is not null) return BadRequest(new { mensagem = erro });
        return NoContent();
    }
}

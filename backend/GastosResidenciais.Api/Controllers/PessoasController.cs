using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GastosResidenciais.Api.Controllers;

/// <summary>
/// Controller responsável pelo CRUD completo de Pessoas.
///
/// <b>Padrão "Controller magro":</b> toda lógica de negócio está no <see cref="IPessoaService"/>.
/// O controller só recebe a requisição, delega ao serviço e converte o resultado
/// no status HTTP semântico correto:
/// <list type="bullet">
///   <item><c>200 OK</c>       — leitura bem-sucedida.</item>
///   <item><c>201 Created</c>  — criação bem-sucedida (com header <c>Location</c>).</item>
///   <item><c>204 No Content</c> — deleção bem-sucedida.</item>
///   <item><c>404 Not Found</c>  — ID não encontrado.</item>
/// </list>
///
/// Rota base: <c>/api/pessoas</c> (convenção: [controller] → "Pessoas" → "pessoas")
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PessoasController : ControllerBase
{
    private readonly IPessoaService _service;

    /// <summary><see cref="IPessoaService"/> injetado pelo contêiner DI.</summary>
    public PessoasController(IPessoaService service)
    {
        _service = service;
    }

    /// <summary>GET /api/pessoas — Lista todas as pessoas ordenadas por nome.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var resultado = await _service.ListarAsync();
        return Ok(resultado);
    }

    /// <summary>GET /api/pessoas/{id} — Obtém uma pessoa pelo ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var resultado = await _service.ObterPorIdAsync(id);
        if (resultado is null) return NotFound();
        return Ok(resultado);
    }

    /// <summary>
    /// POST /api/pessoas — Cria uma nova pessoa.
    /// Retorna <c>201 Created</c> com o header <c>Location: /api/pessoas/{id}</c>
    /// apontando para o recurso criado — padrão REST para criação.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] PessoaInputDto dto)
    {
        var resultado = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    /// <summary>PUT /api/pessoas/{id} — Edita uma pessoa existente.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, [FromBody] PessoaInputDto dto)
    {
        var resultado = await _service.EditarAsync(id, dto);
        if (resultado is null) return NotFound();
        return Ok(resultado);
    }

    /// <summary>
    /// DELETE /api/pessoas/{id} — Deleta a pessoa e todas as suas transações em cascata.
    /// A exclusão em cascata é garantida pelo <c>DeleteBehavior.Cascade</c>
    /// configurado no <c>AppDbContext</c>, não por lógica de aplicação.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deletar(int id)
    {
        var deletado = await _service.DeletarAsync(id);
        if (!deletado) return NotFound();
        return NoContent();
    }
}

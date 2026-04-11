using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GastosResidenciais.Api.Controllers;

/// <summary>
/// Controller responsável pelo gerenciamento de Transações financeiras.
/// Rota base: <c>/api/transacoes</c>
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TransacoesController : ControllerBase
{
    private readonly ITransacaoService _service;

    public TransacoesController(ITransacaoService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET /api/transacoes — Lista todas as transações com nome da pessoa
    /// e descrição da categoria desnormalizados para exibição.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var resultado = await _service.ListarAsync();
        return Ok(resultado);
    }

    /// <summary>
    /// POST /api/transacoes — Cria uma nova transação com validação de regras de negócio.
    ///
    /// Retorna <c>400 Bad Request</c> com campo <c>mensagem</c> se alguma regra for violada
    /// (menor de idade, incompatibilidade de categoria, valor inválido).
    ///
    /// O service usa o padrão Result Tuple para evitar exceções como controle de fluxo.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] TransacaoInputDto dto)
    {
        var (resultado, erro) = await _service.CriarAsync(dto);
        if (erro is not null) return BadRequest(new { mensagem = erro });
        return StatusCode(201, resultado);
    }

    /// <summary>DELETE /api/transacoes/{id} — Deleta uma transação pelo ID.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id)
    {
        var erro = await _service.DeletarAsync(id);
        if (erro is not null) return NotFound(new { mensagem = erro });
        return NoContent();
    }
}

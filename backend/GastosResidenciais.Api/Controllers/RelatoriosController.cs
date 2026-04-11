using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GastosResidenciais.Api.Controllers;

/// <summary>
/// Controller responsável pelos relatórios financeiros consolidados.
/// Rota base: <c>/api/relatorios</c>
///
/// Todos os endpoints são somente leitura (GET) — relatórios não criam nem alteram dados.
/// Suportam filtro opcional por mês e/ou ano via query string.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioService _service;

    public RelatoriosController(IRelatorioService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET /api/relatorios/por-pessoa?mes={1-12}&amp;ano={yyyy}
    ///
    /// Retorna totais de receitas, despesas e saldo por pessoa,
    /// mais os totais gerais consolidados ao final.
    ///
    /// Parâmetros de filtro são opcionais — omiti-los retorna todos os períodos.
    /// </summary>
    /// <param name="mes">Mês de filtro (1–12). Omitir = todos os meses.</param>
    /// <param name="ano">Ano de filtro (ex.: 2025). Omitir = todos os anos.</param>
    [HttpGet("por-pessoa")]
    public async Task<IActionResult> PorPessoa([FromQuery] int? mes, [FromQuery] int? ano)
    {
        var resultado = await _service.ObterTotaisPorPessoaAsync(mes, ano);
        return Ok(resultado);
    }

    /// <summary>
    /// GET /api/relatorios/por-categoria?mes={1-12}&amp;ano={yyyy}
    ///
    /// Retorna totais de receitas, despesas e saldo por categoria,
    /// mais os totais gerais consolidados ao final.
    /// </summary>
    /// <param name="mes">Mês de filtro (1–12). Omitir = todos os meses.</param>
    /// <param name="ano">Ano de filtro (ex.: 2025). Omitir = todos os anos.</param>
    [HttpGet("por-categoria")]
    public async Task<IActionResult> PorCategoria([FromQuery] int? mes, [FromQuery] int? ano)
    {
        var resultado = await _service.ObterTotaisPorCategoriaAsync(mes, ano);
        return Ok(resultado);
    }
}

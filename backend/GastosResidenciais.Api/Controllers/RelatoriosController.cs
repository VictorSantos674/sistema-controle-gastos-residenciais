using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GastosResidenciais.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioService _service;

    public RelatoriosController(IRelatorioService service)
    {
        _service = service;
    }

    /// <param name="mes">Filtro opcional de mês (1–12).</param>
    /// <param name="ano">Filtro opcional de ano (ex.: 2025).</param>
    [HttpGet("por-pessoa")]
    public async Task<IActionResult> PorPessoa([FromQuery] int? mes, [FromQuery] int? ano)
    {
        var resultado = await _service.ObterTotaisPorPessoaAsync(mes, ano);
        return Ok(resultado);
    }

    /// <param name="mes">Filtro opcional de mês (1–12).</param>
    /// <param name="ano">Filtro opcional de ano (ex.: 2025).</param>
    [HttpGet("por-categoria")]
    public async Task<IActionResult> PorCategoria([FromQuery] int? mes, [FromQuery] int? ano)
    {
        var resultado = await _service.ObterTotaisPorCategoriaAsync(mes, ano);
        return Ok(resultado);
    }
}

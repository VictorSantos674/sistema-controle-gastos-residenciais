using System.Security.Claims;
using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GastosResidenciais.Api.Controllers;

/// <summary>
/// Controller responsável pelos relatórios financeiros consolidados.
/// Todos os relatórios são filtrados pelo usuário autenticado via JWT.
/// Rota base: <c>/api/relatorios</c>
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

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("por-pessoa")]
    public async Task<IActionResult> PorPessoa([FromQuery] int? mes, [FromQuery] int? ano)
    {
        var resultado = await _service.ObterTotaisPorPessoaAsync(GetUserId(), mes, ano);
        return Ok(resultado);
    }

    [HttpGet("por-categoria")]
    public async Task<IActionResult> PorCategoria([FromQuery] int? mes, [FromQuery] int? ano)
    {
        var resultado = await _service.ObterTotaisPorCategoriaAsync(GetUserId(), mes, ano);
        return Ok(resultado);
    }
}

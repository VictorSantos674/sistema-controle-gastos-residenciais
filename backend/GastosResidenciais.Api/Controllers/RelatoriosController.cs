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

    [HttpGet("por-pessoa")]
    public async Task<IActionResult> PorPessoa()
    {
        var resultado = await _service.ObterTotaisPorPessoaAsync();
        return Ok(resultado);
    }

    [HttpGet("por-categoria")]
    public async Task<IActionResult> PorCategoria()
    {
        var resultado = await _service.ObterTotaisPorCategoriaAsync();
        return Ok(resultado);
    }
}

using System.Security.Claims;
using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GastosResidenciais.Api.Controllers;

/// <summary>
/// Controller responsável por dados consolidados do Dashboard.
/// Rota base: <c>/api/dashboard</c>
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// GET /api/dashboard/resumo — Retorna totais financeiros e contadores em uma chamada.
    /// </summary>
    [HttpGet("resumo")]
    public async Task<IActionResult> Resumo()
    {
        var resultado = await _service.ObterResumoAsync(GetUserId());
        return Ok(resultado);
    }
}

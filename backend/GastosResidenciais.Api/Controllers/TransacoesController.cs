using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GastosResidenciais.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransacoesController : ControllerBase
{
    private readonly ITransacaoService _service;

    public TransacoesController(ITransacaoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var resultado = await _service.ListarAsync();
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] TransacaoInputDto dto)
    {
        var (resultado, erro) = await _service.CriarAsync(dto);
        if (erro is not null) return BadRequest(new { mensagem = erro });
        return StatusCode(201, resultado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id)
    {
        var erro = await _service.DeletarAsync(id);
        if (erro is not null) return NotFound(new { mensagem = erro });
        return NoContent();
    }
}

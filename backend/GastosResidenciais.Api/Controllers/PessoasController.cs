using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GastosResidenciais.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PessoasController : ControllerBase
{
    private readonly IPessoaService _service;

    public PessoasController(IPessoaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var resultado = await _service.ListarAsync();
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var resultado = await _service.ObterPorIdAsync(id);
        if (resultado is null) return NotFound();
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] PessoaInputDto dto)
    {
        var resultado = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, [FromBody] PessoaInputDto dto)
    {
        var resultado = await _service.EditarAsync(id, dto);
        if (resultado is null) return NotFound();
        return Ok(resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deletar(int id)
    {
        var deletado = await _service.DeletarAsync(id);
        if (!deletado) return NotFound();
        return NoContent();
    }
}

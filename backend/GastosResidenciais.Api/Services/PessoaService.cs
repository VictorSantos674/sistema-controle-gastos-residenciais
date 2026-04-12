using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Implementação concreta das operações de CRUD para <see cref="Pessoa"/>.
/// Todas as queries são filtradas por <c>usuarioId</c> para garantir isolamento de dados.
/// </summary>
public class PessoaService : IPessoaService
{
    private readonly AppDbContext _context;

    public PessoaService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PessoaOutputDto>> ListarAsync(int usuarioId)
    {
        return await _context.Pessoas
            .Where(p => p.UsuarioId == usuarioId)
            .OrderBy(p => p.Nome)
            .Select(p => new PessoaOutputDto { Id = p.Id, Nome = p.Nome, Idade = p.Idade })
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<PessoaOutputDto?> ObterPorIdAsync(int id, int usuarioId)
    {
        var pessoa = await _context.Pessoas
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);
        if (pessoa is null) return null;

        return new PessoaOutputDto { Id = pessoa.Id, Nome = pessoa.Nome, Idade = pessoa.Idade };
    }

    /// <inheritdoc/>
    public async Task<PessoaOutputDto> CriarAsync(PessoaInputDto dto, int usuarioId)
    {
        var pessoa = new Pessoa { Nome = dto.Nome, Idade = dto.Idade, UsuarioId = usuarioId };
        _context.Pessoas.Add(pessoa);
        await _context.SaveChangesAsync();

        return new PessoaOutputDto { Id = pessoa.Id, Nome = pessoa.Nome, Idade = pessoa.Idade };
    }

    /// <inheritdoc/>
    public async Task<PessoaOutputDto?> EditarAsync(int id, PessoaInputDto dto, int usuarioId)
    {
        var pessoa = await _context.Pessoas
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);
        if (pessoa is null) return null;

        pessoa.Nome  = dto.Nome;
        pessoa.Idade = dto.Idade;
        await _context.SaveChangesAsync();

        return new PessoaOutputDto { Id = pessoa.Id, Nome = pessoa.Nome, Idade = pessoa.Idade };
    }

    /// <inheritdoc/>
    public async Task<bool> DeletarAsync(int id, int usuarioId)
    {
        var pessoa = await _context.Pessoas
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);
        if (pessoa is null) return false;

        _context.Pessoas.Remove(pessoa);
        await _context.SaveChangesAsync();
        return true;
    }
}

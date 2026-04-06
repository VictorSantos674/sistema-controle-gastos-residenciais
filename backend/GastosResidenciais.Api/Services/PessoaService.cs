using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Services;

public class PessoaService : IPessoaService
{
    private readonly AppDbContext _context;

    public PessoaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PessoaOutputDto>> ListarAsync()
    {
        return await _context.Pessoas
            .OrderBy(p => p.Nome)
            .Select(p => new PessoaOutputDto { Id = p.Id, Nome = p.Nome, Idade = p.Idade })
            .ToListAsync();
    }

    public async Task<PessoaOutputDto?> ObterPorIdAsync(int id)
    {
        var pessoa = await _context.Pessoas.FindAsync(id);
        if (pessoa is null) return null;
        return new PessoaOutputDto { Id = pessoa.Id, Nome = pessoa.Nome, Idade = pessoa.Idade };
    }

    public async Task<PessoaOutputDto> CriarAsync(PessoaInputDto dto)
    {
        var pessoa = new Pessoa { Nome = dto.Nome, Idade = dto.Idade };
        _context.Pessoas.Add(pessoa);
        await _context.SaveChangesAsync();
        return new PessoaOutputDto { Id = pessoa.Id, Nome = pessoa.Nome, Idade = pessoa.Idade };
    }

    public async Task<PessoaOutputDto?> EditarAsync(int id, PessoaInputDto dto)
    {
        var pessoa = await _context.Pessoas.FindAsync(id);
        if (pessoa is null) return null;

        pessoa.Nome = dto.Nome;
        pessoa.Idade = dto.Idade;
        await _context.SaveChangesAsync();

        return new PessoaOutputDto { Id = pessoa.Id, Nome = pessoa.Nome, Idade = pessoa.Idade };
    }

    public async Task<bool> DeletarAsync(int id)
    {
        var pessoa = await _context.Pessoas.FindAsync(id);
        if (pessoa is null) return false;

        _context.Pessoas.Remove(pessoa);
        await _context.SaveChangesAsync();
        return true;
    }
}

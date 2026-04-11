using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Implementação concreta das operações de CRUD para <see cref="Pessoa"/>.
///
/// <b>Responsabilidades desta camada (Service):</b>
/// <list type="bullet">
///   <item>Acesso ao banco de dados via <see cref="AppDbContext"/>.</item>
///   <item>Mapeamento entre a entidade (<see cref="Pessoa"/>) e o DTO (<see cref="PessoaOutputDto"/>).</item>
///   <item>Persistência assíncrona com <c>SaveChangesAsync()</c>.</item>
/// </list>
///
/// <b>O que NÃO é responsabilidade desta camada:</b>
/// <list type="bullet">
///   <item>Formatação de resposta HTTP (responsabilidade do Controller).</item>
///   <item>Validação de formato dos dados (responsabilidade dos DataAnnotations no DTO).</item>
/// </list>
/// </summary>
public class PessoaService : IPessoaService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// O <see cref="AppDbContext"/> é injetado com ciclo de vida <c>Scoped</c>
    /// (uma instância por requisição HTTP), adequado para EF Core pois garante
    /// que o rastreamento de entidades não vaze entre requisições.
    /// </summary>
    public PessoaService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PessoaOutputDto>> ListarAsync()
    {
        // OrderBy executado no banco (SQL ORDER BY), não na memória.
        // Select projeta diretamente para o DTO, evitando trazer colunas desnecessárias.
        return await _context.Pessoas
            .OrderBy(p => p.Nome)
            .Select(p => new PessoaOutputDto { Id = p.Id, Nome = p.Nome, Idade = p.Idade })
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<PessoaOutputDto?> ObterPorIdAsync(int id)
    {
        // FindAsync utiliza o cache de identidade do EF Core:
        // se a entidade já foi carregada na mesma requisição, não bate no banco novamente.
        var pessoa = await _context.Pessoas.FindAsync(id);
        if (pessoa is null) return null;

        return new PessoaOutputDto { Id = pessoa.Id, Nome = pessoa.Nome, Idade = pessoa.Idade };
    }

    /// <inheritdoc/>
    public async Task<PessoaOutputDto> CriarAsync(PessoaInputDto dto)
    {
        // Mapeamento DTO → entidade: o Id não é definido aqui,
        // será gerado pelo banco de dados ao chamar SaveChangesAsync().
        var pessoa = new Pessoa { Nome = dto.Nome, Idade = dto.Idade };
        _context.Pessoas.Add(pessoa);
        await _context.SaveChangesAsync(); // Id de pessoa é populado após este ponto

        return new PessoaOutputDto { Id = pessoa.Id, Nome = pessoa.Nome, Idade = pessoa.Idade };
    }

    /// <inheritdoc/>
    public async Task<PessoaOutputDto?> EditarAsync(int id, PessoaInputDto dto)
    {
        var pessoa = await _context.Pessoas.FindAsync(id);
        if (pessoa is null) return null;

        // O EF Core rastreia as mudanças na entidade (Change Tracker).
        // Ao chamar SaveChangesAsync(), ele gera um UPDATE apenas com os campos alterados.
        pessoa.Nome  = dto.Nome;
        pessoa.Idade = dto.Idade;
        await _context.SaveChangesAsync();

        return new PessoaOutputDto { Id = pessoa.Id, Nome = pessoa.Nome, Idade = pessoa.Idade };
    }

    /// <inheritdoc/>
    public async Task<bool> DeletarAsync(int id)
    {
        var pessoa = await _context.Pessoas.FindAsync(id);
        if (pessoa is null) return false;

        // O DELETE CASCADE configurado no AppDbContext garante que
        // o banco remova automaticamente todas as Transações vinculadas.
        _context.Pessoas.Remove(pessoa);
        await _context.SaveChangesAsync();
        return true;
    }
}

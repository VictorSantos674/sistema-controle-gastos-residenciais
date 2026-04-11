using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Implementação concreta das operações de <see cref="Categoria"/>.
/// </summary>
public class CategoriaService : ICategoriaService
{
    private readonly AppDbContext _context;

    public CategoriaService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CategoriaOutputDto>> ListarAsync()
    {
        return await _context.Categorias
            .OrderBy(c => c.Descricao)
            .Select(c => new CategoriaOutputDto
            {
                Id = c.Id,
                Descricao = c.Descricao,
                // .ToString() no enum gera o nome do valor ("Despesa", "Receita" ou "Ambas")
                Finalidade = c.Finalidade.ToString()
            })
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<CategoriaOutputDto> CriarAsync(CategoriaInputDto dto)
    {
        var categoria = new Categoria { Descricao = dto.Descricao, Finalidade = dto.Finalidade };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return new CategoriaOutputDto
        {
            Id = categoria.Id,
            Descricao = categoria.Descricao,
            Finalidade = categoria.Finalidade.ToString()
        };
    }

    /// <inheritdoc/>
    public async Task<string?> DeletarAsync(int id)
    {
        // Include é necessário aqui para carregar as Transações e verificar a regra de negócio.
        // Sem Include, a coleção estaria vazia mesmo com transações no banco.
        var categoria = await _context.Categorias
            .Include(c => c.Transacoes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria is null)
            return "Categoria não encontrada.";

        // Regra de negócio: não é possível excluir uma categoria com transações vinculadas.
        // Motivo: preservar a integridade do histórico financeiro.
        // A verificação no serviço complementa o DeleteBehavior.Restrict do banco,
        // retornando uma mensagem amigável em vez de um erro de banco de dados bruto.
        if (categoria.Transacoes.Count > 0)
            return "Não é possível excluir uma categoria que possui transações vinculadas.";

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return null; // null = operação bem-sucedida
    }
}

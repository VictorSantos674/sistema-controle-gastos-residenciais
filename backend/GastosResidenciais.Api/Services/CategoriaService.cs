using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Implementação concreta das operações de <see cref="Categoria"/>.
/// Todas as queries são filtradas por <c>usuarioId</c>.
/// </summary>
public class CategoriaService : ICategoriaService
{
    private readonly AppDbContext _context;

    public CategoriaService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CategoriaOutputDto>> ListarAsync(int usuarioId)
    {
        return await _context.Categorias
            .Where(c => c.UsuarioId == usuarioId)
            .OrderBy(c => c.Descricao)
            .Select(c => new CategoriaOutputDto
            {
                Id         = c.Id,
                Descricao  = c.Descricao,
                Finalidade = c.Finalidade.ToString()
            })
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<CategoriaOutputDto> CriarAsync(CategoriaInputDto dto, int usuarioId)
    {
        var categoria = new Categoria
        {
            Descricao  = dto.Descricao,
            Finalidade = dto.Finalidade,
            UsuarioId  = usuarioId
        };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return new CategoriaOutputDto
        {
            Id         = categoria.Id,
            Descricao  = categoria.Descricao,
            Finalidade = categoria.Finalidade.ToString()
        };
    }

    /// <inheritdoc/>
    public async Task<CategoriaOutputDto?> EditarAsync(int id, CategoriaInputDto dto, int usuarioId)
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId);

        if (categoria is null)
            return null;

        categoria.Descricao = dto.Descricao;
        categoria.Finalidade = dto.Finalidade;
        await _context.SaveChangesAsync();

        return new CategoriaOutputDto
        {
            Id = categoria.Id,
            Descricao = categoria.Descricao,
            Finalidade = categoria.Finalidade.ToString()
        };
    }

    /// <inheritdoc/>
    public async Task<string?> DeletarAsync(int id, int usuarioId)
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId);

        if (categoria is null)
            return "Categoria não encontrada.";

        var possuiTransacoes = await _context.Transacoes
            .AnyAsync(t => t.CategoriaId == id && t.Categoria.UsuarioId == usuarioId);

        if (possuiTransacoes)
            return "Não é possível excluir uma categoria que possui transações vinculadas.";

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return null;
    }
}

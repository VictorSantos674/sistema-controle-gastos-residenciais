using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Services;

public class CategoriaService : ICategoriaService
{
    private readonly AppDbContext _context;

    public CategoriaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoriaOutputDto>> ListarAsync()
    {
        return await _context.Categorias
            .OrderBy(c => c.Descricao)
            .Select(c => new CategoriaOutputDto
            {
                Id = c.Id,
                Descricao = c.Descricao,
                Finalidade = c.Finalidade.ToString()
            })
            .ToListAsync();
    }

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

    public async Task<string?> DeletarAsync(int id)
    {
        var categoria = await _context.Categorias
            .Include(c => c.Transacoes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria is null)
            return "Categoria não encontrada.";

        /// Restrição: categoria com transações vinculadas não pode ser excluída.
        if (categoria.Transacoes.Count > 0)
            return "Não é possível excluir uma categoria que possui transações vinculadas.";

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return null;
    }
}

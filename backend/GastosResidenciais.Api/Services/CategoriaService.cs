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
}

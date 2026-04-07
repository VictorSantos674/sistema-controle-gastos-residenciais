using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

public interface ICategoriaService
{
    Task<IEnumerable<CategoriaOutputDto>> ListarAsync();
    Task<CategoriaOutputDto> CriarAsync(CategoriaInputDto dto);
    Task<string?> DeletarAsync(int id);
}

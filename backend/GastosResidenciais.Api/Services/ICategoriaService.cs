using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Contrato da camada de serviço para operações com <c>Categoria</c>.
/// Todos os métodos recebem <paramref name="usuarioId"/> para garantir isolamento de dados.
/// </summary>
public interface ICategoriaService
{
    Task<IEnumerable<CategoriaOutputDto>> ListarAsync(int usuarioId);
    Task<CategoriaOutputDto> CriarAsync(CategoriaInputDto dto, int usuarioId);
    Task<string?> DeletarAsync(int id, int usuarioId);
}

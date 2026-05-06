using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Contrato da camada de serviço para operações com <c>Categoria</c>.
/// Todos os métodos recebem <paramref name="usuarioId"/> para garantir isolamento de dados.
/// </summary>
public interface ICategoriaService
{
    /// <summary>
    /// Lista categorias do usuário autenticado.
    /// </summary>
    Task<IEnumerable<CategoriaOutputDto>> ListarAsync(int usuarioId);

    /// <summary>
    /// Cria uma categoria para o usuário autenticado.
    /// </summary>
    Task<CategoriaOutputDto> CriarAsync(CategoriaInputDto dto, int usuarioId);

    /// <summary>
    /// Edita uma categoria existente do usuário autenticado.
    /// </summary>
    Task<CategoriaOutputDto?> EditarAsync(int id, CategoriaInputDto dto, int usuarioId);

    /// <summary>
    /// Remove uma categoria quando não houver transações vinculadas.
    /// </summary>
    Task<string?> DeletarAsync(int id, int usuarioId);
}

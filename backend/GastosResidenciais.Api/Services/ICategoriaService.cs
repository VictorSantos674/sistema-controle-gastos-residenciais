using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Contrato da camada de serviço para operações com <c>Categoria</c>.
///
/// Categorias suportam criação, listagem e deleção.
/// A edição não é um requisito do sistema.
/// </summary>
public interface ICategoriaService
{
    /// <summary>Retorna todas as categorias ordenadas por descrição (A-Z).</summary>
    Task<IEnumerable<CategoriaOutputDto>> ListarAsync();

    /// <summary>Cria e persiste uma nova categoria.</summary>
    Task<CategoriaOutputDto> CriarAsync(CategoriaInputDto dto);

    /// <summary>
    /// Tenta deletar uma categoria.
    ///
    /// Retorna <c>null</c> em caso de sucesso.
    /// Retorna uma mensagem de erro descritiva se:
    /// <list type="bullet">
    ///   <item>A categoria não existir.</item>
    ///   <item>A categoria possuir transações vinculadas (integridade referencial).</item>
    /// </list>
    ///
    /// O controller mapeia o erro para 400 Bad Request com a mensagem.
    /// </summary>
    Task<string?> DeletarAsync(int id);
}

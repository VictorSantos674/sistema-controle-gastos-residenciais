using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Contrato da camada de serviço para operações com <c>Transacao</c>.
/// Todos os métodos recebem <paramref name="usuarioId"/> para garantir isolamento de dados.
/// </summary>
public interface ITransacaoService
{
    /// <summary>
    /// Lista transações do usuário autenticado com paginação.
    /// </summary>
    Task<PaginatedResponseDto<TransacaoOutputDto>> ListarAsync(int usuarioId, int page = 1, int pageSize = 20);

    /// <summary>
    /// Cria uma nova transação aplicando as regras de negócio.
    /// </summary>
    Task<(TransacaoOutputDto? Resultado, string? Erro)> CriarAsync(TransacaoInputDto dto, int usuarioId);

    /// <summary>
    /// Edita uma transação existente aplicando as mesmas regras de negócio da criação.
    /// </summary>
    Task<(TransacaoOutputDto? Resultado, string? Erro)> EditarAsync(int id, TransacaoInputDto dto, int usuarioId);

    /// <summary>
    /// Remove uma transação do usuário autenticado.
    /// </summary>
    Task<string?> DeletarAsync(int id, int usuarioId);
}

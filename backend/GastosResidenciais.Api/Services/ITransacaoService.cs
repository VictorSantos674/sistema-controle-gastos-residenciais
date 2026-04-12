using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Contrato da camada de serviço para operações com <c>Transacao</c>.
/// Todos os métodos recebem <paramref name="usuarioId"/> para garantir isolamento de dados.
/// </summary>
public interface ITransacaoService
{
    Task<IEnumerable<TransacaoOutputDto>> ListarAsync(int usuarioId);
    Task<(TransacaoOutputDto? Resultado, string? Erro)> CriarAsync(TransacaoInputDto dto, int usuarioId);
    Task<string?> DeletarAsync(int id, int usuarioId);
}

using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Contrato da camada de serviço para operações com <c>Pessoa</c>.
/// Todos os métodos recebem <paramref name="usuarioId"/> para garantir
/// que cada usuário acesse apenas seus próprios dados.
/// </summary>
public interface IPessoaService
{
    /// <summary>
    /// Lista pessoas do usuário autenticado.
    /// </summary>
    Task<IEnumerable<PessoaOutputDto>> ListarAsync(int usuarioId);

    /// <summary>
    /// Obtém uma pessoa do usuário autenticado por ID.
    /// </summary>
    Task<PessoaOutputDto?> ObterPorIdAsync(int id, int usuarioId);

    /// <summary>
    /// Cria uma pessoa validando regras de negócio da camada de serviço.
    /// </summary>
    Task<(PessoaOutputDto? Resultado, string? Erro)> CriarAsync(PessoaInputDto dto, int usuarioId);

    /// <summary>
    /// Edita uma pessoa existente do usuário autenticado.
    /// </summary>
    Task<(PessoaOutputDto? Resultado, string? Erro)> EditarAsync(int id, PessoaInputDto dto, int usuarioId);

    /// <summary>
    /// Remove uma pessoa e suas transações vinculadas.
    /// </summary>
    Task<string?> DeletarAsync(int id, int usuarioId);
}

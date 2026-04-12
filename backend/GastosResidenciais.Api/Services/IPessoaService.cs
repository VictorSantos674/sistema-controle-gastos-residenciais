using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Contrato da camada de serviço para operações com <c>Pessoa</c>.
/// Todos os métodos recebem <paramref name="usuarioId"/> para garantir
/// que cada usuário acesse apenas seus próprios dados.
/// </summary>
public interface IPessoaService
{
    Task<IEnumerable<PessoaOutputDto>> ListarAsync(int usuarioId);
    Task<PessoaOutputDto?> ObterPorIdAsync(int id, int usuarioId);
    Task<PessoaOutputDto> CriarAsync(PessoaInputDto dto, int usuarioId);
    Task<PessoaOutputDto?> EditarAsync(int id, PessoaInputDto dto, int usuarioId);
    Task<bool> DeletarAsync(int id, int usuarioId);
}

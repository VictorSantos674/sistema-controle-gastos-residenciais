using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

public interface IPessoaService
{
    Task<IEnumerable<PessoaOutputDto>> ListarAsync();
    Task<PessoaOutputDto?> ObterPorIdAsync(int id);
    Task<PessoaOutputDto> CriarAsync(PessoaInputDto dto);
    Task<PessoaOutputDto?> EditarAsync(int id, PessoaInputDto dto);
    Task<bool> DeletarAsync(int id);
}

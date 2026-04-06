using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

public interface ITransacaoService
{
    Task<IEnumerable<TransacaoOutputDto>> ListarAsync();
    Task<(TransacaoOutputDto? Resultado, string? Erro)> CriarAsync(TransacaoInputDto dto);
}

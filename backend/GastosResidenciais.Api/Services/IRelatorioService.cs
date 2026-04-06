using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

public interface IRelatorioService
{
    Task<RelatorioPorPessoaDto> ObterTotaisPorPessoaAsync();
    Task<RelatorioPorCategoriaDto> ObterTotaisPorCategoriaAsync();
}

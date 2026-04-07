using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

public interface IRelatorioService
{
    /// <param name="mes">Mês de filtro (1–12). Nulo = todos os meses.</param>
    /// <param name="ano">Ano de filtro (ex.: 2025). Nulo = todos os anos.</param>
    Task<RelatorioPorPessoaDto> ObterTotaisPorPessoaAsync(int? mes = null, int? ano = null);

    /// <param name="mes">Mês de filtro (1–12). Nulo = todos os meses.</param>
    /// <param name="ano">Ano de filtro (ex.: 2025). Nulo = todos os anos.</param>
    Task<RelatorioPorCategoriaDto> ObterTotaisPorCategoriaAsync(int? mes = null, int? ano = null);
}

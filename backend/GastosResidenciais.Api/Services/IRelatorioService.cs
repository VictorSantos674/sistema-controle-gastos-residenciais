using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Contrato para geração dos relatórios financeiros consolidados.
///
/// Os relatórios são somente leitura — não há criação, edição ou remoção.
/// Ambos os métodos suportam filtro opcional por mês e/ou ano para
/// análise de períodos específicos.
/// </summary>
public interface IRelatorioService
{
    /// <summary>
    /// Retorna totais de receitas, despesas e saldo de cada pessoa cadastrada,
    /// mais os totais gerais consolidados ao final.
    /// </summary>
    /// <param name="mes">Filtro opcional: mês (1–12). <c>null</c> = todos os meses.</param>
    /// <param name="ano">Filtro opcional: ano (ex.: 2025). <c>null</c> = todos os anos.</param>
    Task<RelatorioPorPessoaDto> ObterTotaisPorPessoaAsync(int? mes = null, int? ano = null);

    /// <summary>
    /// Retorna totais de receitas, despesas e saldo de cada categoria cadastrada,
    /// mais os totais gerais consolidados ao final.
    /// </summary>
    /// <param name="mes">Filtro opcional: mês (1–12). <c>null</c> = todos os meses.</param>
    /// <param name="ano">Filtro opcional: ano (ex.: 2025). <c>null</c> = todos os anos.</param>
    Task<RelatorioPorCategoriaDto> ObterTotaisPorCategoriaAsync(int? mes = null, int? ano = null);
}

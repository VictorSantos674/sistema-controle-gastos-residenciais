using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Contrato para geração dos relatórios financeiros filtrados por usuário.
/// </summary>
public interface IRelatorioService
{
    Task<RelatorioPorPessoaDto> ObterTotaisPorPessoaAsync(int usuarioId, int? mes = null, int? ano = null);
    Task<RelatorioPorCategoriaDto> ObterTotaisPorCategoriaAsync(int usuarioId, int? mes = null, int? ano = null);
}

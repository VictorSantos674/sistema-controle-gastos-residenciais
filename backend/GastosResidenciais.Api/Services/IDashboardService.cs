using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Contrato para consulta agregada do Dashboard.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Obtém o resumo financeiro e de cadastros do usuário autenticado.
    /// </summary>
    Task<DashboardResumoDto> ObterResumoAsync(int usuarioId);
}

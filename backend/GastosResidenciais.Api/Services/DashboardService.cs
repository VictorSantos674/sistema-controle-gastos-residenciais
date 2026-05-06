using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Implementa a consulta consolidada usada pelo Dashboard.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<DashboardResumoDto> ObterResumoAsync(int usuarioId)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        var resumo = await _context.Usuarios
            .Where(u => u.Id == usuarioId)
            .Select(u => new DashboardResumoDto
            {
                TotalReceitas =
                    (u.Pessoas.SelectMany(p => p.Transacoes)
                        .Where(t => t.Tipo == TipoTransacao.Receita)
                        .Sum(t => (decimal?)t.Valor) ?? 0)
                    +
                    (u.Pessoas.SelectMany(p => p.Transacoes)
                        .Where(t => t.Tipo == TipoTransacao.Ambas)
                        .Sum(t => t.ValorReceita) ?? 0),
                TotalDespesas =
                    (u.Pessoas.SelectMany(p => p.Transacoes)
                        .Where(t => t.Tipo == TipoTransacao.Despesa)
                        .Sum(t => (decimal?)t.Valor) ?? 0)
                    +
                    (u.Pessoas.SelectMany(p => p.Transacoes)
                        .Where(t => t.Tipo == TipoTransacao.Ambas)
                        .Sum(t => t.ValorDespesa) ?? 0),
                ReceitasMes =
                    (u.Pessoas.SelectMany(p => p.Transacoes)
                        .Where(t => t.Tipo == TipoTransacao.Receita
                            && t.Data.Month == hoje.Month
                            && t.Data.Year == hoje.Year)
                        .Sum(t => (decimal?)t.Valor) ?? 0)
                    +
                    (u.Pessoas.SelectMany(p => p.Transacoes)
                        .Where(t => t.Tipo == TipoTransacao.Ambas
                            && t.Data.Month == hoje.Month
                            && t.Data.Year == hoje.Year)
                        .Sum(t => t.ValorReceita) ?? 0),
                DespesasMes =
                    (u.Pessoas.SelectMany(p => p.Transacoes)
                        .Where(t => t.Tipo == TipoTransacao.Despesa
                            && t.Data.Month == hoje.Month
                            && t.Data.Year == hoje.Year)
                        .Sum(t => (decimal?)t.Valor) ?? 0)
                    +
                    (u.Pessoas.SelectMany(p => p.Transacoes)
                        .Where(t => t.Tipo == TipoTransacao.Ambas
                            && t.Data.Month == hoje.Month
                            && t.Data.Year == hoje.Year)
                        .Sum(t => t.ValorDespesa) ?? 0),
                TotalPessoas = u.Pessoas.Count,
                TotalCategorias = u.Categorias.Count,
                TotalTransacoes = u.Pessoas.SelectMany(p => p.Transacoes).Count()
            })
            .FirstOrDefaultAsync();

        if (resumo is null)
            return new DashboardResumoDto();

        resumo.SaldoLiquido = resumo.TotalReceitas - resumo.TotalDespesas;
        resumo.SaldoMes = resumo.ReceitasMes - resumo.DespesasMes;

        return resumo;
    }
}

using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Implementação dos relatórios financeiros filtrados por <c>usuarioId</c>.
/// </summary>
public class RelatorioService : IRelatorioService
{
    private readonly AppDbContext _context;

    public RelatorioService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<RelatorioPorPessoaDto> ObterTotaisPorPessoaAsync(int usuarioId, int? mes = null, int? ano = null)
    {
        var itens = await _context.Pessoas
            .Where(p => p.UsuarioId == usuarioId)
            .OrderBy(p => p.Nome)
            .Select(p => new TotalPorPessoaDto
            {
                PessoaId = p.Id,
                NomePessoa = p.Nome,
                TotalReceitas =
                    (p.Transacoes
                        .Where(t => t.Tipo == TipoTransacao.Receita
                            && (!mes.HasValue || t.Data.Month == mes.Value)
                            && (!ano.HasValue || t.Data.Year == ano.Value))
                        .Sum(t => (decimal?)t.Valor) ?? 0)
                    +
                    (p.Transacoes
                        .Where(t => t.Tipo == TipoTransacao.Ambas
                            && (!mes.HasValue || t.Data.Month == mes.Value)
                            && (!ano.HasValue || t.Data.Year == ano.Value))
                        .Sum(t => t.ValorReceita) ?? 0),
                TotalDespesas =
                    (p.Transacoes
                        .Where(t => t.Tipo == TipoTransacao.Despesa
                            && (!mes.HasValue || t.Data.Month == mes.Value)
                            && (!ano.HasValue || t.Data.Year == ano.Value))
                        .Sum(t => (decimal?)t.Valor) ?? 0)
                    +
                    (p.Transacoes
                        .Where(t => t.Tipo == TipoTransacao.Ambas
                            && (!mes.HasValue || t.Data.Month == mes.Value)
                            && (!ano.HasValue || t.Data.Year == ano.Value))
                        .Sum(t => t.ValorDespesa) ?? 0)
            })
            .ToListAsync();

        foreach (var item in itens)
            item.Saldo = item.TotalReceitas - item.TotalDespesas;

        return new RelatorioPorPessoaDto
        {
            Pessoas = itens,
            TotalGeralReceitas = itens.Sum(i => i.TotalReceitas),
            TotalGeralDespesas = itens.Sum(i => i.TotalDespesas),
            SaldoLiquido = itens.Sum(i => i.Saldo)
        };
    }

    /// <inheritdoc/>
    public async Task<RelatorioPorCategoriaDto> ObterTotaisPorCategoriaAsync(int usuarioId, int? mes = null, int? ano = null)
    {
        var itens = await _context.Categorias
            .Where(c => c.UsuarioId == usuarioId)
            .OrderBy(c => c.Descricao)
            .Select(c => new TotalPorCategoriaDto
            {
                CategoriaId = c.Id,
                DescricaoCategoria = c.Descricao,
                Finalidade = c.Finalidade.ToString(),
                TotalReceitas =
                    (c.Transacoes
                        .Where(t => t.Tipo == TipoTransacao.Receita
                            && (!mes.HasValue || t.Data.Month == mes.Value)
                            && (!ano.HasValue || t.Data.Year == ano.Value))
                        .Sum(t => (decimal?)t.Valor) ?? 0)
                    +
                    (c.Transacoes
                        .Where(t => t.Tipo == TipoTransacao.Ambas
                            && (!mes.HasValue || t.Data.Month == mes.Value)
                            && (!ano.HasValue || t.Data.Year == ano.Value))
                        .Sum(t => t.ValorReceita) ?? 0),
                TotalDespesas =
                    (c.Transacoes
                        .Where(t => t.Tipo == TipoTransacao.Despesa
                            && (!mes.HasValue || t.Data.Month == mes.Value)
                            && (!ano.HasValue || t.Data.Year == ano.Value))
                        .Sum(t => (decimal?)t.Valor) ?? 0)
                    +
                    (c.Transacoes
                        .Where(t => t.Tipo == TipoTransacao.Ambas
                            && (!mes.HasValue || t.Data.Month == mes.Value)
                            && (!ano.HasValue || t.Data.Year == ano.Value))
                        .Sum(t => t.ValorDespesa) ?? 0)
            })
            .ToListAsync();

        foreach (var item in itens)
            item.Saldo = item.TotalReceitas - item.TotalDespesas;

        return new RelatorioPorCategoriaDto
        {
            Categorias = itens,
            TotalGeralReceitas = itens.Sum(i => i.TotalReceitas),
            TotalGeralDespesas = itens.Sum(i => i.TotalDespesas),
            SaldoLiquido = itens.Sum(i => i.Saldo)
        };
    }
}

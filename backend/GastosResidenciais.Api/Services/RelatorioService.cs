using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Services;

public class RelatorioService : IRelatorioService
{
    private readonly AppDbContext _context;

    public RelatorioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RelatorioPorPessoaDto> ObterTotaisPorPessoaAsync(int? mes = null, int? ano = null)
    {
        var pessoas = await _context.Pessoas
            .Include(p => p.Transacoes)
            .OrderBy(p => p.Nome)
            .ToListAsync();

        var itens = pessoas.Select(p =>
        {
            var transacoes = FiltrarPorPeriodo(p.Transacoes, mes, ano);

            var receitas = transacoes.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor)
                         + transacoes.Where(t => t.Tipo == TipoTransacao.Ambas).Sum(t => t.ValorReceita ?? 0);
            var despesas = transacoes.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor)
                         + transacoes.Where(t => t.Tipo == TipoTransacao.Ambas).Sum(t => t.ValorDespesa ?? 0);

            return new TotalPorPessoaDto
            {
                PessoaId = p.Id,
                NomePessoa = p.Nome,
                TotalReceitas = receitas,
                TotalDespesas = despesas,
                Saldo = receitas - despesas
            };
        }).ToList();

        return new RelatorioPorPessoaDto
        {
            Pessoas = itens,
            TotalGeralReceitas = itens.Sum(i => i.TotalReceitas),
            TotalGeralDespesas = itens.Sum(i => i.TotalDespesas),
            SaldoLiquido = itens.Sum(i => i.Saldo)
        };
    }

    public async Task<RelatorioPorCategoriaDto> ObterTotaisPorCategoriaAsync(int? mes = null, int? ano = null)
    {
        var categorias = await _context.Categorias
            .Include(c => c.Transacoes)
            .OrderBy(c => c.Descricao)
            .ToListAsync();

        var itens = categorias.Select(c =>
        {
            var transacoes = FiltrarPorPeriodo(c.Transacoes, mes, ano);

            var receitas = transacoes.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor)
                         + transacoes.Where(t => t.Tipo == TipoTransacao.Ambas).Sum(t => t.ValorReceita ?? 0);
            var despesas = transacoes.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor)
                         + transacoes.Where(t => t.Tipo == TipoTransacao.Ambas).Sum(t => t.ValorDespesa ?? 0);

            return new TotalPorCategoriaDto
            {
                CategoriaId = c.Id,
                DescricaoCategoria = c.Descricao,
                Finalidade = c.Finalidade.ToString(),
                TotalReceitas = receitas,
                TotalDespesas = despesas,
                Saldo = receitas - despesas
            };
        }).ToList();

        return new RelatorioPorCategoriaDto
        {
            Categorias = itens,
            TotalGeralReceitas = itens.Sum(i => i.TotalReceitas),
            TotalGeralDespesas = itens.Sum(i => i.TotalDespesas),
            SaldoLiquido = itens.Sum(i => i.Saldo)
        };
    }

    private static IEnumerable<Transacao> FiltrarPorPeriodo(
        IEnumerable<Transacao> transacoes, int? mes, int? ano)
    {
        if (mes.HasValue)
            transacoes = transacoes.Where(t => t.Data.Month == mes.Value);
        if (ano.HasValue)
            transacoes = transacoes.Where(t => t.Data.Year == ano.Value);
        return transacoes;
    }
}

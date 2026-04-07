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

    public async Task<RelatorioPorPessoaDto> ObterTotaisPorPessoaAsync()
    {
        var pessoas = await _context.Pessoas
            .Include(p => p.Transacoes)
            .OrderBy(p => p.Nome)
            .ToListAsync();

        var itens = pessoas.Select(p =>
        {
            var receitas = p.Transacoes.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor)
                         + p.Transacoes.Where(t => t.Tipo == TipoTransacao.Ambas).Sum(t => t.ValorReceita ?? 0);
            var despesas = p.Transacoes.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor)
                         + p.Transacoes.Where(t => t.Tipo == TipoTransacao.Ambas).Sum(t => t.ValorDespesa ?? 0);
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

    public async Task<RelatorioPorCategoriaDto> ObterTotaisPorCategoriaAsync()
    {
        var categorias = await _context.Categorias
            .Include(c => c.Transacoes)
            .OrderBy(c => c.Descricao)
            .ToListAsync();

        var itens = categorias.Select(c =>
        {
            var receitas = c.Transacoes.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor)
                         + c.Transacoes.Where(t => t.Tipo == TipoTransacao.Ambas).Sum(t => t.ValorReceita ?? 0);
            var despesas = c.Transacoes.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor)
                         + c.Transacoes.Where(t => t.Tipo == TipoTransacao.Ambas).Sum(t => t.ValorDespesa ?? 0);
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
}

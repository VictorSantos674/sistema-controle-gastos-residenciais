using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Implementação dos relatórios financeiros consolidados.
///
/// <b>Estratégia de agregação:</b>
/// <list type="number">
///   <item>Carrega entidades com transações do banco via <c>Include</c> (JOIN).</item>
///   <item>Aplica filtros de período (mês/ano) em memória com LINQ to Objects.</item>
///   <item>Agrega receitas e despesas separadamente, somando também as parcelas
///         de transações do tipo <c>Ambas</c>.</item>
/// </list>
///
/// Esta abordagem é eficiente para volumes domésticos (centenas de transações).
/// Para volumes maiores, recomenda-se agregar diretamente no SQL com GroupBy.
/// </summary>
public class RelatorioService : IRelatorioService
{
    private readonly AppDbContext _context;

    public RelatorioService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<RelatorioPorPessoaDto> ObterTotaisPorPessoaAsync(int? mes = null, int? ano = null)
    {
        // Carrega todas as pessoas com suas transações em uma única query (JOIN).
        var pessoas = await _context.Pessoas
            .Include(p => p.Transacoes)
            .OrderBy(p => p.Nome)
            .ToListAsync();

        var itens = pessoas.Select(p =>
        {
            // Aplica filtro de período antes de somar
            var transacoes = FiltrarPorPeriodo(p.Transacoes, mes, ano);

            // Soma receitas: transações do tipo Receita + parcela de receita das transações Ambas
            var receitas = transacoes.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor)
                         + transacoes.Where(t => t.Tipo == TipoTransacao.Ambas).Sum(t => t.ValorReceita ?? 0);

            // Soma despesas: transações do tipo Despesa + parcela de despesa das transações Ambas
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

        // Totais gerais são calculados a partir dos itens já processados,
        // evitando uma segunda passagem pelo banco.
        return new RelatorioPorPessoaDto
        {
            Pessoas = itens,
            TotalGeralReceitas = itens.Sum(i => i.TotalReceitas),
            TotalGeralDespesas = itens.Sum(i => i.TotalDespesas),
            SaldoLiquido = itens.Sum(i => i.Saldo)
        };
    }

    /// <inheritdoc/>
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

    /// <summary>
    /// Filtra uma coleção de transações pelo período informado.
    /// Parâmetros nulos indicam "sem restrição" — retorna todas.
    /// </summary>
    private static IEnumerable<Transacao> FiltrarPorPeriodo(
        IEnumerable<Transacao> transacoes, int? mes, int? ano)
    {
        if (mes.HasValue) transacoes = transacoes.Where(t => t.Data.Month == mes.Value);
        if (ano.HasValue) transacoes = transacoes.Where(t => t.Data.Year  == ano.Value);
        return transacoes;
    }
}

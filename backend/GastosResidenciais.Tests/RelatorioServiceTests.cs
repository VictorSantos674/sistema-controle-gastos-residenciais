using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.Models;
using GastosResidenciais.Api.Services;
using Xunit;

namespace GastosResidenciais.Tests;

public class RelatorioServiceTests
{
    [Fact]
    public async Task RelatorioPorPessoaComTransacoesMistasCalculaTotaisCorretamente()
    {
        await using var context = TestDb.CreateContext();
        SeedBase(context);
        context.Transacoes.AddRange(
            Transacao(1, TipoTransacao.Despesa, 100, pessoaId: 1, categoriaId: 1),
            Transacao(2, TipoTransacao.Receita, 500, pessoaId: 1, categoriaId: 2),
            TransacaoAmbas(3, valorReceita: 200, valorDespesa: 50, pessoaId: 1, categoriaId: 3));
        await context.SaveChangesAsync();
        var service = new RelatorioService(context);

        var relatorio = await service.ObterTotaisPorPessoaAsync(usuarioId: 1);
        var pessoa = relatorio.Pessoas.Single();

        Assert.Equal(700, pessoa.TotalReceitas);
        Assert.Equal(150, pessoa.TotalDespesas);
        Assert.Equal(550, pessoa.Saldo);
        Assert.Equal(550, relatorio.SaldoLiquido);
    }

    [Fact]
    public async Task RelatorioPorPessoaFiltradoPorMesRetornaApenasMesInformado()
    {
        await using var context = TestDb.CreateContext();
        SeedBase(context);
        context.Transacoes.AddRange(
            Transacao(1, TipoTransacao.Receita, 500, pessoaId: 1, categoriaId: 2, data: new DateOnly(2026, 5, 6)),
            Transacao(2, TipoTransacao.Receita, 300, pessoaId: 1, categoriaId: 2, data: new DateOnly(2026, 6, 6)));
        await context.SaveChangesAsync();
        var service = new RelatorioService(context);

        var relatorio = await service.ObterTotaisPorPessoaAsync(usuarioId: 1, mes: 5, ano: 2026);

        Assert.Equal(500, relatorio.TotalGeralReceitas);
        Assert.Equal(500, relatorio.SaldoLiquido);
    }

    [Fact]
    public async Task RelatorioPorCategoriaAgrupaCorretamenteTiposDiferentes()
    {
        await using var context = TestDb.CreateContext();
        SeedBase(context);
        context.Transacoes.AddRange(
            Transacao(1, TipoTransacao.Despesa, 100, pessoaId: 1, categoriaId: 1),
            Transacao(2, TipoTransacao.Receita, 500, pessoaId: 1, categoriaId: 2),
            TransacaoAmbas(3, valorReceita: 200, valorDespesa: 50, pessoaId: 1, categoriaId: 3));
        await context.SaveChangesAsync();
        var service = new RelatorioService(context);

        var relatorio = await service.ObterTotaisPorCategoriaAsync(usuarioId: 1);
        var categorias = relatorio.Categorias.ToList();

        Assert.Equal(3, categorias.Count);
        Assert.Equal(700, relatorio.TotalGeralReceitas);
        Assert.Equal(150, relatorio.TotalGeralDespesas);
        Assert.Equal(550, relatorio.SaldoLiquido);
    }

    [Fact]
    public async Task RelatorioRetornaZeroParaUsuarioSemTransacoes()
    {
        await using var context = TestDb.CreateContext();
        context.Usuarios.Add(new Usuario { Id = 1, Login = "user", SenhaHash = "hash" });
        context.Pessoas.Add(new Pessoa { Id = 1, Nome = "Ana", Idade = 30, UsuarioId = 1 });
        await context.SaveChangesAsync();
        var service = new RelatorioService(context);

        var porPessoa = await service.ObterTotaisPorPessoaAsync(usuarioId: 1);
        var porCategoria = await service.ObterTotaisPorCategoriaAsync(usuarioId: 1);

        Assert.Equal(0, porPessoa.TotalGeralReceitas);
        Assert.Equal(0, porPessoa.TotalGeralDespesas);
        Assert.Equal(0, porPessoa.SaldoLiquido);
        Assert.Equal(0, porCategoria.TotalGeralReceitas);
        Assert.Equal(0, porCategoria.TotalGeralDespesas);
        Assert.Equal(0, porCategoria.SaldoLiquido);
    }

    private static void SeedBase(AppDbContext context)
    {
        context.Usuarios.Add(new Usuario { Id = 1, Login = "user", SenhaHash = "hash" });
        context.Pessoas.Add(new Pessoa { Id = 1, Nome = "Ana", Idade = 30, UsuarioId = 1 });
        context.Categorias.AddRange(
            new Categoria { Id = 1, Descricao = "Despesa", Finalidade = Finalidade.Despesa, UsuarioId = 1 },
            new Categoria { Id = 2, Descricao = "Receita", Finalidade = Finalidade.Receita, UsuarioId = 1 },
            new Categoria { Id = 3, Descricao = "Ambas", Finalidade = Finalidade.Ambas, UsuarioId = 1 });
    }

    private static Transacao Transacao(
        int id,
        TipoTransacao tipo,
        decimal valor,
        int pessoaId,
        int categoriaId,
        DateOnly? data = null) =>
        new()
        {
            Id = id,
            Descricao = $"Transação {id}",
            Valor = valor,
            Tipo = tipo,
            PessoaId = pessoaId,
            CategoriaId = categoriaId,
            Data = data ?? new DateOnly(2026, 5, 6)
        };

    private static Transacao TransacaoAmbas(
        int id,
        decimal valorReceita,
        decimal valorDespesa,
        int pessoaId,
        int categoriaId) =>
        new()
        {
            Id = id,
            Descricao = $"Transação {id}",
            Valor = 0,
            ValorReceita = valorReceita,
            ValorDespesa = valorDespesa,
            Tipo = TipoTransacao.Ambas,
            PessoaId = pessoaId,
            CategoriaId = categoriaId,
            Data = new DateOnly(2026, 5, 6)
        };
}

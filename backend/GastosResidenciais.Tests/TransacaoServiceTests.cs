using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using GastosResidenciais.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GastosResidenciais.Tests;

public class TransacaoServiceTests
{
    [Fact]
    public async Task MenorDe18NaoPodeCriarReceitaOuAmbas()
    {
        await using var context = TestDb.CreateContext();
        SeedBase(context, idadePessoa: 17, finalidade: Finalidade.Ambas);
        var service = new TransacaoService(context);

        var receita = await service.CriarAsync(Input(TipoTransacao.Receita, valor: 100), usuarioId: 1);
        var ambas = await service.CriarAsync(Input(TipoTransacao.Ambas, valorReceita: 100, valorDespesa: 10), usuarioId: 1);

        Assert.Null(receita.Resultado);
        Assert.Contains("Menores de 18", receita.Erro);
        Assert.Null(ambas.Resultado);
        Assert.Contains("Menores de 18", ambas.Erro);
    }

    [Fact]
    public async Task CategoriaIncompativelRetornaErro()
    {
        await using var context = TestDb.CreateContext();
        SeedBase(context, idadePessoa: 30, finalidade: Finalidade.Despesa);
        var service = new TransacaoService(context);

        var resultado = await service.CriarAsync(Input(TipoTransacao.Receita, valor: 100), usuarioId: 1);

        Assert.Null(resultado.Resultado);
        Assert.Contains("não é compatível", resultado.Erro);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValorZeroOuNegativoRetornaErro(decimal valor)
    {
        await using var context = TestDb.CreateContext();
        SeedBase(context, idadePessoa: 30, finalidade: Finalidade.Despesa);
        var service = new TransacaoService(context);

        var resultado = await service.CriarAsync(Input(TipoTransacao.Despesa, valor: valor), usuarioId: 1);

        Assert.Null(resultado.Resultado);
        Assert.Equal("O valor deve ser positivo.", resultado.Erro);
    }

    [Fact]
    public async Task TransacaoValidaEPersistidaERetornadaCorretamente()
    {
        await using var context = TestDb.CreateContext();
        SeedBase(context, idadePessoa: 30, finalidade: Finalidade.Despesa);
        var service = new TransacaoService(context);

        var resultado = await service.CriarAsync(Input(TipoTransacao.Despesa, valor: 75), usuarioId: 1);

        Assert.NotNull(resultado.Resultado);
        Assert.Null(resultado.Erro);
        Assert.Equal(75, resultado.Resultado!.Valor);
        Assert.Equal("Despesa", resultado.Resultado.Tipo);
        Assert.Equal(1, await context.Transacoes.CountAsync());
    }

    private static TransacaoInputDto Input(
        TipoTransacao tipo,
        decimal? valor = null,
        decimal? valorReceita = null,
        decimal? valorDespesa = null) =>
        new()
        {
            Descricao = "Teste",
            Tipo = tipo,
            Valor = valor,
            ValorReceita = valorReceita,
            ValorDespesa = valorDespesa,
            CategoriaId = 1,
            PessoaId = 1,
            Data = new DateOnly(2026, 5, 6)
        };

    private static void SeedBase(AppDbContext context, int idadePessoa, Finalidade finalidade)
    {
        context.Usuarios.Add(new Usuario { Id = 1, Login = "user", SenhaHash = "hash" });
        context.Pessoas.Add(new Pessoa { Id = 1, Nome = "Pessoa", Idade = idadePessoa, UsuarioId = 1 });
        context.Categorias.Add(new Categoria { Id = 1, Descricao = "Categoria", Finalidade = finalidade, UsuarioId = 1 });
        context.SaveChanges();
    }
}

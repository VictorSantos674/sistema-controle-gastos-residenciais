using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using GastosResidenciais.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GastosResidenciais.Tests;

public class PessoaServiceTests
{
    [Fact]
    public async Task CriarPessoaComDadosValidosPersisteERetornaDto()
    {
        await using var context = TestDb.CreateContext();
        context.Usuarios.Add(new Usuario { Id = 1, Login = "user", SenhaHash = "hash" });
        await context.SaveChangesAsync();
        var service = new PessoaService(context);

        var resultado = await service.CriarAsync(new PessoaInputDto { Nome = "Ana", Idade = 30 }, usuarioId: 1);

        Assert.Null(resultado.Erro);
        Assert.NotNull(resultado.Resultado);
        Assert.Equal("Ana", resultado.Resultado!.Nome);
        Assert.Equal(1, await context.Pessoas.CountAsync());
    }

    [Fact]
    public async Task CriarPessoaComNomeVazioRetornaErro()
    {
        await using var context = TestDb.CreateContext();
        var service = new PessoaService(context);

        var resultado = await service.CriarAsync(new PessoaInputDto { Nome = "", Idade = 30 }, usuarioId: 1);

        Assert.Null(resultado.Resultado);
        Assert.Equal("O nome é obrigatório.", resultado.Erro);
    }

    [Fact]
    public async Task EditarPessoaInexistenteRetornaErro()
    {
        await using var context = TestDb.CreateContext();
        var service = new PessoaService(context);

        var resultado = await service.EditarAsync(99, new PessoaInputDto { Nome = "Ana", Idade = 30 }, usuarioId: 1);

        Assert.Null(resultado.Resultado);
        Assert.Equal("Pessoa não encontrada.", resultado.Erro);
    }

    [Fact]
    public async Task DeletarPessoaRemoveTransacoesVinculadas()
    {
        await using var context = TestDb.CreateContext();
        SeedUsuario(context, 1);
        context.Pessoas.Add(new Pessoa { Id = 1, Nome = "Ana", Idade = 30, UsuarioId = 1 });
        context.Categorias.Add(new Categoria { Id = 1, Descricao = "Casa", Finalidade = Finalidade.Despesa, UsuarioId = 1 });
        context.Transacoes.Add(new Transacao
        {
            Id = 1,
            Descricao = "Aluguel",
            Valor = 800,
            Tipo = TipoTransacao.Despesa,
            CategoriaId = 1,
            PessoaId = 1,
            Data = new DateOnly(2026, 5, 6)
        });
        await context.SaveChangesAsync();
        var service = new PessoaService(context);

        var erro = await service.DeletarAsync(1, usuarioId: 1);

        Assert.Null(erro);
        Assert.Equal(0, await context.Pessoas.CountAsync());
        Assert.Equal(0, await context.Transacoes.CountAsync());
    }

    [Fact]
    public async Task ListarPessoasRetornaApenasDoUsuarioInformado()
    {
        await using var context = TestDb.CreateContext();
        SeedUsuario(context, 1);
        SeedUsuario(context, 2);
        context.Pessoas.AddRange(
            new Pessoa { Id = 1, Nome = "Ana", Idade = 30, UsuarioId = 1 },
            new Pessoa { Id = 2, Nome = "Bruno", Idade = 40, UsuarioId = 2 });
        await context.SaveChangesAsync();
        var service = new PessoaService(context);

        var pessoas = (await service.ListarAsync(usuarioId: 1)).ToList();

        Assert.Single(pessoas);
        Assert.Equal("Ana", pessoas[0].Nome);
    }

    private static void SeedUsuario(AppDbContext context, int id) =>
        context.Usuarios.Add(new Usuario { Id = id, Login = $"user{id}", SenhaHash = "hash" });
}

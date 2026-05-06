using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using GastosResidenciais.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GastosResidenciais.Tests;

public class CategoriaServiceTests
{
    [Fact]
    public async Task CriarCategoriaComDadosValidosPersisteERetornaDto()
    {
        await using var context = TestDb.CreateContext();
        SeedUsuario(context, 1);
        await context.SaveChangesAsync();
        var service = new CategoriaService(context);

        var resultado = await service.CriarAsync(
            new CategoriaInputDto { Descricao = "Moradia", Finalidade = Finalidade.Despesa },
            usuarioId: 1);

        Assert.Equal("Moradia", resultado.Descricao);
        Assert.Equal("Despesa", resultado.Finalidade);
        Assert.Equal(1, await context.Categorias.CountAsync());
    }

    [Fact]
    public async Task DeletarCategoriaComTransacoesVinculadasRetornaErro()
    {
        await using var context = TestDb.CreateContext();
        SeedCategoriaComTransacao(context);
        var service = new CategoriaService(context);

        var erro = await service.DeletarAsync(1, usuarioId: 1);

        Assert.Equal("Não é possível excluir uma categoria que possui transações vinculadas.", erro);
        Assert.Equal(1, await context.Categorias.CountAsync());
    }

    [Fact]
    public async Task DeletarCategoriaSemTransacoesVinculadasRemoveComSucesso()
    {
        await using var context = TestDb.CreateContext();
        SeedUsuario(context, 1);
        context.Categorias.Add(new Categoria { Id = 1, Descricao = "Lazer", Finalidade = Finalidade.Despesa, UsuarioId = 1 });
        await context.SaveChangesAsync();
        var service = new CategoriaService(context);

        var erro = await service.DeletarAsync(1, usuarioId: 1);

        Assert.Null(erro);
        Assert.Equal(0, await context.Categorias.CountAsync());
    }

    [Fact]
    public async Task ListarCategoriasRetornaApenasDoUsuarioInformado()
    {
        await using var context = TestDb.CreateContext();
        SeedUsuario(context, 1);
        SeedUsuario(context, 2);
        context.Categorias.AddRange(
            new Categoria { Id = 1, Descricao = "Casa", Finalidade = Finalidade.Despesa, UsuarioId = 1 },
            new Categoria { Id = 2, Descricao = "Outro usuário", Finalidade = Finalidade.Receita, UsuarioId = 2 });
        await context.SaveChangesAsync();
        var service = new CategoriaService(context);

        var categorias = (await service.ListarAsync(usuarioId: 1)).ToList();

        Assert.Single(categorias);
        Assert.Equal("Casa", categorias[0].Descricao);
    }

    private static void SeedCategoriaComTransacao(AppDbContext context)
    {
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
        context.SaveChanges();
    }

    private static void SeedUsuario(AppDbContext context, int id) =>
        context.Usuarios.Add(new Usuario { Id = id, Login = $"user{id}", SenhaHash = "hash" });
}

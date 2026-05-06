using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using GastosResidenciais.Api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GastosResidenciais.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginComSenhaErradaRetornaErroGenerico()
    {
        await using var context = TestDb.CreateContext();
        context.Usuarios.Add(new Usuario
        {
            Login = "maria",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("correta")
        });
        await context.SaveChangesAsync();
        var service = new AuthService(context, Config());

        var resultado = await service.LoginAsync(new LoginDto { Login = "maria", Senha = "errada" });

        Assert.Null(resultado.Resultado);
        Assert.Equal("Login ou senha inválidos.", resultado.Erro);
    }

    [Fact]
    public async Task RegistroComLoginDuplicadoRetornaErro()
    {
        await using var context = TestDb.CreateContext();
        context.Usuarios.Add(new Usuario
        {
            Login = "joao",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("123456")
        });
        await context.SaveChangesAsync();
        var service = new AuthService(context, Config());

        var resultado = await service.RegistrarAsync(new RegistrarDto { Login = "joao", Senha = "123456" });

        Assert.Null(resultado.Resultado);
        Assert.Contains("já está em uso", resultado.Erro);
    }

    [Fact]
    public async Task RegistroValidoGeraTokenJwt()
    {
        await using var context = TestDb.CreateContext();
        var service = new AuthService(context, Config());

        var resultado = await service.RegistrarAsync(new RegistrarDto { Login = "ana", Senha = "123456" });

        Assert.NotNull(resultado.Resultado);
        Assert.Null(resultado.Erro);
        Assert.Equal("ana", resultado.Resultado!.Login);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Resultado.Token));
        Assert.Equal(3, resultado.Resultado.Token.Split('.').Length);
    }

    private static IConfiguration Config() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWT_SECRET"] = "test-secret-change-in-tests-min32chars!!"
            })
            .Build();
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GastosResidenciais.Api.DTOs;
using Xunit;

namespace GastosResidenciais.Tests;

public class ControllerIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task TransacoesController_CriaEListaComPaginacao()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();
        await AutenticarAsync(client, "transacoes-user");
        var pessoaId = await CriarPessoaAsync(client);
        var categoriaId = await CriarCategoriaAsync(client, finalidade: 1);

        var criarResponse = await client.PostAsJsonAsync("/api/transacoes", new
        {
            descricao = "Mercado",
            valor = 120,
            tipo = 1,
            categoriaId,
            pessoaId,
            data = "2026-05-06"
        });

        Assert.True(criarResponse.StatusCode == HttpStatusCode.Created, await criarResponse.Content.ReadAsStringAsync());

        var lista = await client.GetFromJsonAsync<PaginatedResponseDto<TransacaoOutputDto>>(
            "/api/transacoes?page=1&pageSize=20",
            JsonOptions);

        Assert.NotNull(lista);
        Assert.Single(lista!.Data);
        Assert.Equal(1, lista.Total);
        Assert.Equal(1, lista.Page);
        Assert.Equal(20, lista.PageSize);
    }

    [Fact]
    public async Task RelatoriosController_RetornaTotaisPorPessoaECategoria()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();
        await AutenticarAsync(client, "relatorios-user");
        var pessoaId = await CriarPessoaAsync(client);
        var categoriaDespesaId = await CriarCategoriaAsync(client, finalidade: 1);
        var categoriaReceitaId = await CriarCategoriaAsync(client, finalidade: 2);

        var receitaResponse = await client.PostAsJsonAsync("/api/transacoes", new
        {
            descricao = "Salário",
            valor = 2000,
            tipo = 2,
            categoriaId = categoriaReceitaId,
            pessoaId,
            data = "2026-05-06"
        });
        Assert.True(receitaResponse.IsSuccessStatusCode, await receitaResponse.Content.ReadAsStringAsync());

        var despesaResponse = await client.PostAsJsonAsync("/api/transacoes", new
        {
            descricao = "Aluguel",
            valor = 800,
            tipo = 1,
            categoriaId = categoriaDespesaId,
            pessoaId,
            data = "2026-05-06"
        });
        Assert.True(despesaResponse.IsSuccessStatusCode, await despesaResponse.Content.ReadAsStringAsync());

        var porPessoa = await client.GetFromJsonAsync<RelatorioPorPessoaDto>(
            "/api/relatorios/por-pessoa?mes=5&ano=2026",
            JsonOptions);
        var porCategoria = await client.GetFromJsonAsync<RelatorioPorCategoriaDto>(
            "/api/relatorios/por-categoria?mes=5&ano=2026",
            JsonOptions);

        Assert.NotNull(porPessoa);
        Assert.Equal(2000, porPessoa!.TotalGeralReceitas);
        Assert.Equal(800, porPessoa.TotalGeralDespesas);
        Assert.Equal(1200, porPessoa.SaldoLiquido);

        Assert.NotNull(porCategoria);
        Assert.Equal(2, porCategoria!.Categorias.Count());
        Assert.Equal(1200, porCategoria.SaldoLiquido);
    }

    private static async Task AutenticarAsync(HttpClient client, string login)
    {
        var response = await client.PostAsJsonAsync("/api/auth/registrar", new
        {
            login,
            senha = "12345678"
        });
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenDto>(JsonOptions);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.Token);
    }

    private static async Task<int> CriarPessoaAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/pessoas", new
        {
            nome = "Pessoa Teste",
            idade = 30
        });
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetInt32();
    }

    private static async Task<int> CriarCategoriaAsync(HttpClient client, int finalidade)
    {
        var response = await client.PostAsJsonAsync("/api/categorias", new
        {
            descricao = $"Categoria {Guid.NewGuid():N}",
            finalidade
        });
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetInt32();
    }
}

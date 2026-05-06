using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace GastosResidenciais.Api.Data;

/// <summary>
/// Factory usada pelo EF Core CLI para criar o contexto em tempo de design sem subir o host web.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// Cria o <see cref="AppDbContext"/> para comandos como <c>dotnet ef migrations add</c>.
    /// </summary>
    public AppDbContext CreateDbContext(string[] args)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        var connectionString = BuildPostgresConnectionString(
            databaseUrl,
            "Host=localhost;Port=5432;Database=gastos_residenciais;Username=postgres;Password=postgres");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static string BuildPostgresConnectionString(string? databaseUrl, string fallback)
    {
        if (string.IsNullOrWhiteSpace(databaseUrl))
            return fallback;

        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            SslMode = SslMode.Require
        };

        return builder.ConnectionString;
    }
}

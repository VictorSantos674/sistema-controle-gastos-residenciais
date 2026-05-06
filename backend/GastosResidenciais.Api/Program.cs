using System.Text;
using System.Threading.RateLimiting;
using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

/// Railway (e outros PaaS) injetam a porta via variável de ambiente PORT.
/// Em desenvolvimento local usa-se a porta padrão do appsettings.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/// PostgreSQL como banco de dados persistente.
/// Em produção, Railway fornece DATABASE_URL no formato URL.
var connectionString = BuildPostgresConnectionString(
    builder.Configuration["DATABASE_URL"],
    builder.Configuration.GetConnectionString("Default"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

/// Registro dos serviços de domínio via injeção de dependência.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPessoaService, PessoaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ITransacaoService, TransacaoService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

/// Autenticação JWT.
/// A chave secreta é lida da variável de ambiente JWT_SECRET.
/// Em desenvolvimento, o fallback garante que a app suba sem configuração extra.
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? "dev-secret-change-in-production-min32chars!!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = "GastosResidenciais",
            ValidAudience            = "GastosResidenciais",
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

/// Política global: todos os endpoints exigem autenticação por padrão.
/// Endpoints públicos usam [AllowAnonymous] explicitamente (ex.: AuthController e /health).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { mensagem = "Muitas tentativas. Tente novamente em 60 segundos." },
            cancellationToken: token);
    };

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

/// CORS: lê origens permitidas da variável de ambiente CORS_ORIGINS (separadas por vírgula).
var corsOrigins = (Environment.GetEnvironmentVariable("CORS_ORIGINS") ?? "http://localhost:5173")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEnd", policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

/// Aplica as migrations pendentes automaticamente ao iniciar.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();
}

app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    ctx.Response.StatusCode = 500;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(new { mensagem = "Erro interno do servidor. Tente novamente." });
}));

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("FrontEnd");
app.UseRateLimiter();

/// A ordem importa: Authentication deve vir antes de Authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();
await app.RunAsync();

static string BuildPostgresConnectionString(string? databaseUrl, string? fallback)
{
    if (string.IsNullOrWhiteSpace(databaseUrl))
        return fallback ?? throw new InvalidOperationException("DATABASE_URL não configurado.");

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

public partial class Program;

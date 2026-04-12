using System.Text;
using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

/// Railway (e outros PaaS) injetam a porta via variável de ambiente PORT.
/// Em desenvolvimento local usa-se a porta padrão do appsettings.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/// SQLite como banco de dados persistente.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

/// Registro dos serviços de domínio via injeção de dependência.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPessoaService, PessoaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ITransacaoService, TransacaoService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();

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
/// Endpoints públicos usam [AllowAnonymous] explicitamente (ex.: AuthController).
builder.Services.AddAuthorization();

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

/// Aplica as migrations pendentes e cria o banco de dados automaticamente ao iniciar.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
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

/// A ordem importa: Authentication deve vir antes de Authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
await app.RunAsync();

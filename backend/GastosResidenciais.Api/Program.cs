using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

/// Railway (e outros PaaS) injetam a porta via variável de ambiente PORT.
/// Em desenvolvimento local usa-se a porta padrão do appsettings.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/// SQLite como banco de dados persistente. O arquivo gastos.db é criado automaticamente na raiz do projeto.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

/// Registro dos serviços de domínio via injeção de dependência.
builder.Services.AddScoped<IPessoaService, PessoaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ITransacaoService, TransacaoService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();

/// CORS: lê origens permitidas da variável de ambiente CORS_ORIGINS (separadas por vírgula).
/// Fallback para localhost em desenvolvimento.
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
app.MapControllers();
await app.RunAsync();

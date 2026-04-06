using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

/// SQLite como banco de dados persistente. O arquivo gastos.db é criado automaticamente na raiz do projeto.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

/// Registro dos serviços de domínio via injeção de dependência.
builder.Services.AddScoped<IPessoaService, PessoaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ITransacaoService, TransacaoService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();

/// CORS liberado para desenvolvimento local com o front-end React (porta 5173).
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEnd", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

/// Aplica as migrations pendentes e cria o banco de dados automaticamente ao iniciar.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors("FrontEnd");
app.MapControllers();
app.Run();

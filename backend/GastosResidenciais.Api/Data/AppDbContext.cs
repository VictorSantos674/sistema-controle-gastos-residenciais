using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Data;

/// <summary>
/// Contexto de banco de dados da aplicação — ponto central de acesso ao SQLite via EF Core.
///
/// <para><b>Padrão usado:</b> Unit of Work + Identity Map implícitos do EF Core.
/// Cada requisição HTTP recebe uma instância Scoped deste contexto, garantindo que
/// todas as operações da requisição compartilhem a mesma sessão de banco e possam
/// ser confirmadas ou revertidas atomicamente via <c>SaveChangesAsync()</c>.</para>
///
/// <para><b>Banco de dados:</b> SQLite — arquivo único (<c>gastos.db</c>), sem servidor,
/// criado automaticamente pelo <see cref="Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.EnsureCreatedAsync()"/>
/// na inicialização da aplicação (<c>Program.cs</c>).</para>
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Recebe as opções de configuração injetadas pelo contêiner DI
    /// (provider SQLite + string de conexão do <c>appsettings.json</c>).
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── DbSets (mapeados para tabelas no banco) ──────────────────────────────

    /// <summary>Tabela <c>Pessoas</c>.</summary>
    public DbSet<Pessoa> Pessoas { get; set; }

    /// <summary>Tabela <c>Categorias</c>.</summary>
    public DbSet<Categoria> Categorias { get; set; }

    /// <summary>Tabela <c>Transacoes</c>.</summary>
    public DbSet<Transacao> Transacoes { get; set; }

    /// <summary>
    /// Configura o mapeamento objeto-relacional via Fluent API.
    /// Chamado automaticamente pelo EF Core ao criar/verificar o schema.
    ///
    /// Decisões de design documentadas abaixo:
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Relacionamento Transacao → Pessoa ────────────────────────────────
        // DeleteBehavior.Cascade: ao excluir uma Pessoa, o banco de dados
        // remove automaticamente todas as Transações vinculadas (ON DELETE CASCADE).
        // Isso garante o requisito "ao deletar pessoa, apaga suas transações"
        // sem precisar de lógica extra na aplicação — mais seguro e eficiente.
        modelBuilder.Entity<Transacao>()
            .HasOne(t => t.Pessoa)
            .WithMany(p => p.Transacoes)
            .HasForeignKey(t => t.PessoaId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Relacionamento Transacao → Categoria ─────────────────────────────
        // DeleteBehavior.Restrict: impede a exclusão de uma Categoria que
        // ainda possua Transações vinculadas (ON DELETE RESTRICT).
        // Isso preserva a integridade do histórico financeiro — a exclusão
        // só é permitida após remover ou reclassificar as transações.
        modelBuilder.Entity<Transacao>()
            .HasOne(t => t.Categoria)
            .WithMany(c => c.Transacoes)
            .HasForeignKey(t => t.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Precisão monetária ────────────────────────────────────────────────
        // decimal(18,2): até 16 dígitos inteiros e 2 casas decimais,
        // suficiente para qualquer valor financeiro residencial.
        // Necessário especificar pois o SQLite usa tipagem dinâmica.
        modelBuilder.Entity<Transacao>()
            .Property(t => t.Valor)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Transacao>()
            .Property(t => t.ValorReceita)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Transacao>()
            .Property(t => t.ValorDespesa)
            .HasColumnType("decimal(18,2)");

        // ── Tipo de data ──────────────────────────────────────────────────────
        // SQLite não tem tipo DATE nativo; armazenamos DateOnly como TEXT
        // no formato ISO 8601 (yyyy-MM-dd) para permitir ordenação lexicográfica.
        modelBuilder.Entity<Transacao>()
            .Property(t => t.Data)
            .HasColumnType("TEXT");
    }
}

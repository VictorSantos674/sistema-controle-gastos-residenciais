using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Data;

/// <summary>
/// Contexto de banco de dados da aplicação, ponto central de acesso ao PostgreSQL via EF Core.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Recebe as opções de configuração injetadas pelo contêiner DI.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Tabela <c>Usuarios</c>.</summary>
    public DbSet<Usuario> Usuarios { get; set; }

    /// <summary>Tabela <c>Pessoas</c>.</summary>
    public DbSet<Pessoa> Pessoas { get; set; }

    /// <summary>Tabela <c>Categorias</c>.</summary>
    public DbSet<Categoria> Categorias { get; set; }

    /// <summary>Tabela <c>Transacoes</c>.</summary>
    public DbSet<Transacao> Transacoes { get; set; }

    /// <summary>
    /// Configura o mapeamento objeto-relacional via Fluent API.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Login)
            .IsUnique();

        modelBuilder.Entity<Pessoa>()
            .HasOne(p => p.Usuario)
            .WithMany(u => u.Pessoas)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Categoria>()
            .HasOne(c => c.Usuario)
            .WithMany(u => u.Categorias)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transacao>()
            .HasOne(t => t.Pessoa)
            .WithMany(p => p.Transacoes)
            .HasForeignKey(t => t.PessoaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transacao>()
            .HasOne(t => t.Categoria)
            .WithMany(c => c.Transacoes)
            .HasForeignKey(t => t.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transacao>()
            .Property(t => t.Valor)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Transacao>()
            .Property(t => t.ValorReceita)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Transacao>()
            .Property(t => t.ValorDespesa)
            .HasColumnType("decimal(18,2)");
    }
}

using System.ComponentModel.DataAnnotations;

namespace GastosResidenciais.Api.Models;

/// <summary>
/// Entidade que representa um usuário autenticado do sistema.
/// Cada usuário possui suas próprias Pessoas e Categorias,
/// garantindo o isolamento de dados entre contas diferentes.
/// </summary>
public class Usuario
{
    /// <summary>Identificador único gerado automaticamente.</summary>
    public int Id { get; set; }

    /// <summary>
    /// Login do usuário (nome de usuário único no sistema).
    /// Deve ser único — garantido pelo índice no <see cref="Data.AppDbContext"/>.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Hash da senha gerado pelo BCrypt.
    /// A senha em texto puro nunca é armazenada.
    /// </summary>
    [Required]
    public string SenhaHash { get; set; } = string.Empty;

    /// <summary>Hash BCrypt do refresh token ativo, quando houver sessão válida.</summary>
    public string? RefreshToken { get; set; }

    /// <summary>Data/hora UTC de expiração do refresh token ativo.</summary>
    public DateTime? RefreshTokenExpiry { get; set; }

    /// <summary>Pessoas cadastradas por este usuário.</summary>
    public ICollection<Pessoa> Pessoas { get; set; } = new List<Pessoa>();

    /// <summary>Categorias cadastradas por este usuário.</summary>
    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
}

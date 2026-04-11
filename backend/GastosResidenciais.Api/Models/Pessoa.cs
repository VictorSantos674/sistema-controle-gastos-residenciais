using System.ComponentModel.DataAnnotations;

namespace GastosResidenciais.Api.Models;

/// <summary>
/// Entidade de domínio que representa uma pessoa cadastrada no sistema.
///
/// Toda transação financeira (despesa ou receita) é vinculada a uma pessoa,
/// permitindo os relatórios de totais por pessoa.
///
/// Regra de negócio associada: pessoas com Idade menor que 18 anos
/// só podem ter transações do tipo <see cref="TipoTransacao.Despesa"/>.
/// </summary>
public class Pessoa
{
    /// <summary>
    /// Identificador único gerado automaticamente pelo banco de dados (auto-increment).
    /// Nunca deve ser definido pelo cliente — é papel exclusivo do EF Core / SQLite.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome completo da pessoa. Campo obrigatório com limite de 200 caracteres
    /// conforme a especificação do sistema.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Idade da pessoa em anos. Utilizada para verificar a regra de menor de idade
    /// no momento do cadastro de transações.
    /// </summary>
    [Required]
    public int Idade { get; set; }

    /// <summary>
    /// Propriedade de navegação do EF Core: lista de transações desta pessoa.
    ///
    /// Configuração no <see cref="Data.AppDbContext"/>:
    ///   <c>DeleteBehavior.Cascade</c> — ao excluir a pessoa, todas as suas
    ///   transações são excluídas automaticamente pelo banco de dados,
    ///   garantindo integridade referencial sem necessidade de lógica extra na aplicação.
    /// </summary>
    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}

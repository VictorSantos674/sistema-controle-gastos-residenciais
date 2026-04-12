using System.ComponentModel.DataAnnotations;

namespace GastosResidenciais.Api.Models;

/// <summary>
/// Entidade de domínio que representa uma categoria para classificação de transações.
///
/// A <see cref="Finalidade"/> da categoria é o principal mecanismo de controle
/// de quais tipos de transação podem usá-la, implementando a seguinte regra:
///   - Finalidade <c>Despesa</c>  → aceita somente transações do tipo <c>Despesa</c>.
///   - Finalidade <c>Receita</c>  → aceita somente transações do tipo <c>Receita</c>.
///   - Finalidade <c>Ambas</c>    → aceita transações <c>Despesa</c>, <c>Receita</c> ou <c>Ambas</c>.
/// </summary>
public class Categoria
{
    /// <summary>
    /// Identificador único gerado automaticamente pelo banco de dados.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Descrição da categoria (ex.: "Alimentação", "Salário", "Aluguel").
    /// Obrigatória, máximo 400 caracteres conforme especificação.
    /// </summary>
    [Required]
    [MaxLength(400)]
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Define para quais tipos de transação esta categoria pode ser usada.
    /// Veja o enum <see cref="Models.Finalidade"/> para os valores possíveis.
    /// </summary>
    [Required]
    public Finalidade Finalidade { get; set; }

    /// <summary>
    /// Chave estrangeira para o <see cref="Usuario"/> dono desta categoria.
    /// Garante isolamento de dados: cada usuário vê apenas suas próprias categorias.
    /// </summary>
    [Required]
    public int UsuarioId { get; set; }

    /// <summary>Propriedade de navegação para o usuário dono.</summary>
    public Usuario Usuario { get; set; } = null!;

    /// <summary>
    /// Propriedade de navegação: transações vinculadas a esta categoria.
    ///
    /// Configuração no <see cref="Data.AppDbContext"/>:
    ///   <c>DeleteBehavior.Restrict</c> — impede a exclusão da categoria
    ///   enquanto existirem transações vinculadas, preservando o histórico financeiro.
    /// </summary>
    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}

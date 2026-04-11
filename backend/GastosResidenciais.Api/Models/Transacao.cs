using System.ComponentModel.DataAnnotations;

namespace GastosResidenciais.Api.Models;

/// <summary>
/// Entidade de domínio que representa uma transação financeira (entrada ou saída de dinheiro).
///
/// Vinculada a uma <see cref="Pessoa"/> e a uma <see cref="Categoria"/>.
/// O tipo determina como o valor impacta o saldo:
///   - <c>Despesa</c>: subtrai do saldo.
///   - <c>Receita</c>: soma ao saldo.
///   - <c>Ambas</c>:   registra receita e despesa separadamente (ex.: conta corrente com juros).
///
/// Regras de negócio validadas na camada de serviço:
///   1. Menores de 18 anos: apenas transações do tipo <c>Despesa</c>.
///   2. O tipo deve ser compatível com a <see cref="Categoria.Finalidade"/>.
///   3. O valor (ou valores para tipo <c>Ambas</c>) deve ser positivo.
/// </summary>
public class Transacao
{
    /// <summary>Identificador único gerado automaticamente.</summary>
    public int Id { get; set; }

    /// <summary>
    /// Descrição da transação (ex.: "Conta de luz", "Salário mensal").
    /// Obrigatória, máximo 400 caracteres.
    /// </summary>
    [Required]
    [MaxLength(400)]
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Valor monetário da transação.
    /// Para tipo <c>Ambas</c>, representa a diferença líquida (ValorReceita - ValorDespesa).
    /// Para tipos simples (Despesa/Receita), é o valor principal da transação.
    /// </summary>
    [Required]
    public decimal Valor { get; set; }

    /// <summary>
    /// Valor de receita — preenchido apenas quando o tipo é <c>Ambas</c>.
    /// Armazenado separadamente para permitir relatórios corretos por tipo.
    /// </summary>
    public decimal? ValorReceita { get; set; }

    /// <summary>
    /// Valor de despesa — preenchido apenas quando o tipo é <c>Ambas</c>.
    /// </summary>
    public decimal? ValorDespesa { get; set; }

    /// <summary>
    /// Tipo da transação. Determina o comportamento no saldo e
    /// quais categorias podem ser utilizadas.
    /// </summary>
    [Required]
    public TipoTransacao Tipo { get; set; }

    /// <summary>
    /// Chave estrangeira para a <see cref="Categoria"/> associada.
    /// A categoria deve ser compatível com o tipo da transação.
    /// </summary>
    [Required]
    public int CategoriaId { get; set; }

    /// <summary>Propriedade de navegação para a Categoria.</summary>
    public Categoria Categoria { get; set; } = null!;

    /// <summary>
    /// Chave estrangeira para a <see cref="Pessoa"/> dona desta transação.
    /// </summary>
    [Required]
    public int PessoaId { get; set; }

    /// <summary>Propriedade de navegação para a Pessoa.</summary>
    public Pessoa Pessoa { get; set; } = null!;

    /// <summary>
    /// Data em que a transação foi realizada.
    /// Padrão: data de hoje se não informada pelo cliente.
    /// Armazenado como TEXT no SQLite no formato ISO 8601 (yyyy-MM-dd).
    /// </summary>
    [Required]
    public DateOnly Data { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}

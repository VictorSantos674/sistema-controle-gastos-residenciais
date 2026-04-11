using System.ComponentModel.DataAnnotations;
using GastosResidenciais.Api.Models;

namespace GastosResidenciais.Api.DTOs;

/// <summary>
/// DTO de entrada para criação de uma Transação.
///
/// Para transações do tipo <c>Despesa</c> ou <c>Receita</c>, use apenas <see cref="Valor"/>.
/// Para transações do tipo <c>Ambas</c>, use <see cref="ValorReceita"/> e <see cref="ValorDespesa"/>.
///
/// As validações de negócio (menor de idade, compatibilidade de categoria, valor positivo)
/// são aplicadas na camada de serviço (<c>TransacaoService</c>), não aqui —
/// DTOs validam apenas o <i>formato</i> dos dados.
/// </summary>
public class TransacaoInputDto
{
    /// <summary>
    /// Descrição da transação (ex.: "Conta de luz de dezembro").
    /// Obrigatória, máximo 400 caracteres.
    /// </summary>
    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [MaxLength(400, ErrorMessage = "A descrição deve ter no máximo 400 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Valor para transações simples (Despesa ou Receita).
    /// Deve ser positivo — validado no serviço.
    /// </summary>
    public decimal? Valor { get; set; }

    /// <summary>
    /// Valor de receita para transações do tipo <c>Ambas</c>.
    /// Obrigatório e positivo quando o tipo for Ambas.
    /// </summary>
    public decimal? ValorReceita { get; set; }

    /// <summary>
    /// Valor de despesa para transações do tipo <c>Ambas</c>.
    /// Opcional — pode ser zero (ex.: só receita sem custo associado).
    /// </summary>
    public decimal? ValorDespesa { get; set; }

    /// <summary>
    /// Tipo da transação: 1 = Despesa, 2 = Receita, 3 = Ambas.
    /// </summary>
    [Required(ErrorMessage = "O tipo é obrigatório.")]
    public TipoTransacao Tipo { get; set; }

    /// <summary>
    /// ID da categoria associada. Deve ser compatível com o tipo da transação.
    /// A verificação de compatibilidade é feita no serviço.
    /// </summary>
    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public int CategoriaId { get; set; }

    /// <summary>
    /// ID da pessoa que realizou a transação.
    /// </summary>
    [Required(ErrorMessage = "A pessoa é obrigatória.")]
    public int PessoaId { get; set; }

    /// <summary>
    /// Data da transação no formato ISO 8601 (yyyy-MM-dd).
    /// Se não informada, o serviço usa a data de hoje.
    /// </summary>
    public DateOnly? Data { get; set; }
}

/// <summary>
/// DTO de saída para exibição de uma Transação.
///
/// Inclui <see cref="PessoaNome"/> e <see cref="CategoriaDescricao"/> desnormalizados
/// para evitar múltiplas requisições no cliente ao renderizar a listagem
/// (padrão conhecido como <i>eager loading no DTO</i>).
/// </summary>
public class TransacaoOutputDto
{
    /// <summary>Identificador único da transação.</summary>
    public int Id { get; set; }

    /// <summary>Descrição da transação.</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Valor principal. Para tipo <c>Ambas</c>, representa o saldo líquido
    /// (ValorReceita - ValorDespesa), que pode ser negativo.
    /// </summary>
    public decimal Valor { get; set; }

    /// <summary>Valor de receita (apenas para tipo Ambas).</summary>
    public decimal? ValorReceita { get; set; }

    /// <summary>Valor de despesa (apenas para tipo Ambas).</summary>
    public decimal? ValorDespesa { get; set; }

    /// <summary>Tipo como string: "Despesa", "Receita" ou "Ambas".</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>ID da categoria associada.</summary>
    public int CategoriaId { get; set; }

    /// <summary>Descrição da categoria (desnormalizado para exibição).</summary>
    public string CategoriaDescricao { get; set; } = string.Empty;

    /// <summary>ID da pessoa associada.</summary>
    public int PessoaId { get; set; }

    /// <summary>Nome da pessoa (desnormalizado para exibição).</summary>
    public string PessoaNome { get; set; } = string.Empty;

    /// <summary>Data da transação.</summary>
    public DateOnly Data { get; set; }
}

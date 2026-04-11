using System.ComponentModel.DataAnnotations;
using GastosResidenciais.Api.Models;

namespace GastosResidenciais.Api.DTOs;

/// <summary>
/// DTO de entrada para criação de uma Categoria.
///
/// A <see cref="Finalidade"/> é recebida como valor numérico do enum no JSON
/// (1 = Despesa, 2 = Receita, 3 = Ambas), e o ASP.NET Core desserializa
/// automaticamente para o tipo <see cref="Models.Finalidade"/>.
/// </summary>
public class CategoriaInputDto
{
    /// <summary>
    /// Descrição da categoria (ex.: "Alimentação", "Salário").
    /// Obrigatória, máximo 400 caracteres.
    /// </summary>
    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [MaxLength(400, ErrorMessage = "A descrição deve ter no máximo 400 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Finalidade da categoria:
    /// <list type="bullet">
    ///   <item><c>1 (Despesa)</c>  — aceita apenas transações Despesa.</item>
    ///   <item><c>2 (Receita)</c>  — aceita apenas transações Receita.</item>
    ///   <item><c>3 (Ambas)</c>    — aceita transações Despesa, Receita ou Ambas.</item>
    /// </list>
    /// </summary>
    [Required(ErrorMessage = "A finalidade é obrigatória.")]
    public Finalidade Finalidade { get; set; }
}

/// <summary>
/// DTO de saída para exibição de uma Categoria.
///
/// A <see cref="Finalidade"/> é retornada como string legível
/// ("Despesa", "Receita" ou "Ambas") em vez do valor numérico do enum,
/// facilitando a exibição no frontend sem mapeamento adicional.
/// </summary>
public class CategoriaOutputDto
{
    /// <summary>Identificador único da categoria.</summary>
    public int Id { get; set; }

    /// <summary>Descrição da categoria.</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Finalidade como string: "Despesa", "Receita" ou "Ambas".</summary>
    public string Finalidade { get; set; } = string.Empty;
}

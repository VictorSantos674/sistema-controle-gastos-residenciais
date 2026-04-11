using System.ComponentModel.DataAnnotations;

namespace GastosResidenciais.Api.DTOs;

/// <summary>
/// DTO de entrada para criação e edição de uma Pessoa.
///
/// <b>Por que usar DTOs em vez da entidade diretamente?</b>
/// <list type="bullet">
///   <item>Evita <i>over-posting</i>: o cliente não pode definir campos
///         como <c>Id</c> ou a coleção <c>Transacoes</c>.</item>
///   <item>Permite validações de formato independentes do domínio
///         (DataAnnotations aqui não poluem a entidade).</item>
///   <item>Desacopla o contrato da API da estrutura interna do banco.</item>
/// </list>
/// </summary>
public class PessoaInputDto
{
    /// <summary>
    /// Nome completo da pessoa.
    /// Obrigatório, máximo 200 caracteres (requisito da especificação).
    /// </summary>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O nome deve ter no máximo 200 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Idade da pessoa em anos. Deve estar entre 0 e 150.
    /// Impacta as regras de negócio de transações: menores de 18 só podem ter Despesas.
    /// </summary>
    [Required(ErrorMessage = "A idade é obrigatória.")]
    [Range(0, 150, ErrorMessage = "A idade deve ser um valor válido entre 0 e 150.")]
    public int Idade { get; set; }
}

/// <summary>
/// DTO de saída para exibição de dados de uma Pessoa.
///
/// Expõe apenas os campos relevantes para o cliente,
/// sem vazar detalhes internos como a lista de transações.
/// </summary>
public class PessoaOutputDto
{
    /// <summary>Identificador único da pessoa.</summary>
    public int Id { get; set; }

    /// <summary>Nome completo da pessoa.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Idade da pessoa.</summary>
    public int Idade { get; set; }
}

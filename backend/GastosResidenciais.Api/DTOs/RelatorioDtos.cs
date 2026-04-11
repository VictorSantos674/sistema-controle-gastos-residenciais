namespace GastosResidenciais.Api.DTOs;

/// <summary>
/// Totais financeiros consolidados de uma única Pessoa.
/// Calculados pelo <c>RelatorioService</c> somando as transações vinculadas.
/// </summary>
public class TotalPorPessoaDto
{
    /// <summary>Identificador da pessoa.</summary>
    public int PessoaId { get; set; }

    /// <summary>Nome da pessoa.</summary>
    public string NomePessoa { get; set; } = string.Empty;

    /// <summary>
    /// Soma de todas as transações do tipo <c>Receita</c>
    /// mais os <c>ValorReceita</c> de transações do tipo <c>Ambas</c>.
    /// </summary>
    public decimal TotalReceitas { get; set; }

    /// <summary>
    /// Soma de todas as transações do tipo <c>Despesa</c>
    /// mais os <c>ValorDespesa</c> de transações do tipo <c>Ambas</c>.
    /// </summary>
    public decimal TotalDespesas { get; set; }

    /// <summary>Saldo líquido: TotalReceitas - TotalDespesas. Pode ser negativo.</summary>
    public decimal Saldo { get; set; }
}

/// <summary>
/// Resposta completa do relatório por pessoa.
/// Contém os dados individuais de cada pessoa e os totais gerais consolidados.
///
/// Os totais gerais são a soma dos itens — calculados no serviço para
/// consistência, evitando que o cliente precise recalcular.
/// </summary>
public class RelatorioPorPessoaDto
{
    /// <summary>Lista de totais por pessoa, ordenada por nome.</summary>
    public IEnumerable<TotalPorPessoaDto> Pessoas { get; set; } = new List<TotalPorPessoaDto>();

    /// <summary>Soma de receitas de todas as pessoas.</summary>
    public decimal TotalGeralReceitas { get; set; }

    /// <summary>Soma de despesas de todas as pessoas.</summary>
    public decimal TotalGeralDespesas { get; set; }

    /// <summary>Saldo líquido geral de todas as pessoas.</summary>
    public decimal SaldoLiquido { get; set; }
}

/// <summary>
/// Totais financeiros consolidados de uma única Categoria.
/// </summary>
public class TotalPorCategoriaDto
{
    /// <summary>Identificador da categoria.</summary>
    public int CategoriaId { get; set; }

    /// <summary>Descrição da categoria.</summary>
    public string DescricaoCategoria { get; set; } = string.Empty;

    /// <summary>Finalidade como string ("Despesa", "Receita" ou "Ambas").</summary>
    public string Finalidade { get; set; } = string.Empty;

    /// <summary>Total de receitas nas transações desta categoria.</summary>
    public decimal TotalReceitas { get; set; }

    /// <summary>Total de despesas nas transações desta categoria.</summary>
    public decimal TotalDespesas { get; set; }

    /// <summary>Saldo líquido: TotalReceitas - TotalDespesas.</summary>
    public decimal Saldo { get; set; }
}

/// <summary>
/// Resposta completa do relatório por categoria, com totais gerais.
/// </summary>
public class RelatorioPorCategoriaDto
{
    /// <summary>Lista de totais por categoria, ordenada por descrição.</summary>
    public IEnumerable<TotalPorCategoriaDto> Categorias { get; set; } = new List<TotalPorCategoriaDto>();

    /// <summary>Soma de receitas de todas as categorias.</summary>
    public decimal TotalGeralReceitas { get; set; }

    /// <summary>Soma de despesas de todas as categorias.</summary>
    public decimal TotalGeralDespesas { get; set; }

    /// <summary>Saldo líquido geral de todas as categorias.</summary>
    public decimal SaldoLiquido { get; set; }
}

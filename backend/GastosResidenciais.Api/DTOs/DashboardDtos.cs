namespace GastosResidenciais.Api.DTOs;

/// <summary>
/// Resumo financeiro e de cadastros exibido no Dashboard.
/// </summary>
public class DashboardResumoDto
{
    /// <summary>Total histórico de receitas.</summary>
    public decimal TotalReceitas { get; set; }

    /// <summary>Total histórico de despesas.</summary>
    public decimal TotalDespesas { get; set; }

    /// <summary>Saldo histórico líquido.</summary>
    public decimal SaldoLiquido { get; set; }

    /// <summary>Total de receitas no mês corrente.</summary>
    public decimal ReceitasMes { get; set; }

    /// <summary>Total de despesas no mês corrente.</summary>
    public decimal DespesasMes { get; set; }

    /// <summary>Saldo líquido no mês corrente.</summary>
    public decimal SaldoMes { get; set; }

    /// <summary>Total de pessoas cadastradas pelo usuário.</summary>
    public int TotalPessoas { get; set; }

    /// <summary>Total de categorias cadastradas pelo usuário.</summary>
    public int TotalCategorias { get; set; }

    /// <summary>Total de transações cadastradas pelo usuário.</summary>
    public int TotalTransacoes { get; set; }
}

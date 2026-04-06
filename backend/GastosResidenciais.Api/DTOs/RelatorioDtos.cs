namespace GastosResidenciais.Api.DTOs;

public class TotalPorPessoaDto
{
    public int PessoaId { get; set; }
    public string NomePessoa { get; set; } = string.Empty;
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal Saldo { get; set; }
}

public class RelatorioPorPessoaDto
{
    public IEnumerable<TotalPorPessoaDto> Pessoas { get; set; } = new List<TotalPorPessoaDto>();
    public decimal TotalGeralReceitas { get; set; }
    public decimal TotalGeralDespesas { get; set; }
    public decimal SaldoLiquido { get; set; }
}

public class TotalPorCategoriaDto
{
    public int CategoriaId { get; set; }
    public string DescricaoCategoria { get; set; } = string.Empty;
    public string Finalidade { get; set; } = string.Empty;
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal Saldo { get; set; }
}

public class RelatorioPorCategoriaDto
{
    public IEnumerable<TotalPorCategoriaDto> Categorias { get; set; } = new List<TotalPorCategoriaDto>();
    public decimal TotalGeralReceitas { get; set; }
    public decimal TotalGeralDespesas { get; set; }
    public decimal SaldoLiquido { get; set; }
}

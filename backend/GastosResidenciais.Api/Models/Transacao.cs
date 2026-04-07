using System.ComponentModel.DataAnnotations;

namespace GastosResidenciais.Api.Models;

public class Transacao
{
    public int Id { get; set; }

    [Required]
    [MaxLength(400)]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    public decimal Valor { get; set; }

    public decimal? ValorReceita { get; set; }
    public decimal? ValorDespesa { get; set; }

    [Required]
    public TipoTransacao Tipo { get; set; }

    [Required]
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    [Required]
    public int PessoaId { get; set; }
    public Pessoa Pessoa { get; set; } = null!;

    [Required]
    public DateOnly Data { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}

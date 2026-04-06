using System.ComponentModel.DataAnnotations;

namespace GastosResidenciais.Api.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required]
    [MaxLength(400)]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    public Finalidade Finalidade { get; set; }

    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}

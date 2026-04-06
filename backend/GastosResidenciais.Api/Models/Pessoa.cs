using System.ComponentModel.DataAnnotations;

namespace GastosResidenciais.Api.Models;

public class Pessoa
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public int Idade { get; set; }
    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}

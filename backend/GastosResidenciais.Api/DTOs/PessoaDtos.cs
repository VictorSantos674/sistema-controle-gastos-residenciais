using System.ComponentModel.DataAnnotations;

namespace GastosResidenciais.Api.DTOs;

public class PessoaInputDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O nome deve ter no máximo 200 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A idade é obrigatória.")]
    [Range(0, 150, ErrorMessage = "A idade deve ser um valor válido.")]
    public int Idade { get; set; }
}
public class PessoaOutputDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Idade { get; set; }
}

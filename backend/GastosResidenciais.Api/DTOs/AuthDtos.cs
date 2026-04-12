using System.ComponentModel.DataAnnotations;

namespace GastosResidenciais.Api.DTOs;

/// <summary>Dados recebidos no cadastro de um novo usuário.</summary>
public class RegistrarDto
{
    [Required(ErrorMessage = "O login é obrigatório.")]
    [MaxLength(100)]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    public string Senha { get; set; } = string.Empty;
}

/// <summary>Dados recebidos no login.</summary>
public class LoginDto
{
    [Required(ErrorMessage = "O login é obrigatório.")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string Senha { get; set; } = string.Empty;
}

/// <summary>Resposta retornada após login ou cadastro bem-sucedido.</summary>
public class TokenDto
{
    /// <summary>JWT Bearer token para autenticação das requisições.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Login do usuário autenticado.</summary>
    public string Login { get; set; } = string.Empty;
}

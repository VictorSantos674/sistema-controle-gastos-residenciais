using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Contrato do serviço de autenticação.
/// Responsável por cadastro, login e geração de tokens JWT.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registra um novo usuário.
    /// Retorna o token JWT em caso de sucesso ou uma mensagem de erro se o login já estiver em uso.
    /// </summary>
    Task<(TokenDto? Resultado, string? Erro)> RegistrarAsync(RegistrarDto dto);

    /// <summary>
    /// Autentica um usuário existente.
    /// Retorna o token JWT em caso de sucesso ou erro se login/senha forem inválidos.
    /// </summary>
    Task<(TokenDto? Resultado, string? Erro)> LoginAsync(LoginDto dto);
}

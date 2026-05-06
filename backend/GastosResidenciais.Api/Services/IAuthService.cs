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
    /// Retorna access token e refresh token em caso de sucesso ou uma mensagem de erro se o login já estiver em uso.
    /// </summary>
    Task<(AuthResultDto? Resultado, string? Erro)> RegistrarAsync(RegistrarDto dto);

    /// <summary>
    /// Autentica um usuário existente.
    /// Retorna access token e refresh token em caso de sucesso ou erro se login/senha forem inválidos.
    /// </summary>
    Task<(AuthResultDto? Resultado, string? Erro)> LoginAsync(LoginDto dto);

    /// <summary>
    /// Valida e rotaciona o refresh token, retornando uma nova sessão.
    /// </summary>
    Task<(AuthResultDto? Resultado, string? Erro)> RefreshAsync(string? refreshToken);

    /// <summary>
    /// Invalida a sessão associada ao refresh token informado.
    /// </summary>
    Task LogoutAsync(string? refreshToken);
}

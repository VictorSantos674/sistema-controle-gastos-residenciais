using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Implementação do serviço de autenticação com JWT e BCrypt.
///
/// <b>Fluxo de registro:</b>
///   1. Verifica se o login já está em uso.
///   2. Faz hash da senha com BCrypt (salt aleatório).
///   3. Persiste o usuário.
///   4. Gera e retorna o JWT.
///
/// <b>Fluxo de login:</b>
///   1. Busca o usuário pelo login.
///   2. Verifica a senha com BCrypt.Verify.
///   3. Gera e retorna o JWT.
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config  = config;
    }

    /// <inheritdoc/>
    public async Task<(TokenDto? Resultado, string? Erro)> RegistrarAsync(RegistrarDto dto)
    {
        // Login deve ser único no sistema
        var existe = await _context.Usuarios.AnyAsync(u => u.Login == dto.Login);
        if (existe)
            return (null, "Este login já está em uso. Escolha outro.");

        var usuario = new Usuario
        {
            Login     = dto.Login,
            // BCrypt gera salt automático e produz um hash seguro de 60 caracteres
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return (GerarToken(usuario), null);
    }

    /// <inheritdoc/>
    public async Task<(TokenDto? Resultado, string? Erro)> LoginAsync(LoginDto dto)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Login == dto.Login);

        // Mensagem genérica: não informar ao atacante se o login existe ou não
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
            return (null, "Login ou senha inválidos.");

        return (GerarToken(usuario), null);
    }

    /// <summary>
    /// Gera um JWT com o ID e login do usuário como claims.
    /// Expiração: 7 dias (suficiente para uso contínuo sem necessidade de refresh token).
    /// </summary>
    private TokenDto GerarToken(Usuario usuario)
    {
        // Lê a chave secreta da configuração / variável de ambiente
        var jwtSecret = _config["JWT_SECRET"]
            ?? throw new InvalidOperationException("JWT_SECRET não configurado.");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            // NameIdentifier = ID numérico — usado para extrair o usuarioId nos controllers
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            // Name = login legível — exibido na interface
            new Claim(ClaimTypes.Name, usuario.Login)
        };

        var token = new JwtSecurityToken(
            issuer:             "GastosResidenciais",
            audience:           "GastosResidenciais",
            claims:             claims,
            expires:            DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new TokenDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Login = usuario.Login
        };
    }
}

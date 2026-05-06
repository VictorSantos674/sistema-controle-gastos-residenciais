using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Implementação do serviço de autenticação com JWT curto, refresh token opaco e BCrypt.
/// </summary>
public class AuthService : IAuthService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    /// <inheritdoc/>
    public async Task<(AuthResultDto? Resultado, string? Erro)> RegistrarAsync(RegistrarDto dto)
    {
        var existe = await _context.Usuarios.AnyAsync(u => u.Login == dto.Login);
        if (existe)
            return (null, "Este login já está em uso. Escolha outro.");

        var usuario = new Usuario
        {
            Login = dto.Login,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return (await GerarSessaoAsync(usuario), null);
    }

    /// <inheritdoc/>
    public async Task<(AuthResultDto? Resultado, string? Erro)> LoginAsync(LoginDto dto)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Login == dto.Login);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
            return (null, "Login ou senha inválidos.");

        return (await GerarSessaoAsync(usuario), null);
    }

    /// <inheritdoc/>
    public async Task<(AuthResultDto? Resultado, string? Erro)> RefreshAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return (null, "Sessão expirada. Faça login novamente.");

        var agora = DateTime.UtcNow;
        var candidatos = await _context.Usuarios
            .Where(u => u.RefreshToken != null && u.RefreshTokenExpiry != null && u.RefreshTokenExpiry > agora)
            .ToListAsync();

        var usuario = candidatos.FirstOrDefault(u =>
            BCrypt.Net.BCrypt.Verify(refreshToken, u.RefreshToken));

        if (usuario is null)
            return (null, "Sessão expirada. Faça login novamente.");

        return (await GerarSessaoAsync(usuario), null);
    }

    /// <inheritdoc/>
    public async Task LogoutAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var candidatos = await _context.Usuarios
            .Where(u => u.RefreshToken != null)
            .ToListAsync();

        var usuario = candidatos.FirstOrDefault(u =>
            BCrypt.Net.BCrypt.Verify(refreshToken, u.RefreshToken));

        if (usuario is null)
            return;

        usuario.RefreshToken = null;
        usuario.RefreshTokenExpiry = null;
        await _context.SaveChangesAsync();
    }

    private async Task<AuthResultDto> GerarSessaoAsync(Usuario usuario)
    {
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        usuario.RefreshToken = BCrypt.Net.BCrypt.HashPassword(refreshToken);
        usuario.RefreshTokenExpiry = DateTime.UtcNow.Add(RefreshTokenLifetime);

        await _context.SaveChangesAsync();

        return new AuthResultDto
        {
            Token = GerarAccessToken(usuario),
            Login = usuario.Login,
            RefreshToken = refreshToken
        };
    }

    private string GerarAccessToken(Usuario usuario)
    {
        var jwtSecret = _config["JWT_SECRET"]
            ?? throw new InvalidOperationException("JWT_SECRET não configurado.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Login)
        };

        var token = new JwtSecurityToken(
            issuer: "GastosResidenciais",
            audience: "GastosResidenciais",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

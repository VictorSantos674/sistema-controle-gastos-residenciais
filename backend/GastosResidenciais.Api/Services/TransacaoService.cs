using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Implementação das operações de <see cref="Transacao"/>.
/// Todas as queries são filtradas por <c>usuarioId</c> via navegação Pessoa -> Usuario.
/// </summary>
public class TransacaoService : ITransacaoService
{
    private readonly AppDbContext _context;

    public TransacaoService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponseDto<TransacaoOutputDto>> ListarAsync(
        int usuarioId,
        int page = 1,
        int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Transacoes
            .Where(t => t.Pessoa.UsuarioId == usuarioId)
            .OrderByDescending(t => t.Id);

        var total = await query.CountAsync();

        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransacaoOutputDto
            {
                Id = t.Id,
                Descricao = t.Descricao,
                Valor = t.Tipo == TipoTransacao.Ambas
                    ? (t.ValorReceita ?? 0) - (t.ValorDespesa ?? 0)
                    : t.Valor,
                ValorReceita = t.ValorReceita,
                ValorDespesa = t.ValorDespesa,
                Tipo = t.Tipo.ToString(),
                CategoriaId = t.CategoriaId,
                CategoriaDescricao = t.Categoria.Descricao,
                PessoaId = t.PessoaId,
                PessoaNome = t.Pessoa.Nome,
                Data = t.Data
            })
            .ToListAsync();

        return new PaginatedResponseDto<TransacaoOutputDto>
        {
            Data = data,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    /// <inheritdoc/>
    public async Task<(TransacaoOutputDto? Resultado, string? Erro)> CriarAsync(TransacaoInputDto dto, int usuarioId)
    {
        var (pessoa, categoria, erro) = await ObterEntidadesValidasAsync(dto, usuarioId);
        if (erro is not null)
            return (null, erro);

        var validacao = ValidarRegrasDeNegocio(dto, pessoa!, categoria!);
        if (validacao is not null)
            return (null, validacao);

        var transacao = new Transacao();
        AplicarDados(transacao, dto);

        _context.Transacoes.Add(transacao);
        await _context.SaveChangesAsync();

        return (MapearOutput(transacao, pessoa!, categoria!), null);
    }

    /// <inheritdoc/>
    public async Task<(TransacaoOutputDto? Resultado, string? Erro)> EditarAsync(int id, TransacaoInputDto dto, int usuarioId)
    {
        var transacao = await _context.Transacoes
            .Include(t => t.Pessoa)
            .FirstOrDefaultAsync(t => t.Id == id && t.Pessoa.UsuarioId == usuarioId);

        if (transacao is null)
            return (null, "Transação não encontrada.");

        var (pessoa, categoria, erro) = await ObterEntidadesValidasAsync(dto, usuarioId);
        if (erro is not null)
            return (null, erro);

        var validacao = ValidarRegrasDeNegocio(dto, pessoa!, categoria!);
        if (validacao is not null)
            return (null, validacao);

        AplicarDados(transacao, dto);
        await _context.SaveChangesAsync();

        return (MapearOutput(transacao, pessoa!, categoria!), null);
    }

    /// <inheritdoc/>
    public async Task<string?> DeletarAsync(int id, int usuarioId)
    {
        var transacao = await _context.Transacoes
            .Include(t => t.Pessoa)
            .FirstOrDefaultAsync(t => t.Id == id && t.Pessoa.UsuarioId == usuarioId);

        if (transacao is null)
            return "Transação não encontrada.";

        _context.Transacoes.Remove(transacao);
        await _context.SaveChangesAsync();
        return null;
    }

    private async Task<(Pessoa? Pessoa, Categoria? Categoria, string? Erro)> ObterEntidadesValidasAsync(
        TransacaoInputDto dto,
        int usuarioId)
    {
        var pessoa = await _context.Pessoas
            .FirstOrDefaultAsync(p => p.Id == dto.PessoaId && p.UsuarioId == usuarioId);
        if (pessoa is null)
            return (null, null, "Pessoa não encontrada.");

        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == dto.CategoriaId && c.UsuarioId == usuarioId);
        if (categoria is null)
            return (null, null, "Categoria não encontrada.");

        return (pessoa, categoria, null);
    }

    private static string? ValidarRegrasDeNegocio(TransacaoInputDto dto, Pessoa pessoa, Categoria categoria)
    {
        if (pessoa.Idade < 18 && dto.Tipo is TipoTransacao.Receita or TipoTransacao.Ambas)
            return "Menores de 18 anos só podem registrar transações do tipo Despesa.";

        if (!CategoriaCompativel(dto.Tipo, categoria.Finalidade))
            return $"A categoria '{categoria.Descricao}' não é compatível com o tipo '{dto.Tipo}'.";

        if (dto.Tipo == TipoTransacao.Ambas)
        {
            if (dto.ValorReceita is null || dto.ValorReceita <= 0)
                return "O valor de receita deve ser positivo.";

            if (dto.ValorDespesa is < 0)
                return "O valor de despesa não pode ser negativo.";

            return null;
        }

        if (dto.Valor is null || dto.Valor <= 0)
            return "O valor deve ser positivo.";

        return null;
    }

    private static bool CategoriaCompativel(TipoTransacao tipo, Finalidade finalidade) =>
        finalidade == Finalidade.Ambas ||
        tipo == TipoTransacao.Despesa && finalidade == Finalidade.Despesa ||
        tipo == TipoTransacao.Receita && finalidade == Finalidade.Receita;

    private static void AplicarDados(Transacao transacao, TransacaoInputDto dto)
    {
        transacao.Descricao = dto.Descricao;
        transacao.Tipo = dto.Tipo;
        transacao.CategoriaId = dto.CategoriaId;
        transacao.PessoaId = dto.PessoaId;
        transacao.Data = dto.Data ?? DateOnly.FromDateTime(DateTime.Today);

        if (dto.Tipo == TipoTransacao.Ambas)
        {
            transacao.Valor = 0;
            transacao.ValorReceita = dto.ValorReceita;
            transacao.ValorDespesa = dto.ValorDespesa ?? 0;
            return;
        }

        transacao.Valor = dto.Valor!.Value;
        transacao.ValorReceita = null;
        transacao.ValorDespesa = null;
    }

    private static TransacaoOutputDto MapearOutput(Transacao transacao, Pessoa pessoa, Categoria categoria) =>
        new()
        {
            Id = transacao.Id,
            Descricao = transacao.Descricao,
            Valor = transacao.Tipo == TipoTransacao.Ambas
                ? (transacao.ValorReceita ?? 0) - (transacao.ValorDespesa ?? 0)
                : transacao.Valor,
            ValorReceita = transacao.ValorReceita,
            ValorDespesa = transacao.ValorDespesa,
            Tipo = transacao.Tipo.ToString(),
            CategoriaId = transacao.CategoriaId,
            CategoriaDescricao = categoria.Descricao,
            PessoaId = transacao.PessoaId,
            PessoaNome = pessoa.Nome,
            Data = transacao.Data
        };
}

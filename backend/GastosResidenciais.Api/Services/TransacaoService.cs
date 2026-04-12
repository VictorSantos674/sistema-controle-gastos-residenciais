using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Implementação das operações de <see cref="Transacao"/>.
/// Todas as queries são filtradas por <c>usuarioId</c> via navegação Pessoa → Usuario.
/// </summary>
public class TransacaoService : ITransacaoService
{
    private readonly AppDbContext _context;

    public TransacaoService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TransacaoOutputDto>> ListarAsync(int usuarioId)
    {
        return await _context.Transacoes
            .Include(t => t.Pessoa)
            .Include(t => t.Categoria)
            .Where(t => t.Pessoa.UsuarioId == usuarioId)
            .OrderByDescending(t => t.Id)
            .Select(t => new TransacaoOutputDto
            {
                Id                 = t.Id,
                Descricao          = t.Descricao,
                Valor              = t.Tipo == TipoTransacao.Ambas
                    ? (t.ValorReceita ?? 0) - (t.ValorDespesa ?? 0)
                    : t.Valor,
                ValorReceita       = t.ValorReceita,
                ValorDespesa       = t.ValorDespesa,
                Tipo               = t.Tipo.ToString(),
                CategoriaId        = t.CategoriaId,
                CategoriaDescricao = t.Categoria.Descricao,
                PessoaId           = t.PessoaId,
                PessoaNome         = t.Pessoa.Nome,
                Data               = t.Data
            })
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<(TransacaoOutputDto? Resultado, string? Erro)> CriarAsync(TransacaoInputDto dto, int usuarioId)
    {
        // Valida que a Pessoa existe e pertence ao usuário atual
        var pessoa = await _context.Pessoas
            .FirstOrDefaultAsync(p => p.Id == dto.PessoaId && p.UsuarioId == usuarioId);
        if (pessoa is null)
            return (null, "Pessoa não encontrada.");

        // Valida que a Categoria existe e pertence ao usuário atual
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == dto.CategoriaId && c.UsuarioId == usuarioId);
        if (categoria is null)
            return (null, "Categoria não encontrada.");

        return dto.Tipo == TipoTransacao.Ambas
            ? await CriarTransacaoAmbas(dto, pessoa, categoria)
            : await CriarTransacaoSimples(dto, pessoa, categoria);
    }

    private async Task<(TransacaoOutputDto? Resultado, string? Erro)> CriarTransacaoAmbas(
        TransacaoInputDto dto, Pessoa pessoa, Categoria categoria)
    {
        if (pessoa.Idade < 18)
            return (null, "Menores de 18 anos só podem registrar transações do tipo Despesa.");

        if (categoria.Finalidade != Finalidade.Ambas)
            return (null, $"A categoria '{categoria.Descricao}' não é compatível com o tipo 'Ambas'.");

        if (dto.ValorReceita is null || dto.ValorReceita <= 0)
            return (null, "O valor de receita deve ser positivo.");

        var transacao = new Transacao
        {
            Descricao    = dto.Descricao,
            Valor        = 0,
            ValorReceita = dto.ValorReceita,
            ValorDespesa = dto.ValorDespesa,
            Tipo         = TipoTransacao.Ambas,
            CategoriaId  = dto.CategoriaId,
            PessoaId     = dto.PessoaId,
            Data         = dto.Data ?? DateOnly.FromDateTime(DateTime.Today)
        };

        return await PersistirERetornar(transacao, pessoa, categoria);
    }

    private async Task<(TransacaoOutputDto? Resultado, string? Erro)> CriarTransacaoSimples(
        TransacaoInputDto dto, Pessoa pessoa, Categoria categoria)
    {
        if (pessoa.Idade < 18 && dto.Tipo == TipoTransacao.Receita)
            return (null, "Menores de 18 anos só podem registrar transações do tipo Despesa.");

        var incompativel =
            dto.Tipo == TipoTransacao.Despesa && categoria.Finalidade == Finalidade.Receita ||
            dto.Tipo == TipoTransacao.Receita && categoria.Finalidade == Finalidade.Despesa;

        if (incompativel)
            return (null, $"A categoria '{categoria.Descricao}' não é compatível com o tipo '{dto.Tipo}'.");

        if (dto.Valor is null || dto.Valor <= 0)
            return (null, "O valor deve ser positivo.");

        var transacao = new Transacao
        {
            Descricao   = dto.Descricao,
            Valor       = dto.Valor.Value,
            Tipo        = dto.Tipo,
            CategoriaId = dto.CategoriaId,
            PessoaId    = dto.PessoaId,
            Data        = dto.Data ?? DateOnly.FromDateTime(DateTime.Today)
        };

        return await PersistirERetornar(transacao, pessoa, categoria);
    }

    private async Task<(TransacaoOutputDto Resultado, string? Erro)> PersistirERetornar(
        Transacao transacao, Pessoa pessoa, Categoria categoria)
    {
        _context.Transacoes.Add(transacao);
        await _context.SaveChangesAsync();

        return (new TransacaoOutputDto
        {
            Id                 = transacao.Id,
            Descricao          = transacao.Descricao,
            Valor              = transacao.Tipo == TipoTransacao.Ambas
                ? (transacao.ValorReceita ?? 0) - (transacao.ValorDespesa ?? 0)
                : transacao.Valor,
            ValorReceita       = transacao.ValorReceita,
            ValorDespesa       = transacao.ValorDespesa,
            Tipo               = transacao.Tipo.ToString(),
            CategoriaId        = transacao.CategoriaId,
            CategoriaDescricao = categoria.Descricao,
            PessoaId           = transacao.PessoaId,
            PessoaNome         = pessoa.Nome,
            Data               = transacao.Data
        }, null);
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
}

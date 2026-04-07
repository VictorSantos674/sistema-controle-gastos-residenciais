using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Services;

public class TransacaoService : ITransacaoService
{
    private readonly AppDbContext _context;

    public TransacaoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TransacaoOutputDto>> ListarAsync()
    {
        return await _context.Transacoes
            .Include(t => t.Pessoa)
            .Include(t => t.Categoria)
            .OrderByDescending(t => t.Id)
            .Select(t => new TransacaoOutputDto
            {
                Id = t.Id,
                Descricao = t.Descricao,
                /// Para transações do tipo Ambas, o Valor exibido é a diferença líquida (pode ser negativo).
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
    }

    public async Task<(TransacaoOutputDto? Resultado, string? Erro)> CriarAsync(TransacaoInputDto dto)
    {
        var pessoa = await _context.Pessoas.FindAsync(dto.PessoaId);
        if (pessoa is null)
            return (null, "Pessoa não encontrada.");

        var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);
        if (categoria is null)
            return (null, "Categoria não encontrada.");

        Transacao transacao;

        if (dto.Tipo == TipoTransacao.Ambas)
        {
            /// Menores de 18 anos não podem registrar transações do tipo Ambas.
            if (pessoa.Idade < 18)
                return (null, "Menores de 18 anos só podem registrar transações do tipo Despesa.");

            /// Transações do tipo Ambas só podem usar categorias com finalidade Ambas.
            if (categoria.Finalidade != Finalidade.Ambas)
                return (null, $"A categoria '{categoria.Descricao}' não é compatível com o tipo 'Ambas'.");

            if (dto.ValorReceita is null || dto.ValorReceita <= 0)
                return (null, "O valor de receita deve ser positivo.");

            transacao = new Transacao
            {
                Descricao = dto.Descricao,
                Valor = 0,
                ValorReceita = dto.ValorReceita,
                ValorDespesa = dto.ValorDespesa,
                Tipo = TipoTransacao.Ambas,
                CategoriaId = dto.CategoriaId,
                PessoaId = dto.PessoaId,
                Data = dto.Data ?? DateOnly.FromDateTime(DateTime.Today)
            };
        }
        else
        {
            /// Menores de 18 anos só podem registrar despesas.
            if (pessoa.Idade < 18 && dto.Tipo == TipoTransacao.Receita)
                return (null, "Menores de 18 anos só podem registrar transações do tipo Despesa.");

            /// Validação de compatibilidade entre tipo da transação e finalidade da categoria.
            var categoriaIncompativel = dto.Tipo == TipoTransacao.Despesa && categoria.Finalidade == Finalidade.Receita
                                     || dto.Tipo == TipoTransacao.Receita && categoria.Finalidade == Finalidade.Despesa;

            if (categoriaIncompativel)
                return (null, $"A categoria '{categoria.Descricao}' não é compatível com o tipo '{dto.Tipo}'.");

            if (dto.Valor is null || dto.Valor <= 0)
                return (null, "O valor deve ser positivo.");

            transacao = new Transacao
            {
                Descricao = dto.Descricao,
                Valor = dto.Valor.Value,
                Tipo = dto.Tipo,
                CategoriaId = dto.CategoriaId,
                PessoaId = dto.PessoaId,
                Data = dto.Data ?? DateOnly.FromDateTime(DateTime.Today)
            };
        }

        _context.Transacoes.Add(transacao);
        await _context.SaveChangesAsync();

        return (new TransacaoOutputDto
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
        }, null);
    }

    public async Task<string?> DeletarAsync(int id)
    {
        var transacao = await _context.Transacoes.FindAsync(id);
        if (transacao is null)
            return "Transação não encontrada.";

        _context.Transacoes.Remove(transacao);
        await _context.SaveChangesAsync();
        return null;
    }
}

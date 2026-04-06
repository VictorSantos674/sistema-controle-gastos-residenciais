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
                Valor = t.Valor,
                Tipo = t.Tipo.ToString(),
                CategoriaId = t.CategoriaId,
                CategoriaDescricao = t.Categoria.Descricao,
                PessoaId = t.PessoaId,
                PessoaNome = t.Pessoa.Nome
            })
            .ToListAsync();
    }

    public async Task<(TransacaoOutputDto? Resultado, string? Erro)> CriarAsync(TransacaoInputDto dto)
    {
        var pessoa = await _context.Pessoas.FindAsync(dto.PessoaId);
        if (pessoa is null)
            return (null, "Pessoa não encontrada.");

        /// Menores de 18 anos só podem registrar despesas.
        if (pessoa.Idade < 18 && dto.Tipo == TipoTransacao.Receita)
            return (null, "Menores de 18 anos só podem registrar transações do tipo Despesa.");

        var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);
        if (categoria is null)
            return (null, "Categoria não encontrada.");

        /// Validação de compatibilidade entre tipo da transação e finalidade da categoria.
        var categoriaIncompativel = dto.Tipo == TipoTransacao.Despesa && categoria.Finalidade == Finalidade.Receita
                                 || dto.Tipo == TipoTransacao.Receita && categoria.Finalidade == Finalidade.Despesa;

        if (categoriaIncompativel)
            return (null, $"A categoria '{categoria.Descricao}' não é compatível com o tipo '{dto.Tipo}'.");

        var transacao = new Transacao
        {
            Descricao = dto.Descricao,
            Valor = dto.Valor,
            Tipo = dto.Tipo,
            CategoriaId = dto.CategoriaId,
            PessoaId = dto.PessoaId
        };

        _context.Transacoes.Add(transacao);
        await _context.SaveChangesAsync();

        return (new TransacaoOutputDto
        {
            Id = transacao.Id,
            Descricao = transacao.Descricao,
            Valor = transacao.Valor,
            Tipo = transacao.Tipo.ToString(),
            CategoriaId = transacao.CategoriaId,
            CategoriaDescricao = categoria.Descricao,
            PessoaId = transacao.PessoaId,
            PessoaNome = pessoa.Nome
        }, null);
    }
}

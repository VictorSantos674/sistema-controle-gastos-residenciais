using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Implementação das operações de <see cref="Transacao"/>.
///
/// Esta é a classe com maior concentração de regras de negócio do sistema.
/// Toda regra é validada antes de persistir no banco — o banco é a última
/// linha de defesa (constraints), mas a lógica de negócio fica aqui.
///
/// <b>Organização do código:</b> o método público <see cref="CriarAsync"/> orquestra
/// o fluxo de criação delegando cada validação a um método privado específico,
/// mantendo a complexidade cognitiva baixa e o código legível.
/// </summary>
public class TransacaoService : ITransacaoService
{
    private readonly AppDbContext _context;

    public TransacaoService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TransacaoOutputDto>> ListarAsync()
    {
        // Include com duas entidades relacionadas: o EF Core gera JOINs eficientes.
        // Evita o problema N+1 (uma query por transação para buscar pessoa/categoria).
        return await _context.Transacoes
            .Include(t => t.Pessoa)
            .Include(t => t.Categoria)
            .OrderByDescending(t => t.Id) // Mais recentes primeiro
            .Select(t => new TransacaoOutputDto
            {
                Id                 = t.Id,
                Descricao          = t.Descricao,
                // Para tipo Ambas, o Valor exibido é o saldo líquido (pode ser negativo).
                // Para tipos simples, é o valor registrado diretamente.
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
    public async Task<(TransacaoOutputDto? Resultado, string? Erro)> CriarAsync(TransacaoInputDto dto)
    {
        // ── Valida existência das entidades referenciadas ─────────────────────
        var pessoa = await _context.Pessoas.FindAsync(dto.PessoaId);
        if (pessoa is null)
            return (null, "Pessoa não encontrada.");

        var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);
        if (categoria is null)
            return (null, "Categoria não encontrada.");

        // ── Delega a criação para o método especializado no tipo ──────────────
        // Separar em dois métodos reduz a complexidade cognitiva e facilita
        // a adição de novos tipos no futuro sem alterar o fluxo principal.
        return dto.Tipo == TipoTransacao.Ambas
            ? await CriarTransacaoAmbas(dto, pessoa, categoria)
            : await CriarTransacaoSimples(dto, pessoa, categoria);
    }

    /// <summary>
    /// Valida e cria uma transação do tipo <c>Ambas</c> (receita + despesa simultâneas).
    /// </summary>
    private async Task<(TransacaoOutputDto? Resultado, string? Erro)> CriarTransacaoAmbas(
        TransacaoInputDto dto, Pessoa pessoa, Categoria categoria)
    {
        // Menores de 18 não podem usar o tipo Ambas (inclui componente de receita).
        if (pessoa.Idade < 18)
            return (null, "Menores de 18 anos só podem registrar transações do tipo Despesa.");

        // Apenas categorias com finalidade Ambas são aceitas para transações do tipo Ambas.
        if (categoria.Finalidade != Finalidade.Ambas)
            return (null, $"A categoria '{categoria.Descricao}' não é compatível com o tipo 'Ambas'.");

        if (dto.ValorReceita is null || dto.ValorReceita <= 0)
            return (null, "O valor de receita deve ser positivo.");

        var transacao = new Transacao
        {
            Descricao    = dto.Descricao,
            Valor        = 0, // Para tipo Ambas, Valor é calculado dinamicamente na listagem
            ValorReceita = dto.ValorReceita,
            ValorDespesa = dto.ValorDespesa,
                Tipo = TipoTransacao.Ambas,
                CategoriaId = dto.CategoriaId,
                PessoaId = dto.PessoaId,
                Data = dto.Data ?? DateOnly.FromDateTime(DateTime.Today)
        };

        return await PersistirERetornar(transacao, pessoa, categoria);
    }

    /// <summary>
    /// Valida e cria uma transação simples (<c>Despesa</c> ou <c>Receita</c>).
    /// </summary>
    private async Task<(TransacaoOutputDto? Resultado, string? Erro)> CriarTransacaoSimples(
        TransacaoInputDto dto, Pessoa pessoa, Categoria categoria)
    {
        // Regra 1: Menor de 18 anos só pode criar Despesas.
        if (pessoa.Idade < 18 && dto.Tipo == TipoTransacao.Receita)
            return (null, "Menores de 18 anos só podem registrar transações do tipo Despesa.");

        // Regra 2: Compatibilidade tipo × finalidade da categoria.
        // Incompatível se: Tipo=Despesa com Finalidade=Receita, ou vice-versa.
        // Categoria com Finalidade=Ambas aceita qualquer tipo simples.
        var incompativel =
            dto.Tipo == TipoTransacao.Despesa && categoria.Finalidade == Finalidade.Receita ||
            dto.Tipo == TipoTransacao.Receita && categoria.Finalidade == Finalidade.Despesa;

        if (incompativel)
            return (null, $"A categoria '{categoria.Descricao}' não é compatível com o tipo '{dto.Tipo}'.");

        // Regra 3: Valor positivo.
        if (dto.Valor is null || dto.Valor <= 0)
            return (null, "O valor deve ser positivo.");

        var transacao = new Transacao
        {
                Descricao = dto.Descricao,
                Valor = dto.Valor.Value,
                Tipo = dto.Tipo,
            CategoriaId = dto.CategoriaId,
                PessoaId = dto.PessoaId,
                Data = dto.Data ?? DateOnly.FromDateTime(DateTime.Today)
        };

        return await PersistirERetornar(transacao, pessoa, categoria);
    }

    /// <summary>
    /// Persiste a transação no banco e retorna o DTO de saída com dados desnormalizados.
    /// Método compartilhado entre <see cref="CriarTransacaoAmbas"/> e <see cref="CriarTransacaoSimples"/>.
    /// </summary>
    private async Task<(TransacaoOutputDto Resultado, string? Erro)> PersistirERetornar(
        Transacao transacao, Pessoa pessoa, Categoria categoria)
    {
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

    /// <inheritdoc/>
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

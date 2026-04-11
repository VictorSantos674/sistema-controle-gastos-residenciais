using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Contrato da camada de serviço para operações com <c>Transacao</c>.
///
/// <b>Padrão Result Tuple:</b>
/// A operação de criação retorna uma tupla <c>(Resultado, Erro)</c>:
/// <list type="bullet">
///   <item>Sucesso: <c>(transação criada, null)</c></item>
///   <item>Falha de negócio: <c>(null, "mensagem de erro")</c></item>
/// </list>
///
/// Este padrão evita o uso de exceções para controle de fluxo esperado
/// (ex.: menor de idade tentando criar receita é uma situação previsível,
/// não uma situação excepcional). O controller mapeia o erro para 400 Bad Request.
/// </summary>
public interface ITransacaoService
{
    /// <summary>
    /// Retorna todas as transações em ordem decrescente de data,
    /// com nome da pessoa e descrição da categoria desnormalizados.
    /// </summary>
    Task<IEnumerable<TransacaoOutputDto>> ListarAsync();

    /// <summary>
    /// Cria uma nova transação aplicando todas as regras de negócio:
    /// <list type="number">
    ///   <item>Pessoa e Categoria devem existir.</item>
    ///   <item>Menores de 18 anos só podem criar transações do tipo <c>Despesa</c>.</item>
    ///   <item>O tipo da transação deve ser compatível com a finalidade da categoria.</item>
    ///   <item>Valor positivo (ou ValorReceita positivo para tipo <c>Ambas</c>).</item>
    /// </list>
    /// </summary>
    Task<(TransacaoOutputDto? Resultado, string? Erro)> CriarAsync(TransacaoInputDto dto);

    /// <summary>
    /// Deleta uma transação pelo ID.
    /// Retorna <c>null</c> em caso de sucesso ou a mensagem de erro se não encontrada.
    /// </summary>
    Task<string?> DeletarAsync(int id);
}

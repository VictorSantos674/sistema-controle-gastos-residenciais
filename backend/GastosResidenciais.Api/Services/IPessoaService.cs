using GastosResidenciais.Api.DTOs;

namespace GastosResidenciais.Api.Services;

/// <summary>
/// Contrato da camada de serviço para operações com <c>Pessoa</c>.
///
/// <b>Por que usar interface?</b>
/// <list type="bullet">
///   <item>Permite trocar a implementação (ex.: outro ORM, serviço externo)
///         sem alterar os controllers que dependem desta interface.</item>
///   <item>Facilita testes unitários: os controllers podem ser testados
///         com implementações mock sem necessidade de banco real.</item>
///   <item>Registrada no contêiner DI com <c>AddScoped</c>:
///         uma instância por requisição HTTP.</item>
/// </list>
/// </summary>
public interface IPessoaService
{
    /// <summary>Retorna todas as pessoas cadastradas, ordenadas por nome (A-Z).</summary>
    Task<IEnumerable<PessoaOutputDto>> ListarAsync();

    /// <summary>
    /// Retorna uma pessoa pelo seu ID.
    /// Retorna <c>null</c> se o ID não existir — o controller mapeia para 404.
    /// </summary>
    Task<PessoaOutputDto?> ObterPorIdAsync(int id);

    /// <summary>
    /// Cria e persiste uma nova pessoa no banco de dados.
    /// O ID é gerado automaticamente pelo banco.
    /// </summary>
    Task<PessoaOutputDto> CriarAsync(PessoaInputDto dto);

    /// <summary>
    /// Edita uma pessoa existente.
    /// Retorna <c>null</c> se o ID não existir — o controller mapeia para 404.
    /// </summary>
    Task<PessoaOutputDto?> EditarAsync(int id, PessoaInputDto dto);

    /// <summary>
    /// Deleta uma pessoa e, em cascata (configurado no banco), todas as suas transações.
    /// Retorna <c>false</c> se o ID não existir — o controller mapeia para 404.
    /// </summary>
    Task<bool> DeletarAsync(int id);
}

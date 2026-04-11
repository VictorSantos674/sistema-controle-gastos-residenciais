namespace GastosResidenciais.Api.Models;

/// <summary>
/// Define a finalidade de uma <see cref="Categoria"/>.
///
/// A finalidade é o contrato que determina quais tipos de transação
/// podem usar a categoria, evitando classificações inconsistentes.
///
/// Tabela de compatibilidade:
/// <code>
/// Finalidade da Categoria | TipoTransacao permitido
/// ------------------------|------------------------
/// Despesa                 | Despesa
/// Receita                 | Receita
/// Ambas                   | Despesa, Receita, Ambas
/// </code>
/// </summary>
public enum Finalidade
{
    Despesa = 1,
    Receita = 2,
    Ambas = 3
}

/// <summary>
/// Tipo de uma transação financeira.
///
/// - <c>Despesa</c>: saída de dinheiro; subtrai do saldo.
/// - <c>Receita</c>: entrada de dinheiro; soma ao saldo.
/// - <c>Ambas</c>:   registra simultaneamente receita e despesa
///   (ex.: transferência com taxa, refinanciamento). Utiliza os campos
///   <c>ValorReceita</c> e <c>ValorDespesa</c> separados.
///
/// Restrição de negócio: pessoas menores de 18 anos só podem
/// usar <c>Despesa</c>.
/// </summary>
public enum TipoTransacao
{
    Despesa = 1,
    Receita = 2,
    Ambas = 3
}

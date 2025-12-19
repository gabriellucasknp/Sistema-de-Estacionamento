// Repositories/IEstacionamentoRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaEstacionamento. Models;

namespace SistemaEstacionamento.Repositories
{
    /// <summary>
    /// Interface do repositório de estacionamento.
    /// Define operações de persistência para veículos. 
    /// </summary>
    public interface IEstacionamentoRepository
    {
        /// <summary>
        /// Adiciona um novo veículo (entrada no estacionamento).
        /// </summary>
        Task AddEntradaAsync(Veiculo veiculo);

        /// <summary>
        /// Obtém um veículo por placa (busca nos veículos atuais ou histórico).
        /// </summary>
        Task<Veiculo?> ObterVeiculoPorPlacaAsync(string placa);

        /// <summary>
        /// Registra a saída de um veículo (atualiza dados de saída).
        /// </summary>
        Task RegistrarSaidaAsync(Veiculo veiculo);

        /// <summary>
        /// Lista todos os veículos (histórico completo).
        /// </summary>
        Task<IReadOnlyCollection<Veiculo>> ListarTodosAsync();

        /// <summary>
        /// Conta quantos veículos estão atualmente no pátio (sem saída registrada).
        /// </summary>
        Task<int> ContarAtuaisAsync();

        /// <summary>
        /// Verifica se existe um veículo com a placa especificada no pátio.
        /// </summary>
        Task<bool> ExisteVeiculoNoPatioAsync(string placa);
    }
}

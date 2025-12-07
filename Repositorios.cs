// Repositories/InMemoryEstacionamentoRepository.cs
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SistemaEstacionamento.Models;
using System;

namespace SistemaEstacionamento.Repositories
{
    /// <summary>
    /// Implementação em memória do repositório. Thread-safe.
    /// Para produção, substitua por implementação EF Core.
    /// </summary>
    public class InMemoryEstacionamentoRepository : IEstacionamentoRepository
    {
        // Veículos atualmente no pátio (chave: placa uppercase)
        private readonly ConcurrentDictionary<string, Veiculo> _veiculosAtuais = new();

        // Histórico completo de entradas/saídas
        private readonly ConcurrentBag<Veiculo> _historico = new();

        public Task AddEntradaAsync(Veiculo veiculo)
        {
            var key = veiculo.Placa.ToUpperInvariant();
            _veiculosAtuais[key] = veiculo;
            _historico.Add(veiculo);
            return Task.CompletedTask;
        }

        public Task<Veiculo?> ObterVeiculoPorPlacaAsync(string placa)
        {
            var key = placa.ToUpperInvariant();
            if (_veiculosAtuais.TryGetValue(key, out var v))
                return Task.FromResult<Veiculo?>(v);

            // Caso esteja no histórico e já saiu, retorna o registro histórico mais recente
            var hist = _historico.LastOrDefault(x => x.Placa.Equals(placa, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult<Veiculo?>(hist);
        }

        public Task RegistrarSaidaAsync(Veiculo veiculo)
        {
            var key = veiculo.Placa.ToUpperInvariant();
            _veiculosAtuais.TryRemove(key, out _);
            // A instância já está no histórico quando entrou; atualizamos os campos de saída
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Veiculo>> ListarTodosAsync()
        {
            // Retorna o histórico (entradas + saídas)
            return Task.FromResult<IReadOnlyCollection<Veiculo>>(_historico.ToArray());
        }

        public Task<int> ContarAtuaisAsync()
        {
            return Task.FromResult(_veiculosAtuais.Count);
        }

        public Task<bool> ExisteVeiculoNoPatioAsync(string placa)
        {
            var key = placa.ToUpperInvariant();
            return Task.FromResult(_veiculosAtuais.ContainsKey(key));
        }
    }
}

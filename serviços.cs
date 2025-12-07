// Services/EstacionamentoService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SistemaEstacionamento.Models;
using SistemaEstacionamento.Options;
using SistemaEstacionamento.Repositories;

namespace SistemaEstacionamento.Services
{
    /// <summary>
    /// Camada de serviço que contém regras de negócio do estacionamento.
    /// </summary>
    public class EstacionamentoService
    {
        private readonly IEstacionamentoRepository _repo;
        private readonly EstacionamentoOptions _options;
        private readonly object _lock = new(); // proteção simples de concorrência

        public EstacionamentoService(IEstacionamentoRepository repo, EstacionamentoOptions options)
        {
            _repo = repo;
            _options = options;
        }

        /// <summary>
        /// Registra a entrada de um veículo. Lança InvalidOperationException em caso de erro (capacidade cheia, duplicata).
        /// </summary>
        public async Task RegistrarEntradaAsync(string placa, string modelo)
        {
            placa = placa.Trim().ToUpperInvariant();

            lock (_lock)
            {
                // Verifica capacidade
                var atuaisTask = _repo.ContarAtuaisAsync();
                atuaisTask.Wait();
                if (atuaisTask.Result >= _options.Capacidade)
                {
                    throw new InvalidOperationException("Estacionamento sem vagas disponíveis.");
                }

                // Verifica se placa já está dentro
                var existeTask = _repo.ExisteVeiculoNoPatioAsync(placa);
                existeTask.Wait();
                if (existeTask.Result)
                {
                    throw new InvalidOperationException("Veículo com essa placa já está no estacionamento.");
                }

                // Cria entidade e persiste
                var veic = new Veiculo
                {
                    Placa = placa,
                    Modelo = modelo,
                    EntradaUtc = DateTime.UtcNow
                };

                _repo.AddEntradaAsync(veic).Wait();
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Registra saída do veículo, calcula tarifa e marca como pago (ou não).
        /// Retorna o valor cobrado.
        /// </summary>
        public async Task<decimal> RegistrarSaidaAsync(string placa)
        {
            placa = placa.Trim().ToUpperInvariant();

            Veiculo? veic = null;
            lock (_lock)
            {
                var tarefa = _repo.ObterVeiculoPorPlacaAsync(placa);
                tarefa.Wait();
                veic = tarefa.Result;

                if (veic == null || veic.SaidaUtc != null)
                {
                    throw new InvalidOperationException("Veículo não encontrado no pátio.");
                }

                // Calcular tarifa
                var saida = DateTime.UtcNow;
                var valor = CalcularTarifa(veic.EntradaUtc, saida);

                // Atualizar entidade
                veic.SaidaUtc = saida;
                veic.ValorPago = valor;
                veic.Pago = true;

                _repo.RegistrarSaidaAsync(veic).Wait();
            }

            return await Task.FromResult(veic.ValorPago);
        }

        /// <summary>
        /// Calcula vagas disponíveis.
        /// </summary>
        public async Task<int> VagasDisponiveisAsync()
        {
            var atuais = await _repo.ContarAtuaisAsync();
            return Math.Max(0, _options.Capacidade - atuais);
        }

        /// <summary>
        /// Lista histórico completo de veículos.
        /// </summary>
        public async Task<IReadOnlyCollection<Veiculo>> ListarVeiculosAsync()
        {
            return await _repo.ListarTodosAsync();
        }

        /// <summary>
        /// Cálculo simples de tarifa por horas (arredonda para cima cada hora parcial).
        /// </summary>
        private decimal CalcularTarifa(DateTime entradaUtc, DateTime saidaUtc)
        {
            var horas = (saidaUtc - entradaUtc).TotalHours;
            var horasCobrar = (int)Math.Ceiling(Math.Max(0.0, horas)); // pelo menos 0
            return horasCobrar * _options.TarifaPorHora;
        }
    }
}

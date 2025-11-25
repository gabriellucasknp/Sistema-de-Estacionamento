
// Program.cs
using System;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using SistemaEstacionamento.Models;
using SistemaEstacionamento.Repositories;
using SistemaEstacionamento.Services;
using SistemaEstacionamento.Options;

namespace SistemaEstacionamento
{
    class Program
    {
        private static EstacionamentoService? _service;
        private static EstacionamentoOptions _options = new();

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Sistema de Estacionamento ===\n");

            // Carregar configurações do appsettings.json
            CarregarConfiguracoes();

            // Inicializar repositório e serviço
            var repository = new InMemoryEstacionamentoRepository();
            _service = new EstacionamentoService(repository, _options);

            Console.WriteLine($"Capacidade: {_options.Capacidade} vagas");
            Console.WriteLine($"Tarifa por hora: R$ {_options.TarifaPorHora:F2}\n");

            // Menu principal
            await ExibirMenu();
        }

        private static void CarregarConfiguracoes()
        {
            try
            {
                var jsonPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (File.Exists(jsonPath))
                {
                    var jsonString = File.ReadAllText(jsonPath);
                    using var document = JsonDocument.Parse(jsonString);
                    
                    if (document.RootElement.TryGetProperty("Estacionamento", out var estacionamentoConfig))
                    {
                        if (estacionamentoConfig.TryGetProperty("Capacidade", out var capacidade))
                            _options.Capacidade = capacidade.GetInt32();
                        
                        if (estacionamentoConfig.TryGetProperty("TarifaPorHora", out var tarifa))
                            _options.TarifaPorHora = tarifa.GetDecimal();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Aviso: Não foi possível carregar configurações. Usando valores padrão. ({ex.Message})");
            }
        }

        private static async Task ExibirMenu()
        {
            bool continuar = true;

            while (continuar)
            {
                Console.WriteLine("\n--- MENU PRINCIPAL ---");
                Console.WriteLine("1. Registrar Entrada de Veículo");
                Console.WriteLine("2. Registrar Saída de Veículo");
                Console.WriteLine("3. Consultar Vagas Disponíveis");
                Console.WriteLine("4. Listar Todos os Veículos");
                Console.WriteLine("5. Buscar Veículo por Placa");
                Console.WriteLine("0. Sair");
                Console.Write("\nEscolha uma opção: ");

                var opcao = Console.ReadLine();

                try
                {
                    switch (opcao)
                    {
                        case "1":
                            await RegistrarEntrada();
                            break;
                        case "2":
                            await RegistrarSaida();
                            break;
                        case "3":
                            await ConsultarVagas();
                            break;
                        case "4":
                            await ListarVeiculos();
                            break;
                        case "5":
                            await BuscarPorPlaca();
                            break;
                        case "0":
                            continuar = false;
                            Console.WriteLine("\nEncerrando sistema. Até logo!");
                            break;
                        default:
                            Console.WriteLine("\nOpção inválida!");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nErro: {ex.Message}");
                }
            }
        }

        private static async Task RegistrarEntrada()
        {
            Console.WriteLine("\n--- REGISTRAR ENTRADA ---");
            Console.Write("Placa do veículo: ");
            var placa = Console.ReadLine()?.Trim().ToUpper() ?? "";

            Console.Write("Modelo do veículo: ");
            var modelo = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(placa) || string.IsNullOrWhiteSpace(modelo))
            {
                Console.WriteLine("\nPlaca e modelo são obrigatórios!");
                return;
            }

            try
            {
                await _service!.RegistrarEntradaAsync(placa, modelo);
                Console.WriteLine($"\n✓ Entrada registrada com sucesso!");
                Console.WriteLine($"Veículo: {modelo}");
                Console.WriteLine($"Placa: {placa}");
                Console.WriteLine($"Horário: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"\n✗ Erro: {ex.Message}");
            }
        }

        private static async Task RegistrarSaida()
        {
            Console.WriteLine("\n--- REGISTRAR SAÍDA ---");
            Console.Write("Placa do veículo: ");
            var placa = Console.ReadLine()?.Trim().ToUpper() ?? "";

            if (string.IsNullOrWhiteSpace(placa))
            {
                Console.WriteLine("\nPlaca é obrigatória!");
                return;
            }

            try
            {
                var valor = await _service!.RegistrarSaidaAsync(placa);
                Console.WriteLine($"\n✓ Saída registrada com sucesso!");
                Console.WriteLine($"Placa: {placa}");
                Console.WriteLine($"Horário: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                Console.WriteLine($"Valor a pagar: R$ {valor:F2}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"\n✗ Erro: {ex.Message}");
            }
        }

        private static async Task ConsultarVagas()
        {
            Console.WriteLine("\n--- VAGAS DISPONÍVEIS ---");
            var vagas = await _service!.VagasDisponiveisAsync();
            var ocupadas = _options.Capacidade - vagas;

            Console.WriteLine($"Vagas disponíveis: {vagas}/{_options.Capacidade}");
            Console.WriteLine($"Vagas ocupadas: {ocupadas}/{_options.Capacidade}");
            
            var percentual = (_options.Capacidade > 0) ? (ocupadas * 100.0 / _options.Capacidade) : 0;
            Console.WriteLine($"Ocupação: {percentual:F1}%");
        }

        private static async Task ListarVeiculos()
        {
            Console.WriteLine("\n--- HISTÓRICO DE VEÍCULOS ---");
            var veiculos = await _service!.ListarVeiculosAsync();

            if (veiculos.Count == 0)
            {
                Console.WriteLine("Nenhum veículo registrado.");
                return;
            }

            Console.WriteLine($"\nTotal de registros: {veiculos.Count}\n");
            Console.WriteLine("{0,-12} {1,-20} {2,-20} {3,-20} {4,-10} {5}",
                "Placa", "Modelo", "Entrada", "Saída", "Valor", "Status");
            Console.WriteLine(new string('-', 110));

            foreach (var v in veiculos)
            {
                var entrada = v.EntradaUtc.ToLocalTime().ToString("dd/MM/yy HH:mm");
                var saida = v.SaidaUtc?.ToLocalTime().ToString("dd/MM/yy HH:mm") ?? "-";
                var status = v.SaidaUtc == null ? "No pátio" : "Finalizado";
                
                Console.WriteLine("{0,-12} {1,-20} {2,-20} {3,-20} {4,-10:C} {5}",
                    v.Placa,
                    v.Modelo.Length > 18 ? v.Modelo.Substring(0, 18) : v.Modelo,
                    entrada,
                    saida,
                    v.ValorPago,
                    status);
            }
        }

        private static async Task BuscarPorPlaca()
        {
            Console.WriteLine("\n--- BUSCAR VEÍCULO ---");
            Console.Write("Placa do veículo: ");
            var placa = Console.ReadLine()?.Trim().ToUpper() ?? "";

            if (string.IsNullOrWhiteSpace(placa))
            {
                Console.WriteLine("\nPlaca é obrigatória!");
                return;
            }

            var veiculos = await _service!.ListarVeiculosAsync();
            var encontrados = veiculos.Where(v => v.Placa.Equals(placa, StringComparison.OrdinalIgnoreCase)).ToList();

            if (encontrados.Count == 0)
            {
                Console.WriteLine($"\nNenhum veículo encontrado com a placa {placa}.");
                return;
            }

            Console.WriteLine($"\n{encontrados.Count} registro(s) encontrado(s) para a placa {placa}:\n");

            foreach (var v in encontrados)
            {
                Console.WriteLine("─────────────────────────────────");
                Console.WriteLine($"Placa: {v.Placa}");
                Console.WriteLine($"Modelo: {v.Modelo}");
                Console.WriteLine($"Entrada: {v.EntradaUtc.ToLocalTime():dd/MM/yyyy HH:mm:ss}");
                
                if (v.SaidaUtc.HasValue)
                {
                    Console.WriteLine($"Saída: {v.SaidaUtc.Value.ToLocalTime():dd/MM/yyyy HH:mm:ss}");
                    var permanencia = v.SaidaUtc.Value - v.EntradaUtc;
                    Console.WriteLine($"Permanência: {permanencia.TotalHours:F2} horas");
                    Console.WriteLine($"Valor pago: R$ {v.ValorPago:F2}");
                    Console.WriteLine($"Status: Finalizado");
                }
                else
                {
                    var permanencia = DateTime.UtcNow - v.EntradaUtc;
                    Console.WriteLine($"Permanência atual: {permanencia.TotalHours:F2} horas");
                    Console.WriteLine($"Status: No pátio");
                }
            }
            Console.WriteLine("─────────────────────────────────");
        }
    }
}


using System;
using System.Globalization;

namespace SistemaDeEstacionamento
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Sistema de Estacionamento ===");

            decimal precoInicial = ReadDecimalWithDefault("Preço inicial (ex: 2.50) [padrão 2.50]: ", 2.50m);
            decimal precoPorHora = ReadDecimalWithDefault("Preço por hora (ex: 1.75) [padrão 1.75]: ", 1.75m);

            // Usa a classe Estacionamento já existente no projeto
            var estacionamento = new Estacionamento(precoInicial, precoPorHora);

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("1 - Estacionar veículo");
                Console.WriteLine("2 - Retirar veículo");
                Console.WriteLine("3 - Listar veículos");
                Console.WriteLine("4 - Sair");
                Console.Write("Escolha uma opção: ");
                string opc = Console.ReadLine();

                switch (opc)
                {
                    case "1":
                        Console.Write("Placa: ");
                        string placa = Console.ReadLine()?.Trim().ToUpperInvariant();
                        Console.Write("Modelo (opcional): ");
                        string modelo = Console.ReadLine()?.Trim();
                        if (string.IsNullOrWhiteSpace(placa))
                        {
                            Console.WriteLine("Placa inválida.");
                            break;
                        }
                        try
                        {
                            estacionamento.AdicionarVeiculo(placa, modelo);
                            Console.WriteLine($"Veículo {placa} estacionado com sucesso.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao estacionar: {ex.Message}");
                        }
                        break;

                    case "2":
                        Console.Write("Placa a retirar: ");
                        string placaRet = Console.ReadLine()?.Trim().ToUpperInvariant();
                        if (string.IsNullOrWhiteSpace(placaRet))
                        {
                            Console.WriteLine("Placa inválida.");
                            break;
                        }
                        var resultado = estacionamento.RemoverVeiculo(placaRet);
                        // Supondo que RemoverVeiculo retorne um objeto com FoiRemovido, TempoEstacionado e ValorTotal
                        if (resultado != null && (resultado.FoiRemovido || resultado is bool && (bool)resultado == true))
                        {
                            // Se for objeto com propriedades:
                            if (resultado is dynamic r)
                            {
                                Console.WriteLine($"Veículo {placaRet} removido.");
                                Console.WriteLine($"Tempo estacionado: {r.TempoEstacionado.TotalMinutes:N0} minutos ({Math.Ceiling(r.TempoEstacionado.TotalHours)} horas cobradas).");
                                Console.WriteLine($"Total a pagar: R$ {r.ValorTotal:F2}");
                            }
                            else
                            {
                                Console.WriteLine($"Veículo {placaRet} removido.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Veículo com placa {placaRet} não encontrado.");
                        }
                        break;

                    case "3":
                        estacionamento.ListarVeiculos();
                        break;

                    case "4":
                        Console.WriteLine("Encerrando...");
                        return;

                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }
            }
        }

        static decimal ReadDecimalWithDefault(string prompt, decimal defaultValue)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;

            input = input.Trim().Replace(',', '.');
            if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
                return val;

            Console.WriteLine("Entrada inválida, usando valor padrão.");
            return defaultValue;
        }
    }
}
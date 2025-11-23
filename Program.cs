using System;
using System.Collections.Generic;
using System.Globalization;

namespace SistemaDeEstacionamento
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Sistema de Estacionamento ===");

            // Solicita preços ao iniciar (padrões se o usuário apertar Enter)
            decimal precoInicial = ReadDecimalWithDefault("Preço inicial (ex: 2.50) [padrão 2.50]: ", 2.50m);
            decimal precoPorHora = ReadDecimalWithDefault("Preço por hora (ex: 1.75) [padrão 1.75]: ", 1.75m);

            Estacionamento estacionamento = new Estacionamento(precoInicial, precoPorHora);

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
                        estacionamento.AdicionarVeiculo(placa, modelo);
                        Console.WriteLine($"Veículo {placa} estacionado com sucesso.");
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
                        if (resultado.FoiRemovido)
                        {
                            Console.WriteLine($"Veículo {placaRet} removido.");
                            Console.WriteLine($"Tempo estacionado: {resultado.TempoEstacionado.TotalMinutes:N0} minutos ({Math.Ceiling(resultado.TempoEstacionado.TotalHours)} horas cobradas).");
                            Console.WriteLine($"Total a pagar: R$ {resultado.ValorTotal:F2}");
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

            if (decimal.TryParse(input.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
                return val;

            Console.WriteLine("Entrada inválida, usando valor padrão.");
            return defaultValue;
        }
    }

    // Classe que representa um veículo no pátio
    class Veiculo
    {
        public string Placa { get; }
        public string Modelo { get; }
        public DateTime Entrada { get; }

        public Veiculo(string placa, string modelo)
        {
            Placa = placa;
            Modelo = modelo;
            Entrada = DateTime.Now;
        }
    }

    // Resultado da remoção (para informar tempo e valor)
    class RemocaoResultado
    {
        public bool FoiRemovido { get; set; }
        public TimeSpan TempoEstacionado { get; set; }
        public decimal ValorTotal { get; set; }
    }

    // Classe do estacionamento (armazena em memória)
    class Estacionamento
    {
        private readonly decimal precoInicial;
        private readonly decimal precoPorHora;
        private readonly List<Veiculo> veiculos = new List<Veiculo>();

        public Estacionamento(decimal precoInicial, decimal precoPorHora)
        {
            this.precoInicial = precoInicial;
            this.precoPorHora = precoPorHora;
        }

        public void AdicionarVeiculo(string placa, string modelo)
        {
            // Evita duplicatas pela placa
            if (veiculos.Exists(v => v.Placa.Equals(placa, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Veículo com essa placa já está estacionado.");

            veiculos.Add(new Veiculo(placa, modelo));
        }

        public RemocaoResultado RemoverVeiculo(string placa)
        {
            var resultado = new RemocaoResultado { FoiRemovido = false };

            var v = veiculos.Find(x => x.Placa.Equals(placa, StringComparison.OrdinalIgnoreCase));
            if (v == null)
                return resultado;

            TimeSpan tempo = DateTime.Now - v.Entrada;
            double horas = Math.Ceiling(tempo.TotalHours);
            decimal valor = precoInicial + (decimal)horas * precoPorHora;

            veiculos.Remove(v);

            resultado.FoiRemovido = true;
            resultado.TempoEstacionado = tempo;
            resultado.ValorTotal = valor;
            return resultado;
        }

        public void ListarVeiculos()
        {
            if (veiculos.Count == 0)
            {
                Console.WriteLine("Nenhum veículo estacionado.");
                return;
            }

            Console.WriteLine("Veículos estacionados:");
            foreach (var v in veiculos)
            {
                Console.WriteLine($"- {v.Placa} | {v.Modelo ?? "-"} | Entrada: {v.Entrada:dd/MM/yyyy HH:mm}");
            }
        }
    }
}

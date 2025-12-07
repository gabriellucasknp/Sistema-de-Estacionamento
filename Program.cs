using SistemaEstacionamento.Models;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Estacionamento estacionamento = new();

// Carrega dados salvos anteriormente
estacionamento.CarregarDados();

while (true)
{
    Console.WriteLine("\n--- SISTEMA DE ESTACIONAMENTO ---");
    Console.WriteLine("1 - Registrar entrada ");
    Console.WriteLine("2 - Registrar saída ");
    Console.WriteLine("3 - Ver vagas disponíveis ");
    Console.WriteLine("4 - Listar veículos ");
    Console.WriteLine("0 - Sair");
    Console.Write("Escolha: ");

    string? opcao = Console.ReadLine();

    switch (opcao)
    {
        case "1":
            Console.Write("Placa: ");
            string? placaEntrada = Console.ReadLine();

            Console.Write("Modelo: ");
            string? modelo = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(placaEntrada) || string.IsNullOrWhiteSpace(modelo))
            {
                Console.WriteLine("❌ Placa e modelo são obrigatórios!");
                break;
            }

            if (estacionamento.VagasDisponiveis() <= 0)
            {
                Console.WriteLine("❌ Não há vagas disponíveis!");
                break;
            }

            estacionamento.RegistrarEntrada(new Veiculo
            {
                Placa = placaEntrada.ToUpper().Trim(),
                Modelo = modelo.Trim(),
                Entrada = DateTime.Now
            });

            Console.WriteLine("✅ Veículo registrado!");
            break;

        case "2":
            Console.Write("Placa: ");
            string? placaSaida = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(placaSaida))
            {
                Console.WriteLine("❌ Placa é obrigatória!");
                break;
            }

            if (estacionamento.RegistrarSaida(placaSaida.ToUpper().Trim()))
            {
                var veiculo = estacionamento.Veiculos.FirstOrDefault(v => v.Placa == placaSaida.ToUpper().Trim());
                if (veiculo != null)
                {
                    var tempo = veiculo.Saida!.Value - veiculo.Entrada;
                    Console.WriteLine($"✅ Saída registrada!");
                    Console.WriteLine($"⏱️  Tempo: {tempo.Hours}h {tempo.Minutes}min");
                    Console.WriteLine($"💰 Valor: R$ {veiculo.ValorPago:F2}");
                }
            }
            else
            {
                Console.WriteLine("❌ Veículo não encontrado ou já teve saída registrada!");
            }
            break;

        case "3":
            int vagasLivres = estacionamento.VagasDisponiveis();
            int vagasOcupadas = estacionamento.TotalVagas - vagasLivres;
            Console.WriteLine($"📌 Vagas disponíveis: {vagasLivres}/{estacionamento.TotalVagas}");
            Console.WriteLine($"🚗 Vagas ocupadas: {vagasOcupadas}");
            break;

        case "4":
            var veiculosEstacionados = estacionamento.Veiculos.Where(v => v.Saida == null).ToList();
            var veiculosHistorico = estacionamento.Veiculos.Where(v => v.Saida != null).ToList();

            if (veiculosEstacionados.Any())
            {
                Console.WriteLine("\n🚗 VEÍCULOS ESTACIONADOS:");
                foreach (var v in veiculosEstacionados)
                {
                    var tempo = DateTime.Now - v.Entrada;
                    Console.WriteLine($"  {v.Placa} | {v.Modelo} | Entrada: {v.Entrada:dd/MM/yyyy HH:mm} | Tempo: {tempo.Hours}h {tempo.Minutes}min");
                }
            }
            else
            {
                Console.WriteLine("\n✨ Nenhum veículo estacionado no momento.");
            }

            if (veiculosHistorico.Any())
            {
                Console.WriteLine("\n📋 HISTÓRICO (últimos 10):");
                foreach (var v in veiculosHistorico.OrderByDescending(x => x.Saida).Take(10))
                {
                    Console.WriteLine($"  {v.Placa} | {v.Modelo} | Saída: {v.Saida:dd/MM/yyyy HH:mm} | Valor: R$ {v.ValorPago:F2}");
                }
            }
            break;

        case "0":
            Console.WriteLine("💾 Salvando dados...");
            estacionamento.SalvarDados();
            Console.WriteLine("✅ Encerrando sistema...");
            return;

        default:
            Console.WriteLine("❌ Opção inválida.");
            break;
    }
}


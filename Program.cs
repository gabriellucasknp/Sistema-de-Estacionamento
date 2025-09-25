using SistemaEstacionamento.Models;

Estacionamento estacionamento = new();

while (true)
{
    Console.WriteLine("\n--- SISTEMA DE ESTACIONAMENTO ---");
    Console.WriteLine("1 - Registrar entrada ");
    Console.WriteLine("2 - Registrar saída ");
    Console.WriteLine("3 - Ver vagas disponíveis ");
    Console.WriteLine("4 - Listar veículos ");
    Console.WriteLine("0 - Sair");
    Console.Write("Escolha: ");

    string opcao = Console.ReadLine();

    switch (opcao)
    {
        case "1":
            Console.Write("Placa: ");
            string placaEntrada = Console.ReadLine();

            Console.Write("Modelo: ");
            string modelo = Console.ReadLine();

            estacionamento.RegistrarEntrada(new Veiculo
            {
                Placa = placaEntrada,
                Modelo = modelo,
                Entrada = DateTime.Now
            });

            Console.WriteLine("✅ Veículo registrado!");
            break;

        case "2":
            Console.Write("Placa: ");
            string placaSaida = Console.ReadLine();

            estacionamento.RegistrarSaida(placaSaida);
            Console.WriteLine("✅ Saída registrada!");
            break;

        case "3":
            Console.WriteLine($"📌 Vagas disponíveis: {estacionamento.VagasDisponiveis()}");
            break;

        case "4":
            foreach (var v in estacionamento.Veiculos)
            {
                Console.WriteLine($"{v.Placa} | {v.Modelo} | Entrada: {v.Entrada} | Saída: {v.Saida} | Pago: R${v.ValorPago}");
            }
            break;

        case "0":
            Console.WriteLine("Encerrando...");
            return;

        default:
            Console.WriteLine("❌ Opção inválida.");
            break;
    }
}


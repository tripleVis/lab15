using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace lab15 {
	// Управление каналами
	class ChannelManager {
		public int ChannelsAmt { get; private set; }
		public int ClientsAmt { get; private set; }

		public bool Running { get; private set; }

		public List<Channel> Channels { get; private set; }
		public List<Client> Clients { get; private set; }

		static readonly Random Rndm = new Random();
		static readonly List<string> Names;

		public ChannelManager(int channelsAmt, int clientsAmt) {
			ChannelsAmt = channelsAmt;
			ClientsAmt = clientsAmt;

			Channels = new List<Channel>();
			Clients = new List<Client>();

			// Ушедшие клиенты и их удовлетворённость
			_departed = new List<(Client, bool)>();

			for (int i = 0; i < channelsAmt; i++) {
				var channel = new Channel();
				Channels.Add(channel);
			}
		}

		static ChannelManager() {
			// Чтение имён для случайных клиентов
			using var sr = new StreamReader("names.txt");
			Names = new List<string>();
			string line;
			while ((line = sr.ReadLine()) != null)
				Names.Add(line);
		}

		int GetRndmChannel() => Rndm.Next(0, Channels.Count);

		public void Start() {
			Running = true;
			while (Running) {
				Console.Clear();
				// Добавление клиентов
				for (int i = Clients.Count; i < ClientsAmt; i++)
					Clients.Add(new Client(
							Names[Rndm.Next(0, Names.Count)],
							Rndm.Next(1000, 10000),
							GetRndmChannel(),
							Rndm.Next(1000, 10000),
							this));

				// Вывод каналов
				for (int i = 0; i < ChannelsAmt; i++) {
					string str = $"Канал {i + 1}";
					Console.Write($"{str,-15}");
				}

				Console.WriteLine();
				// Вывод текущих пользователей
				for (int i = 0; i < ChannelsAmt; i++) {
					string str = Channels[i].Client == null ? "Нет клиента" : $"{Channels[i].Client}";
					Console.Write($"{str,-15}");
				}

				Console.WriteLine();
				// Вывод ожидающих пользователей
				for (int i = 0; i < ChannelsAmt; i++) {
					Console.WriteLine($"\nПользователи ожидающие подключение к каналу {i + 1}");
					foreach (var item in Clients) {
						if (item.ChannelN == i && Channels[i].Client != item)
							Console.Write(item + " ");
					}
				}

				Console.WriteLine();
				// Вывод ушедших пользователей
				Console.WriteLine("\nУшедшие пользователи");
				foreach (var item in _departed) {
					string res = item.Item2 == false ? "НЕ " : "";
					Console.WriteLine($"{item.Item1} ушёл {res}получив услугу");
				}
				_departed.Clear();
				Thread.Sleep(1000);
			}
		}

		List<(Client, bool)> _departed = new List<(Client, bool)>();
		public void Leave(Client client, bool state) {
			_departed.Add((client, state));
			Clients.Remove(client);
		}

		public void Stop() => Running = false;
	}
}

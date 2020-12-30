using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable UseObjectOrCollectionInitializer

namespace lab15 {
	class Program {
		static readonly List<Action> Tasks = new List<Action> {
			Task1,
			Task2,
			Task3,
			Task4,
			Task5,
			TaskDop1,
			TaskDop2
		};

		public static void Menu() {
			while (true) {
				Console.Write(
					"1 - вывод всех запущенных процессов" +
					"\n2 - текущий домен приложения" +
					"\n3 - задача расчёта в потоке" +
					"\n4 - 2 потока, чётные/нечётные числа" +
					"\n5 - Timer" +
					"\n6 - доп 1" +
					"\n7 - доп 2" +
					"\n0 - выход" +
					"\nВыберите действие: "
					);
				if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > Tasks.Count) {
					Console.WriteLine("Нет такого действия");
					Console.ReadKey();
					Console.Clear();
					continue;
				}
				if (choice == 0) {
					Console.WriteLine("Выход...");
					Environment.Exit(0);
				}
				Tasks[choice - 1]();
				Console.ReadKey();
				Console.Clear();
			}
		}

		static void Task1() {
			var processes = Process.GetProcesses();
			Console.WriteLine("Получение данных о запущенных процессах...");
			var processesInfo = processes
					.OrderBy(item => item.Id)
					.Select(item => new CustomProcessInfo(item))
					.ToList();
			Console.BufferWidth = 256;
			CustomProcessInfo.SmartPrinter(processesInfo,
				"Id", "ProcessName", "BasePriority", "StartTime", "Responding", "TotalProcessorTime");
		}

		static void Task2() {
			var domain = AppDomain.CurrentDomain;
			Console.WriteLine(
				$"\nТекущий домен\n" +
				$"\nИмя домена: {domain.FriendlyName}" +
				$"\nДетали конфигуриации" +
				$"\n    Директория приложения: {domain.SetupInformation.ApplicationBase}" +
				$"\n    Framework:             {domain.SetupInformation.TargetFrameworkName}" +
				$"\nВсе сборки, загруженные в домен:"
				);
			foreach (var item in domain.GetAssemblies())
				Console.WriteLine(item.FullName);
		}

		static void Task3() {
			int num;
			do Console.Write("Введите натуральное число: ");
			while (!int.TryParse(Console.ReadLine(), out num) || num < 1);

			var thread = new Thread(PrimeNumberFinder);
			Console.WriteLine(
				"Поток создан" +
				$"\nСостояние потока: {thread.ThreadState}" +
				$"\nИмя потока:       {thread.Name}" +
				$"\nПриоритет потока: {thread.Priority}" +
				$"\nID:               {thread.ManagedThreadId}"
				);

			Console.WriteLine($"\nПростые числа до {num}: ");
			thread.Start();
			bool pause = false;
			while (thread.IsAlive) {
				if (pause) {
					Thread.Sleep(1000);
					thread.Interrupt();
					Console.WriteLine("main > Работа потока возоблена через 1 сек");
					break;
				}
			}

			Console.WriteLine("main > Ожидание завершения работы потока");
			thread.Join();


			void PrimeNumberFinder() {
				for (int current = 1; current < num; current++) {
					if (current == num / 2) {
						Console.WriteLine("\n\nsub > Работа потока приостановлена на 2 сек");
						pause = true;
						try {
							Thread.Sleep(2000);
						}
						catch {
							Console.WriteLine("sub > Работа потока возоблена извне\n");
						}
					}
					if (IsPrime(current))
						Console.Write(current + " ");
				}

				static bool IsPrime(int n) {
					for (var i = 2; i < n; i++)
						if (n % i == 0)
							return false;
					return true;
				}
			}
		}

		static void Task4() {
			var counter = new Counter(10, CounterMode.AtOnce, "AtOnce.txt");
			Console.WriteLine("Вывод сначала нечётных, затем чётных чисел");
			counter.Start();
			Console.ReadKey();

			counter.Reset();
			counter.SetMode(CounterMode.InTurn);
			counter.SetFile("InTurn.txt");

			Console.WriteLine("Вывод чётных и нечётных чисел по очереди");
			counter.Start();
		}

		static void Task5() {
			var timers = new List<Timer>();
			timers.Add(new Timer(CreateNewTimer, new AutoResetEvent(true), 0, 1000));

			Console.ReadLine();
			foreach (var item in timers)
				item.Dispose();
			Console.WriteLine("Таймеры удалены");

			void CreateNewTimer(object state) {
				timers.Add(new Timer(CreateNewTimer, new AutoResetEvent(true), 1000, 0));
				Console.WriteLine($"Создан новый таймер, всего таймеров: {timers.Count}");
			}
		}

		static void TaskDop1() {
			var cars = new List<Car>() {
				new Car("Машина 1", 100),
				new Car("Машина 2", 200),
				new Car("Машина 3", 300)
			};
			using var storage = new Storage("storage.txt", 20, cars);
			var task = new Task(storage.StartUnloading);
			task.Start();
			task.Wait();
			Console.WriteLine("Склад разгружен\nСодержимое машин");
			foreach (var car in cars) {
				Console.WriteLine(car.Name);
				foreach (var item in car.Products)
					Console.WriteLine(item);
			}
		}

		static void TaskDop2() {
			var channelManager = new ChannelManager(5, 20);
			var task = new Task(channelManager.Start);
			task.Start();
			Console.ReadKey();
			channelManager.Stop();
		}
	}
}

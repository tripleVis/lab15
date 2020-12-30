using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace lab15 {
	class Storage : IDisposable {
		Thread _thread;

		public string File { get; private set; }
		public int ProductsAmt { get; private set; }
		public Queue<Car> Queue { get; private set; }
		public List<Car> Cars { get; private set; }
		public bool Loader { get; private set; }

		// Создание склада, регистрация машин
		public Storage(string file, int productsAmt, IEnumerable<Car> cars) {
			File = file;
			ProductsAmt = productsAmt;
			Cars = (List<Car>)cars;

			Queue = new Queue<Car>();

			using var sw = new StreamWriter(File);
			Console.WriteLine("Содержимое склада:");
			for (int i = 0; i < productsAmt; i++) {
				string data = $"{i + 1}) {Math.Pow((i + 1) * DateTime.Now.Millisecond, Math.PI)}";
				sw.WriteLine(data);
				Console.WriteLine(data);
			}
		}

		// Начало разгрузки
		public void StartUnloading() {
			// Команда всем машинам о начале разгрузки
			Cars.ForEach(car => car.StartUnloading(File, this));
			_thread = new Thread(Holder);
			_thread.Start();
			_thread.Join();
		}

		// Обработка логики очереди разгрузки
		private void Holder() {
			while (ProductsAmt != 0) {
				// Добавление машин вне очереди в очередь
				foreach (var car in Cars)
					if (!Queue.Contains(car))
						Queue.Enqueue(car);

				// Ожидание пока какая-либа машина встанет в очередь, пока ProductsAmt != 0
				if (Queue.Count != 0) {
					var car = Queue.Peek();
					// Если удалось получить машину и сейчас нет разгружающей машины
					if (car != null && Loader == false) {
						Console.Write("Текущая очередь: ");
						foreach (var item in Queue)
							Console.Write(item.Name + " / ");
						// Сообщение машине о её очереди
						car.Turn();
						ProductsAmt--;
						Loader = true;
					}
				}
			}
			// Ждать пока машина завершит разгрузку
			while (Loader)
				;
			// Сообщение всем машинам о завершении
			Cars.ForEach(car => car.FinishRequest());
		}

		// Проверка есть ли машина в очереди
		public bool IsRegistered(Car car) => Queue.Contains(car);

		// Добавление машины в очередь
		public void Register(Car car) {
			Console.WriteLine($"{car.Name} register request");
			if (IsRegistered(car)) {
				Console.WriteLine("Deny");
				return;
			}
			Console.WriteLine("Ok");
			Queue.Enqueue(car);
		}

		// Принятие сообщения от машины о завершении своей очереди
		public void Next(Car car) {
			if (Queue.Peek() != car)
				Console.WriteLine("!");
			Queue.Dequeue();
			Loader = false;
		}

		// Удаление файла вместе со складом
		public void Dispose() {
			new FileInfo(File).Delete();
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace lab15 {
	class Car {
		Thread _thread;
		bool _myTurn;

		// Для товара
		public string Name { get; private set; }
		public List<string> Products { get; private set; }
		public int Speed { get; private set; }

		// Для логики доставки
		public Storage Storage { get; private set; }
		public bool IsRegistered { get; private set; }
		public bool FinishUnloading { get; private set; }
		public string File { get; private set; }

		public Car(string name, int speed) {
			Name = name;
			Speed = speed;

			Products = new List<string>();
		}

		// Начало разгрузки
		public void StartUnloading(string file, Storage storage) {
			File = file;
			Storage = storage;

			IsRegistered = false;
			FinishUnloading = false;
			_myTurn = false;

			_thread = new Thread(Holder);
			_thread.Start();
		}

		public void Turn() => _myTurn = true;

		public void FinishRequest() => FinishUnloading = true;

		// Обработка логики загрузки товара
		private void Holder() {
			while (!FinishUnloading) {
				// Если _myTurn - начать разгружать, иначе ждать пока не FinishUnloading
				if (_myTurn) {
					Console.WriteLine($"\n{Name} загружает товар");
					// Задержка для "загрузки"
					Thread.Sleep(Speed);
					// Чтение данных в файле
					string line;
					var items = new List<string>();
					using (var sr = new StreamReader(File)) {
						while ((line = sr.ReadLine()) != null)
							items.Add(line);
					}
					// Добавление первого товара из файла к себе
					Products.Add(items[0]);

					// Удаление первой записи из файла и перезапись
					items.RemoveAt(0);
					using var sw = new StreamWriter(File);
					foreach (var item in items)
						sw.WriteLine(item);

					// Послать складу уведомление о завершении очереди загрузки этой машины
					Storage.Next(this);
					_myTurn = false;
				}
			}
		}
	}
}

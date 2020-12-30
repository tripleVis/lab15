using System;
using System.IO;
using System.Threading;

namespace lab15 {
	class Counter {
		Thread _oddThread;
		Thread _evenThread;
		int _oddThreadNum;
		int _evenThreadNum;
		bool _firstTurn;
		StreamWriter _sw;

		public CounterState State { get; private set; }
		public CounterMode Mode { get; private set; }
		public int Number { get; private set; }
		public string File { get; private set; }
		public int OddThreadSpeed { get; private set; }
		public int EvenThreadSpeed { get; private set; }

		// Создание счётчика с потоками для чётных и нечётных чисел
		public Counter(int number, CounterMode mode, string file, int speed1 = 250, int speed2 = 500) {
			State = CounterState.Ready;
			Mode = mode;
			Number = number;
			File = file;
			_sw = new StreamWriter(File);
			OddThreadSpeed = speed1;
			EvenThreadSpeed = speed2;

			_oddThread = new Thread(Odd);
			_evenThread = new Thread(Even);
			_oddThreadNum = 1;
			_evenThreadNum = 2;
			_firstTurn = true;
		}

		// Запуск счётчика и ожидание его завершения
		public void Start() {
			State = CounterState.Working;
			_oddThread.Start();
			_evenThread.Start();

			_oddThread.Join();
			_evenThread.Join();
			State = CounterState.Completed;
			Console.WriteLine("Счётчик завершил свою работу");
		}

		// Сброс счётчика
		public void Reset() {
			_oddThread = new Thread(Odd);
			_evenThread = new Thread(Even);
			_oddThreadNum = 1;
			_evenThreadNum = 2;
			_firstTurn = true;
			State = CounterState.Ready;
		}

		// Обработчик нечётного потока
		void Odd() {
			while (_oddThreadNum <= Number) {
				if (Mode == CounterMode.AtOnce) {
					Output();
				}
				else {
					// Ожидание своей очереди
					while (!_firstTurn)
						;
					Output();
					_firstTurn = false;
				}
				_oddThreadNum += 2;
			}

			void Output() {
				Thread.Sleep(OddThreadSpeed);
				Console.WriteLine("Нечётный поток: " + _oddThreadNum);
				_sw.WriteLine("Нечётный поток: " + _oddThreadNum);
				_sw.Flush();
			}
		}

		// Обработчик чётного потока
		void Even() {
			// Ожидание пока нечётный поток завершит свою работу
			if (Mode == CounterMode.AtOnce)
				_oddThread.Join();

			while (_evenThreadNum <= Number) {
				if (Mode == CounterMode.AtOnce) {
					Output();
				}
				else {
					// Ожидание своей очереди
					while (_firstTurn)
						;
					Output();
					_firstTurn = true;
				}
				_evenThreadNum += 2;
			}

			void Output() {
				Thread.Sleep(EvenThreadSpeed);
				Console.WriteLine("Чётный поток:   " + _evenThreadNum);
				_sw.WriteLine("Чётный поток: " + _evenThreadNum);
				_sw.Flush();
			}
		}


		// Изменения параметров счётчика
		public void SetPriority(CounterThread thread, ThreadPriority priority) {
			if (thread == CounterThread.Odd)
				_oddThread.Priority = priority;
			else
				_evenThread.Priority = priority;
		}

		public void SetFile(string file) {
			if (State == CounterState.Working)
				throw new Exception("Нельзя изменить файл во время работы счётчика");
			File = file;
			_sw = new StreamWriter(File);
		}

		public void SetMode(CounterMode mode) {
			if (State == CounterState.Working)
				throw new Exception("Нельзя изменить способ подсчёта во время работы счётчика");
			Mode = mode;
		}

		public void SetNumber(int number) {
			if (State == CounterState.Working)
				throw new Exception("Нельзя изменить число во время работы счётчика");
			Number = number;
		}

		public void SetSpeed(CounterThread thread, int speed) {
			if (thread == CounterThread.Odd)
				OddThreadSpeed = speed;
			else
				EvenThreadSpeed = speed;
		}
	}

	enum CounterThread {
		Odd,
		Even
	}

	// Состояние счётчика
	enum CounterState {
		Ready,
		Working,
		Completed
	}

	// Способ счёта: сначала одно потом другое или по очереди
	enum CounterMode {
		AtOnce,
		InTurn
	}
}

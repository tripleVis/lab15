using System.Threading;

namespace lab15 {
	class Channel {
		public Semaphore Semaphore { get; private set; }
		public Client Client { get; private set; }

		public Channel() {
			// Семафор на 1 поток
			Semaphore = new Semaphore(1, 1);
		}

		public void Reserve(Client client) => Client = client;

		public void Free() => Client = null;
	}
}

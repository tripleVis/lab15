using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace lab15 {
	class CustomProcessInfo : IEnumerable {
		// Процесс
		public Process Process { get; private set; }
		// Словарь свойств для дальнейшего использования
		public Dictionary<PropertyInfo, string> Properties { get; }
		// Словарь свойств для вывода
		public Dictionary<string, string> PropertiesStr { get; }
		// Список недоступных свойств
		public List<PropertyInfo> InaccessibleProperties { get; }

		// Массив доступных свойств класса Process
		static PropertyInfo[] PropertiesInfo { get; }

		static CustomProcessInfo() {
			// Получение всех свойств класса Process
			PropertiesInfo = typeof(Process).GetProperties().OrderBy(item => item.Name).ToArray();
		}

		public CustomProcessInfo(Process process) {
			Process = process;
			Properties = new Dictionary<PropertyInfo, string>();
			PropertiesStr = new Dictionary<string, string>();
			InaccessibleProperties = new List<PropertyInfo>();
			// Перебора всех свойств переданного процесса
			foreach (var propertie in PropertiesInfo) {
				// Добавление свойства в словарь доступных свойств, если не было отказано в доступе
				try {
					string name = propertie.Name;
					object value = propertie.GetValue(Process);
					if (value == null)
						continue;
					string strValue = value.ToString();
					Properties.Add(propertie, strValue);
					PropertiesStr.Add(propertie.Name, strValue);
				}
				// Если было отказано системой в доступе, добавить свойство к недоступным
				catch {
					InaccessibleProperties.Add(propertie);
				}
			}
		}

		// Просмотр свойства процесса по индексу
		public string this[string index] => PropertiesStr[index];
		// Получение перечислителя из списка свойств
		public IEnumerator GetEnumerator() => ((IEnumerable)PropertiesStr).GetEnumerator();

		// Вывод всех свойств данного процесса
		public void PrintAllProperties() {
			foreach (var property in PropertiesInfo) {
				try {
					Console.WriteLine($"{property.Name,-30}{property.GetValue(Process)}");
				}
				catch {
					Console.WriteLine($"{property.Name,-30}отказано в доступе");
				}
			}
		}

		// Вывод только доступных для чтения свойств данного процесса
		public void PrintAvailableProperties() {
			foreach (var property in PropertiesInfo) {
				try {
					Console.WriteLine($"{property.Name,-30}{property.GetValue(Process)}");
				}
				catch { }
			}
		}

		// Вывод доступных свойств процессов в виде таблицы
		public static void SmartPrinter(IEnumerable<CustomProcessInfo> processes, params string[] requestedProperties) {
			// Проверки
			if (requestedProperties.Length == 0) {
				Console.WriteLine("Не заданы свойства");
				return;
			}
			var passedRequestedProperties = new List<string>();
			foreach (var reqProp in requestedProperties) {
				if (PropertiesInfo.Any(existingProperty => existingProperty.Name == reqProp)) {
					passedRequestedProperties.Add(reqProp);
				}
				else {
					Console.WriteLine($"Класс Process не содержит свойства {reqProp}");
				}
			}
			if (passedRequestedProperties.Count == 0) {
				Console.WriteLine("Не было определено ни одного заданного свойства");
				return;
			}

			var propertiesLengths = new Dictionary<string, int>();
			// Определение оптимальной длины строки значения свойства
			foreach (var process in processes) {
				foreach (var property in process.Properties) {
					if (passedRequestedProperties.All(item => item != property.Key.Name))
						continue;
					if (propertiesLengths.All(pair => pair.Key != property.Key.Name))
						propertiesLengths.Add(property.Key.Name, 0);
					if (!CheckAccessibility(process, property.Key.Name))
						propertiesLengths[property.Key.Name] = "Отказано в доступе".Length;
					if (propertiesLengths[property.Key.Name] < property.Value.Length)
						propertiesLengths[property.Key.Name] = property.Value.Length;
				}
			}

			// Таблица
			// Вывод заголовков
			foreach (var reqProp in passedRequestedProperties) {
				bool inaccessible = false;
				foreach (var process in processes) {
					string property = propertiesLengths.Where(pair => pair.Key == reqProp).ToList()[0].Key;
					if (!CheckAccessibility(process, property)) {
						inaccessible = true;
						break;
					}
				}
				var item = propertiesLengths.Where(pair => pair.Key == reqProp).ToList()[0];
				int optimalLength = inaccessible
					? item.Key.Length > "Отказано в доступе".Length
						? item.Key.Length > item.Value ? item.Key.Length : item.Value
						: "Отказано в доступе".Length > item.Value ? "Отказано в доступе".Length : item.Value
					: item.Key.Length > item.Value ? item.Key.Length : item.Value;
				Console.Write(item.Key);
				for (int i = item.Key.Length; i < optimalLength + 4; i++)
					Console.Write(" ");
			}
			Console.WriteLine();
			// Вывод значений
			foreach (var process in processes) {
				foreach (var propertyStr in passedRequestedProperties) {
					string property = propertiesLengths.Where(pair => pair.Key == propertyStr).ToList()[0].Key;
					if (!CheckAccessibility(process, property))
						process.PropertiesStr[property] = "Отказано в доступе";
					var optimalLength = property.Length > propertiesLengths[property] ? property.Length : propertiesLengths[property];
					Console.Write(process.PropertiesStr[propertyStr]);
					for (int i = process.PropertiesStr[propertyStr].Length; i < optimalLength + 4; i++)
						Console.Write(" ");
				}
				Console.WriteLine();
			}

			// Проверка доступности свойства определённого процесса
			static bool CheckAccessibility(CustomProcessInfo process, string property) =>
				!process.InaccessibleProperties.Any(inaProp => inaProp.Name == property);
		}
	}
}

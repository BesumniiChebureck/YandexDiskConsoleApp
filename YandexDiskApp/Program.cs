using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using YandexDisk.Client.Http;
using YandexDiskApp.Services;
using YandexDiskApp.Services.Interfaces;

namespace YandexDiskApp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			if (args.Length == 0)
			{
				// Если запуск без аргументов - показать справку и ждать ввода
				ShowHelp();
				Console.WriteLine("\nВведите команду (или 'exit' для выхода):");

				while (true)
				{
					Console.Write("> ");
					var input = Console.ReadLine()?.Trim();

					if (string.IsNullOrEmpty(input)) continue;
					if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

					try
					{
						await ProcessCommand(input.Split(' '));
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Ошибка: {ex.Message}");
					}
				}
				return;
			}

			// Обработка аргументов командной строки
			try
			{
				await ProcessCommand(args);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка: {ex.Message}");
				Environment.Exit(1);
			}
		}

		private static async Task ProcessCommand(string[] args)
		{
			// Настройка DI
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.Build();

			var services = new ServiceCollection();
			ConfigureServices(services, configuration);

			using (var serviceProvider = services.BuildServiceProvider())
			{
				var diskService = serviceProvider.GetRequiredService<IYandexDiskService>();

				if (args.Length < 2) throw new ArgumentException("Не указан путь");
				string path = NormalizePath(args[1]);

				switch (args[0].ToLower())
				{
					case "view":
						await diskService.PrintFolderItemsAsync(path);
						break;

					case "mkdir":
						await diskService.CreateFolderAsync(path);
						Console.WriteLine($"Операция выполнена успешно");
						break;

					case "upload":
						if (args.Length < 3) throw new ArgumentException("Не указаны пути");
						await diskService.UploadFileAsync(NormalizePath(args[2]), args[1]);
						Console.WriteLine($"Файл {args[1]} успешно загружен в {NormalizePath(args[2])}");
						break;

					case "download":
						var destPath = args.Length > 2 ? NormalizePath(args[2]) : string.Empty;
						await diskService.DownloadFileAsync(args[1], destPath);
						Console.WriteLine($"Файл {args[1]} успешно скачан");
						break;

					case "download-link":
						var link = await diskService.GetDownloadFileLinkAsync(path);
						Console.WriteLine($"Ссылка для скачивания: {link}");
						break;

					default:
						ShowHelp();
						Environment.Exit(1);
						break;
				}
			}
		}

		/// <summary>
		/// Настройка сервисов (DI)
		/// </summary>
		private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		{
			services.AddLogging(configure => configure.AddConsole());

			services.AddSingleton<DiskHttpApi>(_ => new DiskHttpApi(configuration["YandexDisk:AccessToken"]));
			services.AddSingleton<IYandexDiskService, YandexDiskService>();
		}

		private static void ShowHelp()
		{
			Console.WriteLine("Использование:");
			Console.WriteLine("  YandexDiskApp view <путь>");
			Console.WriteLine("  YandexDiskApp mkdir <путь>");
			Console.WriteLine("  YandexDiskApp upload <локальный_файл> <путь_на_диске>");
			Console.WriteLine("  YandexDiskApp download <файл_на_диске> [локальный_путь]");
			Console.WriteLine("  YandexDiskApp download-link <файл_на_диске>");
			Console.WriteLine("\nИли запустите без аргументов для интерактивного режима");
		}

		/// <summary>
		/// Нормализует путь, добавляя завершающий слеш если его нет
		/// </summary>
		/// <param name="path">Исходный путь</param>
		/// <returns>Путь с завершающим слешем</returns>
		public static string NormalizePath(string path)
		{
			string formattedPath = path.TrimEnd('/');
			if (string.IsNullOrEmpty(formattedPath))
				return "/";
			return formattedPath;
		}
	}
}
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YandexDisk.Client.Clients;
using YandexDisk.Client.Http;
using YandexDisk.Client.Protocol;
using YandexDiskApp.Services.Interfaces;

namespace YandexDiskApp.Services
{
	public class YandexDiskService : IYandexDiskService
	{
		private readonly DiskHttpApi _diskApi;
		private readonly ILogger<YandexDiskService> _logger;

		public YandexDiskService(DiskHttpApi diskApi, ILogger<YandexDiskService> logger)
		{
			_diskApi = diskApi;
			_logger = logger;
		}

		/// <summary>
		/// Получить данные директории по пути path
		/// </summary>
		/// <param name="path">Путь к директории на Яндекс Диске</param>
		public async Task<Resource> GetFolderDataAsync(string path)
		{
			try
			{
				Resource folderData = await _diskApi.MetaInfo.GetInfoAsync(new ResourceRequest
					{
						Path = path
					}
				);

				return folderData;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при получении данных папки {path}");
				throw;
			}
		}

		/// <summary>
		/// Вывести список объектов в директории по пути path
		/// </summary>
		/// <param name="path">Путь к директории на Яндекс Диске</param>
		public async Task PrintFolderItemsAsync(string path)
		{
			try
			{
				var folderData = await GetFolderDataAsync(path);

				foreach (var item in folderData.Embedded.Items)
				{
					Console.WriteLine($"{item.Name}\t{item.Type}\t{item.MimeType}\t");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при выводе списка объектов {path}");
				throw;
			}
		}

		/// <summary>
		/// Создать папку с именем nameOfBeCreatedFolder в директории по пути pathToCreate (если папка уже создана - ничего не делать)
		/// </summary>
		/// <param name="pathToCreate">Путь к директории на Яндекс Диске, где будет создана папка</param>
		/// <param name="nameOfBeCreatedFolder">Имя создаваемой папки</param>
		public async Task CreateFolderAsync(string pathToCreate, string nameOfBeCreatedFolder)
		{
			try
			{
				var folderData = await GetFolderDataAsync(pathToCreate);

				if (!folderData.Embedded.Items.Any(i => i.Type == ResourceType.Dir && i.Name.Equals(nameOfBeCreatedFolder)))
				{
					await _diskApi.Commands.CreateDictionaryAsync(pathToCreate + nameOfBeCreatedFolder);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при создании папки {nameOfBeCreatedFolder} в папке {pathToCreate}");
				throw;
			}
		}

		/// <summary>
		/// Создать папку по пути path (если папка уже создана - ничего не делать)
		/// </summary>
		/// <param name="path">Путь для создания папки</param>
		public async Task CreateFolderAsync(string path)
		{
			try
			{
				var folderData = await GetFolderDataAsync(GetParentFolderUnix(path));

				if (!folderData.Embedded.Items.Any(i => i.Type == ResourceType.Dir && i.Name.Equals(Path.GetFileName(path))))
				{
					await _diskApi.Commands.CreateDictionaryAsync(path);
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("points to existent directory"))
				{
					// Если в исключении кидает ошибку с текстом
					// points to existent directory (папка уже создана) - игнорируем предупреждение
					return;
				}
				_logger.LogError(ex, $"Ошибка при создании папки по пути {path}");
				throw;
			}
		}

		/// <summary>
		/// Загрузить файл filePath на диск в директорию pathToUpload с режимом перезаписи overwrite
		/// </summary>
		/// <param name="pathToUpload">Путь в который нужно загрузить файл на диске</param>
		/// <param name="filePath">Путь к файлу для загрузки на диск</param>
		/// <param name="overwrite">Необходимо ли перезаписать загружаемый файл, если он уже существует</param>
		public async Task UploadFileAsync(string pathToUpload, string filePath, bool overwrite = false)
		{
			try
			{
				// Проверка существования пути для загрузки файлов (если нет - создать недостающие папки)
				await CreateFolderAsync(pathToUpload);

				var uploadLink = await _diskApi.Files.GetUploadLinkAsync(pathToUpload + "/" + Path.GetFileName(filePath), overwrite: overwrite);

				using (var fs = File.OpenRead(filePath))
				{
					await _diskApi.Files.UploadAsync(uploadLink, fs);
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("already exists") && !overwrite)
				{
					// Если в исключении кидает ошибку с текстом already exists (файл уже создан)
					// и при этом выключен режим перезаписи файлов - игнорируем предупреждение
					return;
				}
				_logger.LogError(ex, $"Ошибка при загрузке файла {filePath} в папку {pathToUpload} с режимом перезаписи в значении {overwrite}");
				throw;
			}
		}

		/// <summary>
		/// Скачать файл с диска по пути pathToFileOnDisk в папку pathToDownload
		/// </summary>
		/// <param name="pathToFileOnDisk">Путь к файлу на диске</param>
		/// <param name="pathToDownload">Путь для скачивания файла (если не указать - скачается в папку Downloads в той же папке, что и .exe файл приложения</param>
		public async Task DownloadFileAsync(string pathToFileOnDisk, string pathToDownload = "")
		{
			string destDir =
				string.IsNullOrEmpty(pathToDownload)
					? Path.Combine(Environment.CurrentDirectory, "Downloads")
					: pathToDownload;

			try
			{
				if (!Directory.Exists(destDir))
					Directory.CreateDirectory(destDir);

				await _diskApi.Files.DownloadFileAsync(pathToFileOnDisk, Path.Combine(destDir, Path.GetFileName(pathToFileOnDisk)));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при скачивании файла {pathToFileOnDisk} в папку {destDir}");
				throw;
			}
		}

		/// <summary>
		/// Получить ссылку на скачивание файла с диска по пути pathToFileOnDisk
		/// </summary>
		/// <param name="pathToFileOnDisk">Путь к файлу на диске</param>
		public async Task<string> GetDownloadFileLinkAsync(string pathToFileOnDisk)
		{
			try
			{
				var link = await _diskApi.Files.GetDownloadLinkAsync(pathToFileOnDisk);
				return link.Href;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при получении ссылки для скачивания файла {pathToFileOnDisk}");
				throw;
			}
		}

		/// <summary>
		/// Получение родительского каталога в Unix пути к файлу
		/// </summary>
		/// <param name="path">Полный путь в Unix формате, из которого нужно получить родительский каталог</param>
		/// <returns>Путь к родительскому каталогу в Unix формате</returns>
		private string GetParentFolderUnix(string path)
		{
			var parts = path.Trim('/').Split('/');
			return parts.Length > 1 ? parts[parts.Length - 2] : "/";
		}
	}
}

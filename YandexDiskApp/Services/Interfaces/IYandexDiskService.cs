using System.Threading.Tasks;
using YandexDisk.Client.Protocol;

namespace YandexDiskApp.Services.Interfaces
{
	public interface IYandexDiskService
	{
		Task<Resource> GetFolderDataAsync(string path);
		Task PrintFolderItemsAsync(string path);
		Task CreateFolderAsync(string pathToCreate, string folderName);
		Task CreateFolderAsync(string path);
		Task UploadFileAsync(string pathToUpload, string fileName, bool overwrite = false);
		Task DownloadFileAsync(string pathToFileOnDisk, string pathToDownload = "");
		Task<string> GetDownloadFileLinkAsync(string pathToFileOnDisk);
	}
}

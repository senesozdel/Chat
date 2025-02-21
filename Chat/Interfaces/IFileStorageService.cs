using B2Net.Models;

namespace Chat.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task DeleteFileAsync(string fileName);

        Task<B2File> DownloadFileAsync(string fileName);
    }
}

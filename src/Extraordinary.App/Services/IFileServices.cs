using Extraordinary.App.Config;
using Extraordinary.App.Respone;

namespace Extraordinary.App.Services
{
    public interface IFileServices
    {
        Task<ResponeReturn<string>> UpdateAsync(string updateConfigFilePath, string urlConfigName, Func<UpdateConfig, string>? func = null);
        Task<ResponeReturn<string>> DownloadAppAsync(string url, string name, string dirPath);
        Task<ResponeReturn<string>> UnCompressAsync(string sourcefilePath, string destdirPath);
        Task<ResponeReturn<bool>> StartProcessAsync(string dirPath, string name);
        Task<ResponeReturn<string>> GetFileMD5HashAsync(string filePath);

        Task<ResponeReturn<T>> GetConfigAsync<T>(string filePath);
        Task<ResponeReturn<T>> SaveConfigAsync<T>(T config, string filePath);
        Task<ResponeReturn<string>> CompareFileAsync(string filePath, string md5Version);
        Task KillProcessAsync(string processName);
    }
}

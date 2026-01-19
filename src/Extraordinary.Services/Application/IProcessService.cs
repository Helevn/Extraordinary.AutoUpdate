using Extraordinary.Shared;
using Extraordinary.Shared.Local;

namespace Extraordinary.Services.Application
{
    public interface IProcessService
    {
        /// <summary>
        /// 自动更新主流程
        /// </summary>
        /// <param name="updateConfigFilePath"></param>
        /// <param name="urlConfigName"></param>
        /// <returns></returns>
        Task<ResponeReturn<LocalConfig>> UpdateAsync(string updateConfigFilePath, string urlConfigName);
        /// <summary>
        /// 从指定URL下载应用程序
        /// </summary>
        /// <param name="url"></param>
        /// <param name="name"></param>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        Task<ResponeReturn<string>> DownloadAppAsync(string url, string name, string dirPath);
        /// <summary>
        /// 运行指定目录下的进程
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<ResponeReturn<bool>> StartProcessAsync(string dirPath, string name);
        /// <summary>
        /// 杀死指定名称的进程
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        Task KillProcessAsync(string processName);
        /// <summary>
        /// 通过名称查找指定目录下的文件
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<ResponeReturn<FileInfo>> FindProcessFileAsync(string dirPath, string name);
        /// <summary>
        /// 创建新的软件应用包
        /// </summary>
        /// <param name="path"></param>
        /// <param name="version"></param>
        /// <param name="originConfigName"></param>
        /// <returns></returns>
        Task<ResponeReturn<string>> MakeNewVersionConfigAsync(string path, string version, string originConfigName);
    }
}

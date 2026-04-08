using Extraordinary.Shared;

namespace Extraordinary.WebApi.Document
{
    public interface IFileService
    {
        /// <summary>
        /// 文件解压
        /// </summary>
        /// <param name="sourcefilePath"></param>
        /// <param name="destdirPath"></param>
        /// <returns></returns>
        Task<ResponeReturn<string>> UnCompressAsync(string sourcefilePath, string destdirPath);
        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        Task<ResponeReturn<string>> GetFileMD5HashAsync(string filePath);
        /// <summary>
        /// 从本地读取配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        Task<ResponeReturn<T>> GetConfigAsync<T>(string filePath);
        /// <summary>
        /// 保存配置文件到本地
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        Task<ResponeReturn<T>> SaveConfigAsync<T>(T config, string filePath);
        /// <summary>
        /// 比对文件MD5值
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="md5Version"></param>
        /// <returns></returns>
        Task<ResponeReturn<string>> CompareFileAsync(string filePath, string md5Version);
        /// <summary>
        /// 文件的另存为
        /// </summary>
        /// <param name="sourcefilePath"></param>
        /// <param name="destfilePath"></param>
        /// <returns></returns>
        Task<ResponeReturn<string>> SaveAsAsync(string sourcefilePath, string destfilePath);
    }
}

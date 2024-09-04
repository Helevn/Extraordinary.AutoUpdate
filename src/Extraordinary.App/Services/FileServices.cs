using Extraordinary.App.Config;
using Extraordinary.App.MessageHandler;
using Extraordinary.App.Respone;
using MediatR;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Extraordinary.App.Services
{
    public class FileServices : IFileServices
    {
        private readonly IMediator _mediator;
        public FileServices(IMediator mediator)
        {
            this._mediator = mediator;
        }

        public async Task<ResponeReturn<string>> UpdateAsync(string updateConfigFilePath, string urlConfigName, Func<UpdateConfig, string>? func = null)
        {
            var r1 = await this.GetConfigAsync<UpdateConfig>(updateConfigFilePath);
            if (!r1.Succeed)
                return r1.ErrorValue.ToErrorResult<string>();
            var tempPath = Path.GetTempPath();

            var updateConfig = r1.ResultValue;
            var r2 = await this.DownloadAppAsync(updateConfig.ServerUrl, urlConfigName, tempPath);
            if (!r2.Succeed)
                return r2.ErrorValue.ToErrorResult<string>();

            var urlconfigFilePath = r2.ResultValue;
            var r3 = await this.GetConfigAsync<UrlConfig>(urlconfigFilePath);
            if (!r3.Succeed)
                return r3.ErrorValue.ToErrorResult<string>();

            var urlconfig = r3.ResultValue;

            var needUpdate = updateConfig.CurrentMD5Version != urlconfig.AppMD5Version;
            if (needUpdate)
            {
                var r4 = await this.DownloadAppAsync(updateConfig.ServerUrl, urlconfig.PackageName, updateConfig.DownloadPath);
                if (!r4.Succeed)
                    return r4.ErrorValue.ToErrorResult<string>();

                var appPath = r4.ResultValue;
                var r5 = await this.CompareFileAsync(appPath, urlconfig.AppMD5Version);
                if (!r5.Succeed)
                    return r5.ErrorValue.ToErrorResult<string>();

                var compare = r5.ResultValue;
                var r6 = await this.UnCompressAsync(appPath, updateConfig.InstallationPath);
                if (!r6.Succeed)
                    return r6.ErrorValue.ToErrorResult<string>();

                var installPath = r6.ResultValue;
                var r7 = await this.StartProcessAsync(installPath, updateConfig.AppName);
                if (!r7.Succeed)
                    return r7.ErrorValue.ToErrorResult<string>();

                var config = new UpdateConfig
                {
                    ServerUrl = updateConfig.ServerUrl,
                    AppName = updateConfig.AppName,
                    DownloadPath = updateConfig.DownloadPath,
                    CurrentMD5Version = compare,
                    InstallationPath = updateConfig.InstallationPath,
                    Self_Starting = updateConfig.Self_Starting
                };

                var r8 = await SaveConfigAsync(config, updateConfigFilePath);
                if (!r8.Succeed)
                    return r8.ErrorValue.ToErrorResult<string>();

                if (func != null)
                    func(config);

                return "程序更新成功".ToOkResult();
            }
            else
            {
                var r10 = await this.StartProcessAsync(updateConfig.InstallationPath, updateConfig.AppName);
                if (!r10.Succeed)
                    return r10.ErrorValue.ToErrorResult<string>();
                return "程序已经是最新的".ToOkResult();
            }
        }

        public async Task<ResponeReturn<string>> DownloadAppAsync(string url, string name, string dirPath)
        {
            await _mediator.Publish(ProgressBarINotification.Min("文件下载"));
            if (string.IsNullOrEmpty(url))
            {
                return "服务器下载地址不可为空".ToErrorResult<string>();
            }
            if (string.IsNullOrEmpty(name))
            {
                return "下载文件名不可为空".ToErrorResult<string>();
            }
            if (string.IsNullOrEmpty(dirPath))
            {
                return "下载地址不可为空".ToErrorResult<string>();
            }
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            var filepath = Path.Combine(dirPath, name);
            if (File.Exists(filepath))
                File.Delete(filepath);

            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync($"{url}/{name}", HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            var buffer = new byte[8192];
            int bytesRead = 0;
            long totalBytesRead = 0;
            long totalBytes = response.Content.Headers.ContentLength ?? -1;
            await _mediator.Publish(new ProgressBarINotification { Action = "文件下载", MaxValue = totalBytes, Value = 0 });
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;
                await _mediator.Publish(new ProgressBarINotification { Action = "文件下载", MaxValue = totalBytes, Value = totalBytesRead });
                //await Task.Delay(1);
            }
            fileStream.Close();
            fileStream.Dispose();
            return filepath.ToOkResult();
        }

        public async Task<ResponeReturn<string>> UnCompressAsync(string sourcefilePath, string destdirPath)
        {
            await _mediator.Publish(ProgressBarINotification.Min("文件解压"));

            if (string.IsNullOrEmpty(sourcefilePath))
            {
                return "解压源文件地址不可为空".ToErrorResult<string>();
            }
            if (string.IsNullOrEmpty(destdirPath))
            {
                return "解压目标地址不可为空".ToErrorResult<string>();
            }

            if (!Directory.Exists(destdirPath))
                Directory.CreateDirectory(destdirPath);

            ZipFile.ExtractToDirectory(sourcefilePath, destdirPath, true);

            await _mediator.Publish(ProgressBarINotification.Max("文件解压"));
            return destdirPath.ToOkResult<string>();
        }

        private async Task<ResponeReturn<FileInfo>> FindProcessFile(string dirPath, string name)
        {
            if (string.IsNullOrEmpty(dirPath))
            {
                return "程序安装地址不可为空".ToErrorResult<FileInfo>();
            }
            if (string.IsNullOrEmpty(name))
            {
                return "程序名称不可为空".ToErrorResult<FileInfo>();
            }

            foreach (var item in Directory.GetFiles(dirPath))
            {
                var filePath = new FileInfo(item);
                if (filePath.Name == name)
                    return filePath.ToOkResult<FileInfo>();
            }
            foreach (var item in Directory.GetDirectories(dirPath))
            {
                var fileinfo = await this.FindProcessFile(item, name);
                if (fileinfo.Succeed)
                    return fileinfo;
            }
            return $"找不到名称:{name}的程序".ToErrorResult<FileInfo>();
        }

        public async Task<ResponeReturn<bool>> StartProcessAsync(string dirPath, string name)
        {
            await _mediator.Publish(ProgressBarINotification.Min("程序启动"));
            if (!Directory.Exists(dirPath))
            {
                return $"路径{dirPath}不存在,无法启动".ToErrorResult<bool>();
            }
            var fileinfo = await this.FindProcessFile(dirPath, name);
            if (!fileinfo.Succeed)
            {
                return fileinfo.ErrorValue.ToErrorResult<bool>();
            }
            using (Process myProcess = new())
            {
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.FileName = fileinfo.ResultValue.FullName;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.Start();
            }
            await _mediator.Publish(ProgressBarINotification.Max("程序启动"));
            return true.ToOkResult();
        }

        public async Task<ResponeReturn<string>> GetFileMD5HashAsync(string filePath)
        {
            using FileStream stream = File.OpenRead(filePath);
            MD5 md5 = MD5.Create();
            byte[] hashValue = await md5.ComputeHashAsync(stream);

            // 将字节数组转换为十六进制字符串
            StringBuilder hex = new StringBuilder(hashValue.Length * 2);
            foreach (byte b in hashValue)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString().ToOkResult();
        }

        public async Task<ResponeReturn<T>> SaveConfigAsync<T>(T config, string filePath)
        {
            var str = JsonConvert.SerializeObject(config);
            await File.WriteAllTextAsync(filePath, str, Encoding.UTF8);
            return config.ToOkResult();
        }
        public async Task<ResponeReturn<T>> GetConfigAsync<T>(string filePath)
        {
            var str = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            var config = JsonConvert.DeserializeObject<T>(str);
            if (config == null)
                return $"文件解析失败:{filePath}".ToErrorResult<T>();
            return config.ToOkResult<T>();
        }

        public async Task<ResponeReturn<string>> CompareFileAsync(string filePath, string md5Version)
        {
            var filemd5Res = await GetFileMD5HashAsync(filePath);
            if (!filemd5Res.Succeed)
                return filemd5Res.ErrorValue.ToErrorResult<string>();
            var filemd5 = filemd5Res.ResultValue;
            if (filemd5 != md5Version)
                return $"文件:{filePath}的MD5比对失败".ToErrorResult<string>();
            return filemd5.ToOkResult();
        }
    }
}

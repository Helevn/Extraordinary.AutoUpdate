using Extraordinary.Services.Document;
using Extraordinary.Shared;
using Extraordinary.Shared.Local;
using Extraordinary.Shared.Message;
using Extraordinary.Shared.Origin;
using MediatR;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Extraordinary.Services.Application
{
    public class ProcessService(IMediator mediator, IFileService fileService) : IProcessService
    {
        public async Task<ResponeReturn<LocalConfig>> UpdateAsync(string updateConfigFilePath, string urlConfigName)
        {
            var r1 = await fileService.GetConfigAsync<LocalConfig>(updateConfigFilePath);//读本地配置文件
            if (!r1.Succeed)
                return r1.ErrorValue.ToErrorResult<LocalConfig>();
            var tempPath = Path.GetTempPath();

            var updateConfig = r1.ResultValue;
            var r2 = await this.DownloadAppAsync(updateConfig.ServerUrl, urlConfigName, tempPath);//访问服务端配置文件
            if (!r2.Succeed)
                return r2.ErrorValue.ToErrorResult<LocalConfig>();

            var urlconfigFilePath = r2.ResultValue;
            var r3 = await fileService.GetConfigAsync<OriginConfig>(urlconfigFilePath);
            if (!r3.Succeed)
                return r3.ErrorValue.ToErrorResult<LocalConfig>();

            if (updateConfig.Kill_App)
                await this.KillProcessAsync(updateConfig.AppName);//杀死当前进程

            var urlconfig = r3.ResultValue;
            //通过比对版本和MD5信息判断是否需要更新
            var needUpdate = updateConfig.CurrentMD5Version != urlconfig.AppMD5Version || updateConfig.CurrentVersion != urlconfig.AppVersion;
            if (needUpdate)
            {
                var r4 = await this.DownloadAppAsync(updateConfig.ServerUrl, urlconfig.PackageName, updateConfig.DownloadPath);//下载更新程序
                if (!r4.Succeed)
                    return r4.ErrorValue.ToErrorResult<LocalConfig>();

                var appPath = r4.ResultValue;
                var r5 = await fileService.CompareFileAsync(appPath, urlconfig.AppMD5Version);//文件完整性比对
                if (!r5.Succeed)
                    return r5.ErrorValue.ToErrorResult<LocalConfig>();

                var r6 = await fileService.UnCompressAsync(appPath, updateConfig.InstallationPath);//解压
                if (!r6.Succeed)
                    return r6.ErrorValue.ToErrorResult<LocalConfig>();

                updateConfig.CurrentVersion = urlconfig.AppVersion;
                updateConfig.CurrentMD5Version = urlconfig.AppMD5Version;
                var r8 = await fileService.SaveConfigAsync(updateConfig, updateConfigFilePath);//保存更新后的数据
                if (!r8.Succeed)
                    return r8.ErrorValue.ToErrorResult<LocalConfig>();
            }
            //是否自动启动应用程序
            if (updateConfig.Self_Starting)
            {
                var r10 = await this.StartProcessAsync(updateConfig.InstallationPath, updateConfig.AppName);//启动
                if (!r10.Succeed)
                    return r10.ErrorValue.ToErrorResult<LocalConfig>();
            }
            return updateConfig.ToOkResult();
        }

        public async Task<ResponeReturn<string>> DownloadAppAsync(string url, string name, string dirPath)
        {
            await mediator.Publish(ProgressBarINotification.Min("文件下载"));
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
            await mediator.Publish(new ProgressBarINotification { Action = "文件下载", MaxValue = totalBytes, Value = 0 });
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;
                await mediator.Publish(new ProgressBarINotification { Action = "文件下载", MaxValue = totalBytes, Value = totalBytesRead });
                //await Task.Delay(1);
            }
            fileStream.Close();
            fileStream.Dispose();
            return filepath.ToOkResult();
        }

        public async Task<ResponeReturn<FileInfo>> FindProcessFileAsync(string dirPath, string name)
        {
            if (string.IsNullOrEmpty(dirPath))
            {
                return "程序文件安装地址不可为空".ToErrorResult<FileInfo>();
            }
            if (string.IsNullOrEmpty(name))
            {
                return "程序文件名称不可为空".ToErrorResult<FileInfo>();
            }

            foreach (var item in Directory.GetFiles(dirPath))
            {
                var filePath = new FileInfo(item);
                if (filePath.Name == name)
                    return filePath.ToOkResult<FileInfo>();
            }
            foreach (var item in Directory.GetDirectories(dirPath))
            {
                var fileinfo = await this.FindProcessFileAsync(item, name);
                if (fileinfo.Succeed)
                    return fileinfo;
            }
            return $"找不到名称:{name}的文件".ToErrorResult<FileInfo>();
        }

        public async Task<ResponeReturn<bool>> StartProcessAsync(string dirPath, string name)
        {
            await mediator.Publish(ProgressBarINotification.Min("程序启动"));
            if (!Directory.Exists(dirPath))
            {
                return $"路径{dirPath}不存在,无法启动".ToErrorResult<bool>();
            }
            var fileinfo = await this.FindProcessFileAsync(dirPath, name);
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
            await mediator.Publish(ProgressBarINotification.Max("程序启动"));
            return true.ToOkResult();
        }

        public async Task KillProcessAsync(string processName)
        {
            processName = Path.GetFileNameWithoutExtension(processName);
            var ps = Process.GetProcesses();
            foreach (var p in ps)
            {
                if (p.ProcessName.Contains(processName))
                {
                    p.Kill();
                    await p.WaitForExitAsync(); // possibly with a timeout
                }
            }
        }

        public async Task<ResponeReturn<string>> MakeNewVersionConfigAsync(string path, string version, string originConfigName)
        {
            //拿文件的MD5信息
            var r1 = await fileService.GetFileMD5HashAsync(path);
            if (!r1.Succeed)
                return r1;
            var md5info = r1.ResultValue;
            //另存为新的文件
            var fileinfo = new FileInfo(path);
            var fileDir = fileinfo.Directory?.FullName ?? "";

            var zipname = Path.GetFileNameWithoutExtension(path);
            var zipex = Path.GetExtension(path);
            var newzip = Path.Combine(fileDir, zipname + "-" + version + "-" + md5info + zipex);
            var r2 = await fileService.SaveAsAsync(path, newzip);
            if (!r2.Succeed)
                return r2;

            var filePath = Path.Combine(fileDir, originConfigName);
            var config = new OriginConfig { PackageName = Path.GetFileName(newzip), AppVersion = version, AppMD5Version = md5info };
            var r3 = await fileService.SaveConfigAsync(config, filePath);
            if (!r3.Succeed)
                return r3.ErrorValue.ToErrorResult<string>();
            return fileDir.ToOkResult();
        }
    }
}

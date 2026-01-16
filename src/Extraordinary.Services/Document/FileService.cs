using Extraordinary.Shared;
using Extraordinary.Shared.Local;
using Extraordinary.Shared.Message;
using Extraordinary.Shared.Origin;
using MediatR;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Extraordinary.Services.Document
{
    public class FileService(IMediator mediator) : IFileService
    {
        public async Task<ResponeReturn<string>> UnCompressAsync(string sourcefilePath, string destdirPath)
        {
            await mediator.Publish(ProgressBarINotification.Min("文件解压"));

            if (string.IsNullOrEmpty(sourcefilePath))
            {
                return "解压源文件地址不可为空".ToErrorResult<string>();
            }
            if (string.IsNullOrEmpty(destdirPath))
            {
                return "解压目标地址不可为空".ToErrorResult<string>();
            }

            if (Directory.Exists(destdirPath))
            {
                Directory.GetDirectories(destdirPath)
                    .ToList()
                    .ForEach(dir => Directory.Delete(dir, true));
                Directory.GetFiles(destdirPath)
                    .ToList()
                    .ForEach(file => File.Delete(file));
            }

            if (!Directory.Exists(destdirPath))
                Directory.CreateDirectory(destdirPath);

            ZipFile.ExtractToDirectory(sourcefilePath, destdirPath, true);

            await mediator.Publish(ProgressBarINotification.Max("文件解压"));
            return destdirPath.ToOkResult<string>();
        }
        public async Task<ResponeReturn<string>> SaveAsAsync(string sourcefilePath, string destfilePath)
        {
            try
            {
                string fileName = Path.GetFileName(sourcefilePath);
                if (File.Exists(destfilePath))
                    File.Delete(destfilePath);

                using (FileStream sourceStream = new FileStream(sourcefilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                using (FileStream destStream = new FileStream(destfilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                {
                    await sourceStream.CopyToAsync(destStream);
                }
                return destfilePath.ToOkResult<string>();
            }
            catch (Exception ex)
            {
                return ResponeReturn<string>.NewError($"另存为失败: {ex.Message}");
            }
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
            try
            {
                var str = JsonConvert.SerializeObject(config);
                await File.WriteAllTextAsync(filePath, str, Encoding.UTF8);
                return config.ToOkResult();
            }
            catch (Exception ex)
            {
                return ex.Message.ToErrorResult<T>();
            }
        }
        public async Task<ResponeReturn<T>> GetConfigAsync<T>(string filePath)
        {
            try
            {
                var str = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                var config = JsonConvert.DeserializeObject<T>(str);
                if (config == null)
                    return $"文件解析失败:{filePath}".ToErrorResult<T>();
                return config.ToOkResult<T>();
            }
            catch (Exception ex)
            {
                return ex.Message.ToErrorResult<T>();
            }
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

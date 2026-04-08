using Extraordinary.Shared.Origin;
using Extraordinary.WebApi.Document;
using Microsoft.AspNetCore.Mvc;

namespace Extraordinary.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IFileService fileService, IWebHostEnvironment env, ILogger<FilesController> logger)
        {
            _fileService = fileService;
            _env = env;
            _logger = logger;
        }

        private string PackagesDirectory
        {
            get
            {
                var webRoot = string.IsNullOrEmpty(_env.WebRootPath) ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot") : _env.WebRootPath;
                var dir = Path.Combine(webRoot, "packages");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                return dir;
            }
        }

        [HttpGet("list")]
        public IActionResult List()
        {
            try
            {
                var files = Directory.EnumerateFiles(PackagesDirectory)
                    .Select(f =>
                    {
                        var fi = new FileInfo(f);
                        return new Models.FileItem
                        {
                            Name = fi.Name,
                            SizeBytes = fi.Length,
                            Size = HumanReadableSize(fi.Length),
                            Modified = fi.LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                    })
                    .OrderByDescending(x => x.Modified)
                    .ToList();

                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "列出包文件失败");
                return Problem(detail: ex.Message, title: "无法列出文件");
            }
        }

        [HttpGet("download/{fileName}")]
        public IActionResult Download(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("文件名不可为空");

            var path = Path.Combine(PackagesDirectory, fileName);
            if (!System.IO.File.Exists(path))
                return NotFound();

            return PhysicalFile(path, "application/octet-stream", fileName);
        }

        /// <summary>
        /// 返回 OriginConfig.json 的完整内容（供前端管理界面或其他服务使用）
        /// </summary>
        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            try
            {
                var originPath = Path.Combine(string.IsNullOrEmpty(_env.WebRootPath) ? Directory.GetCurrentDirectory() : _env.WebRootPath, "OriginConfig.json");
                if (!System.IO.File.Exists(originPath))
                    return NotFound();

                var json = System.IO.File.ReadAllText(originPath);
                try
                {
                    var cfg = System.Text.Json.JsonSerializer.Deserialize<OriginConfig>(json);
                    return Ok(cfg ?? (object)json);
                }
                catch
                {
                    // 如果无法反序列化为对象，返回原始 json
                    return Content(json, "application/json");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取 OriginConfig.json 失败");
                return Problem(detail: ex.Message, title: "无法读取配置");
            }
        }

        /// <summary>
        /// 返回简化的版本信息（供客户端 App 查询最新版本使用）
        /// </summary>
        [HttpGet("version")]
        public IActionResult Version()
        {
            try
            {
                var originPath = Path.Combine(string.IsNullOrEmpty(_env.WebRootPath) ? Directory.GetCurrentDirectory() : _env.WebRootPath, "OriginConfig.json");
                if (!System.IO.File.Exists(originPath))
                    return NotFound();

                var json = System.IO.File.ReadAllText(originPath);
                var cfg = System.Text.Json.JsonSerializer.Deserialize<OriginConfig>(json);
                if (cfg == null)
                    return NotFound();

                return Ok(new
                {
                    PackageName = cfg.PackageName ?? cfg.PackageName,
                    AppVersion = cfg.AppVersion,
                    AppMD5Version = cfg.AppMD5Version
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取版本信息失败");
                return Problem(detail: ex.Message, title: "无法读取版本信息");
            }
        }

        [HttpPost("upload")]
        [RequestSizeLimit(1_500_000_000)] // 约 1.5GB，可根据需要调整
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string version = "")
        {
            if (file == null || file.Length == 0)
                return BadRequest("上传文件为空");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".zip" && ext != ".exe" && ext != ".msi")
                return BadRequest("仅允许上传 .zip/.exe/.msi 包");

            // 保存到临时文件
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + "_" + Path.GetFileName(file.FileName));
            try
            {
                await using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                {
                    await file.CopyToAsync(fs);
                }

                // 计算 MD5
                var md5Resp = await _fileService.GetFileMD5HashAsync(tempFile);
                if (!md5Resp.Succeed)
                    return Problem(detail: md5Resp.ErrorValue?.ToString() ?? "计算 MD5 失败", title: "文件校验失败");

                var md5 = md5Resp.ResultValue;
                // 版本优先使用传入参数，否则尝试从文件名中解析
                var parsedVersion = string.IsNullOrWhiteSpace(version) ? ParseVersionFromFileName(file.FileName) ?? DateTime.UtcNow.ToString("yyyyMMddHHmmss") : version.Trim();

                var baseName = Path.GetFileNameWithoutExtension(file.FileName);
                var destFileName = $"{baseName}-{parsedVersion}-{md5}{ext}";
                var destPath = Path.Combine(PackagesDirectory, destFileName);

                // 如果已存在同名文件，先删除（覆盖）
                if (System.IO.File.Exists(destPath))
                    System.IO.File.Delete(destPath);

                var saveResp = await _fileService.SaveAsAsync(tempFile, destPath);
                if (!saveResp.Succeed)
                    return Problem(detail: saveResp.ErrorValue?.ToString() ?? "保存文件失败", title: "文件保存失败");

                // 生成 OriginConfig 并保存到 webroot 根目录 (OriginConfig.json)
                var origin = new OriginConfig
                {
                    PackageName = destFileName,
                    AppVersion = parsedVersion,
                    AppMD5Version = md5
                };

                var originPath = Path.Combine(string.IsNullOrEmpty(_env.WebRootPath) ? Directory.GetCurrentDirectory() : _env.WebRootPath, "OriginConfig.json");
                var saveConfigResp = await _fileService.SaveConfigAsync(origin, originPath);
                if (!saveConfigResp.Succeed)
                    _logger.LogWarning("生成 OriginConfig.json 失败: {0}", saveConfigResp.ErrorValue?.ToString());

                // 返回已上传的元信息
                return CreatedAtAction(nameof(Download), new { fileName = destFileName }, new
                {
                    FileName = destFileName,
                    Size = HumanReadableSize(new FileInfo(destPath).Length),
                    Md5 = md5,
                    Version = parsedVersion,
                    OriginConfigPath = Path.GetFileName(originPath)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传文件处理失败");
                return Problem(detail: ex.Message, title: "上传失败");
            }
            finally
            {
                try { if (System.IO.File.Exists(tempFile)) System.IO.File.Delete(tempFile); } catch { }
            }
        }

        [HttpDelete("{fileName}")]
        public IActionResult Delete(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("文件名不可为空");

            var path = Path.Combine(PackagesDirectory, fileName);
            if (!System.IO.File.Exists(path))
                return NotFound();

            try
            {
                System.IO.File.Delete(path);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除文件失败 {file}", fileName);
                return Problem(detail: ex.Message, title: "删除失败");
            }
        }

        private static string HumanReadableSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{(bytes / 1024.0):F1} KB";
            if (bytes < 1024L * 1024 * 1024) return $"{(bytes / (1024.0 * 1024.0)):F1} MB";
            return $"{(bytes / (1024.0 * 1024.0 * 1024.0)):F1} GB";
        }

        private static string? ParseVersionFromFileName(string fileName)
        {
            // 常见版本号匹配，例如 1.0.2 或 v1.2.3
            var m = System.Text.RegularExpressions.Regex.Match(fileName, @"v?(\d+\.\d+(?:\.\d+)*)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (m.Success && m.Groups.Count > 1)
                return m.Groups[1].Value;
            return null;
        }
    }
}
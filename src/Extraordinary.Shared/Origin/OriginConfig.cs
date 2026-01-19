namespace Extraordinary.Shared.Origin
{
    /// <summary>
    /// 远端配置
    /// </summary>
    public class OriginConfig
    {
        /// <summary>
        /// 压缩包文件清单
        /// </summary>
        public string PackageName { get; set; } = "";
        /// <summary>
        /// 服务器程序版本
        /// </summary>
        public string AppVersion { get; set; } = "";
        /// <summary>
        /// 服务器程序MD5版本
        /// </summary>
        public string AppMD5Version { get; set; } = "";
    }
}

namespace Extraordinary.Shared.Local
{
    /// <summary>
    /// 本地高级配置
    /// </summary>
    public class LocalAdvancedConfig
    {
        /// <summary>
        /// 需要保留的文件扩展名，多个用逗号分隔
        /// </summary>
        public string Retain { get; set; } = string.Empty;
        /// <summary>
        /// 需要保留的具体文件列表，支持相对路径和绝对路径，多个用逗号分隔
        /// </summary>
        public string[] RetainFile { get; set; } = [];
    }
}

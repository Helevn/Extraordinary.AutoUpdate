namespace Extraordinary.Shared
{
    /// <summary>
    /// 基础配置选项
    /// <para/>
    /// 此配置选项是定义在appsettings.json中的BaseConfigOpt节点下的映射类
    /// <para/>
    /// 未配置appsettings.json的情况下，使用默认值
    /// </summary>
    public class BaseConfigOpt
    {
        public const string DefaultLocalConfigPath = "LocalConfig.json";
        public const string DefaultOriginConfigName = "OriginConfig.json";
        /// <summary>
        /// 本地配置文件的存储路径
        /// </summary>
        public string LocalConfigPath { get; set; } = "";
        /// <summary>
        /// 服务器配置文件的名称
        /// </summary>
        public string OriginConfigName { get; set; } = "";
    }
}

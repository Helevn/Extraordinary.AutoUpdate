namespace Extraordinary.App.Config
{
    public class UpdateConfig
    {
        /// <summary>
        /// 服务端地址
        /// </summary>
        public string ServerUrl { get; set; } = "http://127.0.0.1:5000";
        /// <summary>
        /// 下载位置
        /// </summary>
        public string DownloadPath { get; set; } = "";
        /// <summary>
        /// 程序名称
        /// </summary>
        public string AppName { get; set; } = "";
        /// <summary>
        /// 当前程序的MD5版本信息
        /// </summary>
        public string CurrentMD5Version { get; set; } = "";
        /// <summary>
        /// 程序安装位置
        /// </summary>
        public string InstallationPath { get; set; } = "";

        /// <summary>
        /// 自启动
        /// </summary>
        public bool Self_Starting { get; set; } = true;
    }
}

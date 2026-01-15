using Extraordinary.App.Views.UserCtrl;
using Extraordinary.Services.Application;
using Extraordinary.Services.Document;
using Extraordinary.Shared;
using Extraordinary.Shared.Local;
using Extraordinary.Shared.Origin;
using HandyControl.Controls;
using Microsoft.Extensions.Options;
using Reactive.Bindings;
using System.IO;

namespace Extraordinary.App.ViewModels
{
    public class RealtimeViewModel
    {
        private readonly IServiceProvider _ssf;
        private readonly IOptions<BaseConfigOpt> _configopt;
        private readonly IFileService _fileService;
        private readonly IProcessService _process;
        private readonly AppViewModel _appViewModel;

        private string LocalConfigPath => string.IsNullOrEmpty(this._configopt.Value.LocalConfigPath)
            ? BaseConfigOpt.DefaultLocalConfigPath : this._configopt.Value.LocalConfigPath;
        private string OriginConfigName => string.IsNullOrEmpty(this._configopt.Value.OriginConfigName)
            ? BaseConfigOpt.DefaultOriginConfigName : this._configopt.Value.OriginConfigName;
        public RealtimeViewModel(IServiceProvider ssf, IOptions<BaseConfigOpt> configopt, IFileService fileService, IProcessService process, AppViewModel appViewModel)
        {
            this._ssf = ssf;
            this._configopt = configopt;
            this._fileService = fileService;
            this._process = process;
            this._appViewModel = appViewModel;
            this.ProgressMaxValue = new ReactiveProperty<long>(long.MaxValue);
            this.ProgressValue = new ReactiveProperty<long>(0);
            this.ProgressAction = new ReactiveProperty<string>("");
            this.AppVersion = new ReactiveProperty<string>("");
            this.AppMD5Version = new ReactiveProperty<string>("");

            this.CmdUpdate = new ReactiveCommand().WithSubscribe(() =>
            {
                var thread = new Thread(async () =>
                {
                    try
                    {
                        var r = await _process.UpdateAsync(this.LocalConfigPath, this.OriginConfigName);
                        if (r.Succeed)
                        {
                            var ok = r.ResultValue;
                            this.AppVersion.Value = ok.CurrentVersion;
                            this.AppMD5Version.Value = ok.CurrentMD5Version;
                            Growl.SuccessGlobal("检查更新完成");
                        }
                        else
                        {
                            MessageBox.Show(r.ErrorValue);
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                });
                thread.Start();
            });
            this.CmdCreate = new ReactiveCommand<string>().WithSubscribe(async (path) =>
            {
                try
                {
                    var r1 = await _fileService.GetFileMD5HashAsync(path);
                    if (r1.Succeed)
                    {
                        var fileinfo = new FileInfo(path);
                        var fileDir = fileinfo.Directory?.FullName ?? "";
                        var fileName = this.OriginConfigName;
                        var filePath = Path.Combine(fileDir, fileName);
                        var userinput = UserInputStringDialog.OpenDailog("请输入当前程序的版本号信息");
                        var config = new OriginConfig { PackageName = fileinfo.Name, AppVersion = userinput, AppMD5Version = r1.ResultValue };
                        var r2 = await _fileService.SaveConfigAsync(config, filePath);
                        if (r2.Succeed)
                        {
                            Growl.SuccessGlobal("生成成功");
                        }
                        else
                        {
                            MessageBox.Show(r2.ErrorValue);
                        }
                    }
                    else
                    {
                        MessageBox.Show(r1.ErrorValue);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
            this.CmdStart = new ReactiveCommand<string>().WithSubscribe(async (path) =>
            {
                try
                {
                    var r1 = await _fileService.GetConfigAsync<LocalConfig>(this.LocalConfigPath);
                    if (r1.Succeed)
                    {
                        var updateConfig = r1.ResultValue;
                        var r2 = await _process.StartProcessAsync(updateConfig.InstallationPath, updateConfig.AppName);
                        if (r2.Succeed)
                        {
                            Growl.SuccessGlobal("启动成功");
                        }
                        else
                        {
                            MessageBox.Show(r2.ErrorValue);
                        }
                    }
                    else
                    {
                        MessageBox.Show(r1.ErrorValue);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
            this.CmdGotoParamConfig = new ReactiveCommand().WithSubscribe(() => _appViewModel.NavigateTo(UrlDefines.URL_Params));
        }

        public void RefreshProgressBar(string action, long maxVaue, long value)
        {
            this.ProgressAction.Value = action;
            this.ProgressMaxValue.Value = maxVaue;
            this.ProgressValue.Value = value;
        }

        public void Close(bool need)
        {
            if (need)
                App.Current.Shutdown();
        }

        #region Reactive
        public ReactiveCommand CmdGotoParamConfig { get; }
        public ReactiveCommand CmdUpdate { get; }
        public ReactiveCommand<string> CmdCreate { get; }
        public ReactiveCommand<string> CmdStart { get; }
        public ReactiveProperty<long> ProgressMaxValue { get; set; }
        public ReactiveProperty<long> ProgressValue { get; set; }
        public ReactiveProperty<string> ProgressAction { get; set; }

        public ReactiveProperty<string> AppVersion { get; set; }
        public ReactiveProperty<string> AppMD5Version { get; set; }
        #endregion
    }
}

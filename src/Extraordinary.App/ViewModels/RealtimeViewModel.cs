using Extraordinary.App.Config;
using Extraordinary.App.Opt;
using Extraordinary.App.Services;
using HandyControl.Controls;
using Microsoft.Extensions.Options;
using Reactive.Bindings;
using System.IO;

namespace Extraordinary.App.ViewModels
{
    public class RealtimeViewModel
    {
        private const string DefaultUpdateConfigFilePath = "UpdateConfig.json";
        private const string DefaultUrlConfigFileName = "UrlConfig.json";

        private readonly IServiceProvider _ssf;
        private readonly IOptionsMonitor<ConfigOpt> _configopt;
        private readonly IFileServices _fileServices;

        private string UpdateConfigFilePath => string.IsNullOrEmpty(this._configopt.CurrentValue.Path) ? DefaultUpdateConfigFilePath : this._configopt.CurrentValue.Path;
        private string UrlConfigName => string.IsNullOrEmpty(this._configopt.CurrentValue.UrlConfigName) ? DefaultUrlConfigFileName : this._configopt.CurrentValue.UrlConfigName;
        public RealtimeViewModel(IServiceProvider ssf, IOptionsMonitor<ConfigOpt> configopt, IFileServices fileServices)
        {
            this._ssf = ssf;
            this._configopt = configopt;
            this._fileServices = fileServices;

            this.ServerUrl = new ReactiveProperty<string>();
            this.DownloadPath = new ReactiveProperty<string>();
            this.AppName = new ReactiveProperty<string>();
            this.CurrentMD5Version = new ReactiveProperty<string>();
            this.InstallationPath = new ReactiveProperty<string>();
            this.Self_Starting = new ReactiveProperty<bool>();
            this.Kill_App = new ReactiveProperty<bool>();

            this.ProgressMaxValue = new ReactiveProperty<long>(long.MaxValue);
            this.ProgressValue = new ReactiveProperty<long>(0);
            this.ProgressAction = new ReactiveProperty<string>("");

            this.CmdUpdate = new ReactiveCommand().WithSubscribe(() =>
            {
                var thread = new Thread(async () =>
                {
                    try
                    {
                        var r = await _fileServices.UpdateAsync(this.UpdateConfigFilePath, this.UrlConfigName, config =>
                        {
                            UIHelper.RunInUIThread(pl =>
                            {
                                Refresh(config);
                            });
                            return "";
                        });
                        if (r.Succeed)
                        {
                            Growl.SuccessGlobal(r.ResultValue);
                            UIHelper.RunInUIThread(pl =>
                            {
                                this.Close(this.Self_Starting.Value);
                            });
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
            this.CmdLoad = new ReactiveCommand<bool?>().WithSubscribe(async start =>
            {
                try
                {
                    UpdateConfig config = null;
                    var res = await _fileServices.GetConfigAsync<UpdateConfig>(this.UpdateConfigFilePath);
                    if (res.Succeed)
                    {
                        config = res.ResultValue;
                        Refresh(start, config);
                    }
                    else
                    {
                        var initres = await _fileServices.SaveConfigAsync(new UpdateConfig(), this.UpdateConfigFilePath);
                        if (initres.Succeed)
                        {
                            config = initres.ResultValue;
                            Refresh(start, config);
                        }
                        else
                        {
                            MessageBox.Show(initres.ErrorValue);
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                void Refresh(bool? start, UpdateConfig config)
                {
                    this.Refresh(config);
                    if (config.Self_Starting && (start ?? false))
                        this.CmdUpdate.Execute();
                }
            });
            this.CmdSave = new ReactiveCommand().WithSubscribe(async () =>
            {
                try
                {
                    var config = new UpdateConfig
                    {
                        ServerUrl = this.ServerUrl.Value,
                        AppName = this.AppName.Value,
                        DownloadPath = this.DownloadPath.Value,
                        CurrentMD5Version = this.CurrentMD5Version.Value,
                        InstallationPath = this.InstallationPath.Value,
                        Self_Starting = this.Self_Starting.Value,
                        Kill_App = this.Kill_App.Value,
                    };
                    await _fileServices.SaveConfigAsync(config, this.UpdateConfigFilePath);
                    this.Refresh(config);
                    Growl.SuccessGlobal("保存成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
            this.CmdCreate = new ReactiveCommand<string>().WithSubscribe(async (path) =>
            {
                try
                {
                    var r1 = await _fileServices.GetFileMD5HashAsync(path);
                    if (r1.Succeed)
                    {
                        var fileinfo = new FileInfo(path);
                        var fileDir = fileinfo.Directory?.FullName ?? "";
                        var fileName = this.UrlConfigName;
                        var filePath = Path.Combine(fileDir, fileName);
                        var config = new UrlConfig { PackageName = fileinfo.Name, AppMD5Version = r1.ResultValue };
                        var r2 = await _fileServices.SaveConfigAsync(config, filePath);
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
                    var r1 = await _fileServices.GetConfigAsync<UpdateConfig>(this.UpdateConfigFilePath);
                    if (r1.Succeed)
                    {
                        var updateConfig = r1.ResultValue;
                        var r2 = await _fileServices.StartProcessAsync(updateConfig.InstallationPath, updateConfig.AppName);
                        if (r2.Succeed)
                        {
                            Growl.SuccessGlobal("启动成功");
                            //MessageBox.Show(r1.ErrorValue);
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

            this.CmdLoad.Execute(true);
        }

        #region Method
        private void Refresh(UpdateConfig config)
        {
            this.ServerUrl.Value = config.ServerUrl;
            this.DownloadPath.Value = config.DownloadPath;
            this.AppName.Value = config.AppName;
            this.CurrentMD5Version.Value = config.CurrentMD5Version;
            this.InstallationPath.Value = config.InstallationPath;
            this.Self_Starting.Value = config.Self_Starting;
            this.Kill_App.Value = config.Kill_App;
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
        #endregion

        #region Reactive
        public ReactiveCommand CmdUpdate { get; }
        public ReactiveCommand<bool?> CmdLoad { get; }
        public ReactiveCommand CmdSave { get; }
        public ReactiveCommand<string> CmdCreate { get; }
        public ReactiveCommand<string> CmdStart { get; }

        public ReactiveProperty<string> ServerUrl { get; set; }
        public ReactiveProperty<string> DownloadPath { get; set; }
        public ReactiveProperty<string> AppName { get; set; }
        public ReactiveProperty<string> CurrentMD5Version { get; set; }
        public ReactiveProperty<string> InstallationPath { get; set; }
        public ReactiveProperty<bool> Self_Starting { get; set; }
        public ReactiveProperty<bool> Kill_App { get; set; }


        public ReactiveProperty<long> ProgressMaxValue { get; set; }
        public ReactiveProperty<long> ProgressValue { get; set; }
        public ReactiveProperty<string> ProgressAction { get; set; }
        #endregion
    }
}

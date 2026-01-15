using Extraordinary.Services.Document;
using Extraordinary.Shared;
using Extraordinary.Shared.Local;
using HandyControl.Controls;
using Microsoft.Extensions.Options;
using Reactive.Bindings;

namespace Extraordinary.App.ViewModels
{
    public class ParamMgmtVM
    {
        private readonly IServiceProvider _ssf;
        private readonly IOptions<BaseConfigOpt> _configopt;
        private readonly IFileService _fileService;
        private readonly AppViewModel _appViewModel;

        private string LocalConfigPath => string.IsNullOrEmpty(this._configopt.Value.LocalConfigPath)
            ? BaseConfigOpt.DefaultLocalConfigPath : this._configopt.Value.LocalConfigPath;
        public ParamMgmtVM(IServiceProvider ssf, IOptions<BaseConfigOpt> configopt, IFileService fileService, AppViewModel appViewModel)
        {
            this._ssf = ssf;
            this._configopt = configopt;
            this._fileService = fileService;
            this._appViewModel = appViewModel;
            this.ServerUrl = new ReactiveProperty<string>();
            this.DownloadPath = new ReactiveProperty<string>();
            this.AppName = new ReactiveProperty<string>();
            this.CurrentMD5Version = new ReactiveProperty<string>();
            this.InstallationPath = new ReactiveProperty<string>();
            this.Self_Starting = new ReactiveProperty<bool>(true);
            this.Kill_App = new ReactiveProperty<bool>(true);

            this.CmdLoad = new ReactiveCommand<bool?>().WithSubscribe(async start =>
            {
                try
                {
                    LocalConfig config = null;
                    var res = await _fileService.GetConfigAsync<LocalConfig>(this.LocalConfigPath);
                    if (res.Succeed)
                    {
                        config = res.ResultValue;
                        Refresh(start, config);
                    }
                    else
                    {
                        var initres = await _fileService.SaveConfigAsync(new LocalConfig(), this.LocalConfigPath);
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

                void Refresh(bool? start, LocalConfig config)
                {
                    this.Refresh(config);
                }
            });
            this.CmdSave = new ReactiveCommand().WithSubscribe(async () =>
            {
                try
                {
                    var config = new LocalConfig
                    {
                        ServerUrl = this.ServerUrl.Value,
                        AppName = this.AppName.Value,
                        DownloadPath = this.DownloadPath.Value,
                        CurrentMD5Version = this.CurrentMD5Version.Value,
                        InstallationPath = this.InstallationPath.Value,
                        Self_Starting = this.Self_Starting.Value,
                        Kill_App = this.Kill_App.Value,
                    };
                    await _fileService.SaveConfigAsync(config, this.LocalConfigPath);
                    this.Refresh(config);
                    Growl.SuccessGlobal("保存成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
            this.CmdGoBack = new ReactiveCommand().WithSubscribe(() => _appViewModel.NavigateTo(UrlDefines.URL_Realtime));
            this.CmdLoad.Execute(true);
        }

        private void Refresh(LocalConfig config)
        {
            this.ServerUrl.Value = config.ServerUrl;
            this.DownloadPath.Value = config.DownloadPath;
            this.AppName.Value = config.AppName;
            this.CurrentMD5Version.Value = config.CurrentMD5Version;
            this.InstallationPath.Value = config.InstallationPath;
            this.Self_Starting.Value = config.Self_Starting;
            this.Kill_App.Value = config.Kill_App;
        }
        #region Reactive
        public ReactiveCommand<bool?> CmdLoad { get; }
        public ReactiveCommand CmdSave { get; }
        public ReactiveCommand CmdGoBack { get; }

        public ReactiveProperty<string> ServerUrl { get; set; }
        public ReactiveProperty<string> DownloadPath { get; set; }
        public ReactiveProperty<string> AppName { get; set; }
        public ReactiveProperty<string> CurrentMD5Version { get; set; }
        public ReactiveProperty<string> InstallationPath { get; set; }
        public ReactiveProperty<bool> Self_Starting { get; set; }
        public ReactiveProperty<bool> Kill_App { get; set; }
        #endregion
    }
}

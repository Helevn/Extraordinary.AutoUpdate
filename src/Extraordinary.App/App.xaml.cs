using Extraordinary.App.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Extraordinary.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Mutex _singletonMutex;

        public App()
        {
            var appname = typeof(App).AssemblyQualifiedName;
            this._singletonMutex = new Mutex(true, appname, out var createdNew);
            //if (!createdNew)
            //{
            //    MessageBox.Show($"软件已经启动！不可重复启动！");
            //    Environment.Exit(0);
            //    return;
            //}

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            InitHost();
        }

        private void InitHost()
        {
            this._host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder
                        .SetBasePath(context.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
                    builder.AddEnvironmentVariables();
                })
                .ConfigureServices(HostStartup.ConfigureServices)
                .Build();
            this.RootServiceProvider = this._host.Services;
        }
        public IHost _host { get; private set; }
        public IServiceProvider RootServiceProvider { get; internal set; }
        private CancellationTokenSource cts = new CancellationTokenSource();

        private void SigninOperator(IServiceProvider sp)
        {
            var appvm = sp.GetRequiredService<AppViewModel>();
            var mainWin = sp.GetRequiredService<MainWindow>();
            mainWin.Show();
            appvm.NavigateTo(UrlDefines.URL_Realtime);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            try
            {
                SigninOperator(this.RootServiceProvider);

                var thread = new Thread(async () =>
                {
                    try
                    {
                        // do sth before running
                        await _host.RunAsync(cts.Token);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        this.Shutdown();
                    }
                });
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                this._singletonMutex.ReleaseMutex();
            }
            finally
            {
                using (_host)
                {
                    var lieftime = _host.Services.GetRequiredService<IHostApplicationLifetime>();
                    lieftime.StopApplication();
                }
                //base.OnExit(e);
                Environment.Exit(0);
            }
        }
    }
}

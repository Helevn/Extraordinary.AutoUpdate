using Extraordinary.App.ViewModels;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace Extraordinary.App.Views
{
    /// <summary>
    /// Interaction logic for ParamMgmt.xaml
    /// </summary>
    public partial class ParamMgmt : Page
    {
        public ParamMgmt(ParamMgmtVM vm)
        {
            this.DataContext = vm;
            InitializeComponent();
            Vm = vm;
        }

        public ParamMgmtVM Vm { get; }

        private void BtnBrowseDownload_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "请选择文件夹",
            };

            if (dialog.ShowDialog() == true)
            {
                var folderName = dialog.FolderName;
                if (!string.IsNullOrEmpty(folderName))
                {
                    this.Vm.DownloadPath.Value = folderName;
                }
            }
        }

        private void BtnBrowseInstall_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "请选择文件夹",
            };

            if (dialog.ShowDialog() == true)
            {
                var folderName = dialog.FolderName;
                if (!string.IsNullOrEmpty(folderName))
                {
                    this.Vm.InstallationPath.Value = folderName;
                }
            }
        }
    }
}

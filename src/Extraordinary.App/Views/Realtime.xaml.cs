using Extraordinary.App.ViewModels;
using System.Windows.Controls;
using System;
using System.Windows;
using Microsoft.Win32;

namespace Extraordinary.App.Views
{
    /// <summary>
    /// Interaction logic for RealtimePage.xaml
    /// </summary>
    public partial class Realtime : Page
    {
        public Realtime(RealtimeViewModel vm)
        {
            this.DataContext = vm;
            InitializeComponent();
            Vm = vm;
        }

        public RealtimeViewModel Vm { get; }

        private void Create_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "压缩文件(*.zip)|*.zip",
                FilterIndex = 1,
                Title = "请选择",
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Vm.CmdCreate.Execute(openFileDialog.FileName);
            }
        }
    }
}

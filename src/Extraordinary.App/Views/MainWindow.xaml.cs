using Extraordinary.App.ViewModels;
using System.Windows;
using System;
using System.ComponentModel;

namespace Extraordinary.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public AppViewModel appvm { get; }
        public MainWindow(AppViewModel vm)
        {
            InitializeComponent();
            this.appvm = vm;
            this.appvm.CurrentPage.Subscribe(page =>
            {
                if (page != null)
                {
                    this.navWin.Navigate(page);
                }
            });
            this.DataContext = this.appvm;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var res = MessageBox.Show("是否真的要退出？", "退出程序确认", MessageBoxButton.YesNo);
            if (!res.Equals(MessageBoxResult.Yes))
            {
                e.Cancel = true;
            }
        }
    }
}
using System.Windows;

namespace Extraordinary.App.Views.UserCtrl
{
    /// <summary>
    /// UserInputStringDialog.xaml 的交互逻辑
    /// </summary>
    public partial class UserInputStringDialog : Window
    {
        public UserInputStringDialog(string title = "")
        {
            this.UserTitle = title;
            InitializeComponent();
        }

        public static string OpenDailog(string title)
        {
            var dialog = new UserInputStringDialog(title);
            dialog.ShowDialog();
            return dialog.StringValue;
        }


        public string UserTitle
        {
            get { return (string)GetValue(UserTitleProperty); }
            set { SetValue(UserTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserTitleProperty =
            DependencyProperty.Register(nameof(UserTitle), typeof(string), typeof(UserInputStringDialog), new PropertyMetadata(string.Empty));

        public string StringValue
        {
            get { return (string)GetValue(StringValueProperty); }
            set { SetValue(StringValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StringValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StringValueProperty =
            DependencyProperty.Register(nameof(StringValue), typeof(string), typeof(UserInputStringDialog), new PropertyMetadata(string.Empty));

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

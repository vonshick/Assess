using System.Windows;

namespace UTA.Views
{
    public partial class AboutDialogView
    {
        public AboutDialogView()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }
    }
}
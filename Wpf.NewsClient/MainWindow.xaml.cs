using System.Windows;

namespace Wpf.NewsClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            ((NewsViewModel)DataContext).RefreshNewsCommand.Execute(this);
        }
    }
    class NewsViewModel : Portable.NewsViewLogic.NewsViewModel { }
}
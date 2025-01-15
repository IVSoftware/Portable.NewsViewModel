using Portable.NewsViewLogic;

namespace Maui.NewsClient
{
    public partial class MainPage : ContentPage
    {
        public MainPage() => InitializeComponent();
        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((NewsViewModel)BindingContext).RefreshNewsCommand.Execute(this);
        }
    }
    class NewsViewModel : Portable.NewsViewLogic.NewsViewModel { }
}

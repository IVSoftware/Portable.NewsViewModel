Your code shows a method descriptively named `UpdateNewsView` and it would seem like a reasonable and fair assumption that your intent is to retrieve (conceptually) "the latest news" from "some news source" and display it in a "news view control". You said:

> I thought cleaning the observableCollection and adding items was going to work, but I was wrong.

OK, so you weren't completely wrong. Try going about it this way:

- Declare your ObservableCollection as a public property in the `BindingContext` (i.e. the ViewModel).
- Leave it alone, and do not reassign it (because this would require a different kind of change notification).
- Bind the `ItemsSource` property of your `CollectionView` (or other control) to that collection.
- Then, _rely on the `INotifyCollectionChanged` events that are automatically generated when you modify the list_ as you clean and add items as shown in this snippet.

___
**Minimal Example**

This ViewModel retrieves some "news" from a web API, showing one correct way to do the collection binding.

~~~csharp
public class NewsViewModel : INotifyPropertyChanged
{
    // The property itself should not change, and we make sure of that by making it 'get'
    // only. However, changes to this list will fire INotifyCollectionChanged events.
    public IList Headlines { get; } = new ObservableCollection<NewsHeadline>();
    // =============================================================================

    public NewsViewModel()
    {
        RefreshNewsCommand = new Command(OnRefreshNews);
    }
    public ICommand RefreshNewsCommand { get; }

    private string _apiUrl = $"https://catfact.ninja/facts?limit={RandomRequestSize}";
    private async void OnRefreshNews(object? sendere)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string newsData = await response.Content.ReadAsStringAsync();
                    if (JsonConvert.DeserializeObject<NewsWrapper>(newsData) is { } news)
                    {
                        _apiUrl =
                            news.Next_Page_Url is null
                            ? $"https://catfact.ninja/facts?limit={RandomRequestSize}"
                            : $"{news.Next_Page_Url}&limit={RandomRequestSize}";
                        if (news.Data is List<NewsHeadline> headlines)
                        {
                            // "cleaning the observableCollection..."
                            Headlines.Clear();
                            // "...adding items..."
                            foreach (NewsHeadline headline in headlines)
                            {
                                Headlines.Add(headline);
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"Failed to fetch news. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
    static Random _rando = new Random(2);
    private static int RandomRequestSize => _rando.Next(2, 5);

    // Although INotifyPropertyChanged is not utilized in this
    // example, we'll include it for the sake of inheritance.
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public event PropertyChangedEventHandler? PropertyChanged;
}
~~~

___

**XAML**

Kludging a `CollectionView` onto the default MAUI app page gives us a way to display the results.


[![MAUI client][1]][1]

___

~~~xaml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.NewsClient.MainPage"
             xmlns:local="clr-namespace:Maui.NewsClient">
    <ContentPage.BindingContext>
        <local:NewsViewModel/>
    </ContentPage.BindingContext>
    <ScrollView>
        <VerticalStackLayout
            Padding="30,0"
            Spacing="25">
            <Image
                Source="dotnet_bot.png"
                HeightRequest="185"
                Aspect="AspectFit"
                SemanticProperties.Description="dot net bot in a race car number eight" />
            <Label
                Text="Welcome to &#10;News Headlines"
                Style="{StaticResource SubHeadline}"
                SemanticProperties.HeadingLevel="Level2"
                SemanticProperties.Description="Welcome to dot net Multi platform App U I" />
            <Button
                Text="Refresh News" 
                SemanticProperties.Hint="Retrieves the latest news headlines"
                Command="{Binding RefreshNewsCommand}"
                HorizontalOptions="Fill" />            
            <CollectionView
                ItemsSource="{Binding Headlines}"
                BackgroundColor="Azure"
                Margin="0,10,0,0">
                <CollectionView.ItemTemplate>
                    <DataTemplate>
                        <StackLayout Padding="10" Spacing="5" BackgroundColor="#f5f5f5">
                            <Label Text="{Binding LocalTime, StringFormat='{0:MMMM dd, yyyy HH:mm:ss}'}"
                                   FontAttributes="Italic"
                                   FontSize="14"
                                   TextColor="#888" />
                            <Label Text="{Binding Headline}"
                                   FontSize="18"
                                   FontAttributes="Bold"
                                   TextColor="#333" />
                        </StackLayout>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
~~~

___

**Why it Matters**

Don't overlook the "bigger idea" of MVVM as being a decoupling of the UI from the logic. If this is done correctly, the `NewViewModel` is interchangeable, for example between Maiu and Wpf or WinForms.


[![WPF client][2]][2]


  [1]: https://i.sstatic.net/VzAHvoth.png
  [2]: https://i.sstatic.net/nSBrTpJP.png
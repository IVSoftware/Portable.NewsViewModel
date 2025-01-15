﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Portable.NewsViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace Maui.NewsClient
{
    public partial class MainPage : ContentPage
    {
        public MainPage() => InitializeComponent();
    }
    class NewsViewModel : INotifyPropertyChanged
    {
        static Random _rando = new Random(1);
        private static int RandomRequestSize => _rando.Next(2, 10);
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
                            _apiUrl = news.Next_Page_Url;
                            if (news.Data is List<NewsHeadline> headlines)
                            {
                                Headlines.Clear();
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
        /// <summary>
        /// Should not change, and we make sure of that my making it get only.
        /// Nevertheless, changes to this list will fire INotifyCollectionChanged events.
        /// </summary>
        public ObservableCollection<NewsHeadline> Headlines { get; } = new ObservableCollection<NewsHeadline>();
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    class NewsWrapper
    {
        public List<NewsHeadline>? Data { get; set; }
        public int Current_Page { get; set; }
        public int Last_Page { get; set; }

        public string Next_Page_Url { get; set; }
    }
    class NewsHeadline
    {
        [JsonProperty("Fact")]
        public string? Headline { get; set; }
        public DateTimeOffset LocalTime { get; set; } = DateTimeOffset.Now;
    }
}
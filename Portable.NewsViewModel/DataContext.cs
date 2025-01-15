using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;

namespace Portable.NewsViewModel
{
    public class DataContext : INotifyPropertyChanged
    {
        /// <summary>
        /// Should not change, and we make sure of that my making it get only.
        /// Nevertheless, changes to this list will fire INotifyCollectionChanged events.
        /// </summary>
        public IEnumerable<NewsItem> NewsItems { get; } = new ObservableCollection<NewsItem>();

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler? PropertyChanged;
    }
    public class NewsItem
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [NotNull]
        public string Headline { get; set; } = Guid.NewGuid().ToString();

        [NotNull]
        public string? TimeStampString 
        { 
            get; 
            set; 
        }

        [Ignore]
        public DateTimeOffset TimeStamp
        {
            get => 
                DateTimeOffset.TryParse(TimeStampString, out var dto) 
                ? dto
                : DateTimeOffset.MinValue;
            set => TimeStampString = value.ToString();
        }
    }
}

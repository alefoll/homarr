using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using Windows.UI;

namespace homarr.Movie {
    public sealed partial class Movies : Page {
        public ObservableCollection<Movie> MovieList = new ObservableCollection<Movie>();

        private readonly Radarr Radarr;

        public Movies() {
            this.InitializeComponent();

            this.Radarr = new Radarr(Settings.GetSetting("RadarrUrl"), Settings.GetSetting("RadarrApiKey"));

            this.getMovies();
        }

        private async void getMovies() {
            foreach (var movie in await this.Radarr.GetMovies()) {
                this.MovieList.Add(movie);
            }
        }

        private void OnMovieClick(object sender, ItemClickEventArgs e) {
            var movie = e.ClickedItem as Movie;

            movie.Play();
        }

        private void OnMenuItemPlay(object sender, RoutedEventArgs e) {
            var movie = (sender as MenuFlyoutItem).DataContext as Movie;

            movie.Play();
        }

        private async void OnMenuItemDelete(object sender, RoutedEventArgs e) {
            var movie = (sender as MenuFlyoutItem).DataContext as Movie;

            await movie.Delete();

            this.MovieList.Remove(movie);
        }

        private void OnBorderPointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e) {
            var element = sender as Border;

            element.BorderBrush = new SolidColorBrush(Color.FromArgb(45, 255, 255, 255));
            element.Opacity = 0.7;
        }

        private void OnBorderPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e) {
            var element = sender as Border;

            element.BorderBrush = new SolidColorBrush(Colors.Transparent);
            element.Opacity = 1;
        }
    }
}

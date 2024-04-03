using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace homarr.Movie {
    public sealed partial class Movies : Page {
        public ObservableCollection<Movie> MovieList = new ObservableCollection<Movie>();

        private readonly Radarr Radarr;

        public Movies() {
            this.InitializeComponent();

            this.Radarr = new Radarr(Settings.GetSetting("RadarrUrl"), Settings.GetSetting("RadarrApiKey"));

            getMovies();
        }

        private async void getMovies() {
            foreach (var movie in await this.Radarr.GetMovies()) {
                this.MovieList.Add(movie);
            }
        }

        private void OnMovieClick(object sender, RoutedEventArgs e) {
            var movie = (sender as Button).DataContext as Movie;

            movie.Play();
        }
    }
}

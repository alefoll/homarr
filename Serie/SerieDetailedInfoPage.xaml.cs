using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace homarr.Serie {
    public sealed partial class SerieDetailedInfoPage : Page {
        public record class Season(
            int SeasonNumber = 0,
            List<Episode> Episodes = null
        );

        private Serie SelectedSerie;
        private ObservableCollection<Season> Seasons = new();

        public SerieDetailedInfoPage() {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            var serie = (Serie)e.Parameter;

            SelectedSerie = serie;

            _ = this.PopulateSeasons();
        }

        private async Task PopulateSeasons() {
            var episodes = await this.SelectedSerie.GetEpisodesOnDisk();

            if (episodes.Count() == 0) {
                Frame.GoBack();
            }

            var seasons = episodes
                .GroupBy(episode => episode.SeasonNumber)
                .Select(episodes => {
                    return new Season {
                        SeasonNumber = episodes.First().SeasonNumber,
                        Episodes = episodes.ToList<Episode>(),
                    };
                });

            foreach (var season in seasons) {
                this.Seasons.Add(season);
            }
        }

        private void OnGoBackClick(object sender, RoutedEventArgs e) {
            Frame.GoBack();
        }

        private async void OnDeleteFile(object sender, RoutedEventArgs e) {
            var episode = (sender as Button).DataContext as Episode;

            await SelectedSerie.DeleteEpisode(episode);

            this.Seasons.Clear();

            await this.PopulateSeasons();
        }

        private void OnListClick(object sender, ItemClickEventArgs e) {
            var episode = e.ClickedItem as Episode;

            episode.Play();
        }

        private void OnListClick(object sender, RoutedEventArgs e) {
            var episode = (sender as Button).DataContext as Episode;

            episode.Play();
        }

        private void OnMenuItemPlay(object sender, RoutedEventArgs e) {
            var episode = (sender as MenuFlyoutItem).DataContext as Episode;

            episode.Play();
        }

        private async void OnMenuItemDelete(object sender, RoutedEventArgs e) {
            var episode = (sender as MenuFlyoutItem).DataContext as Episode;

            await SelectedSerie.DeleteEpisode(episode);

            this.Seasons.Clear();

            await this.PopulateSeasons();
        }
    }
}

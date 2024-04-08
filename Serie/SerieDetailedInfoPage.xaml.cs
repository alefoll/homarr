using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;

namespace homarr.Serie {
    public sealed partial class SerieDetailedInfoPage : Page {
        private Serie SelectedSerie;

        public SerieDetailedInfoPage() {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            var serie = (Serie)e.Parameter;

            this.SelectedSerie = serie;

            if (this.SelectedSerie.Episodes.Count() == 0) {
                Frame.GoBack();
            }
        }

        private void OnGoBackClick(object sender, RoutedEventArgs e) {
            Frame.GoBack();
        }

        private async void OnDeleteFile(object sender, RoutedEventArgs e) {
            var episode = (sender as Button).DataContext as Episode;

            await this.SelectedSerie.DeleteEpisode(episode);

            if (this.SelectedSerie.Episodes.Count() == 0) {
                Frame.GoBack();
            }
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

            if (this.SelectedSerie.Episodes.Count() == 0) {
                Frame.GoBack();
            }
        }
    }
}

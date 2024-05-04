using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI;

namespace homarr.Serie {
    public sealed partial class Series : Page {
        public readonly ObservableCollection<Serie> SerieList = new();

        private readonly Sonarr Sonarr;

        public Series() {
            this.InitializeComponent();

            this.Sonarr = new Sonarr(Settings.GetSetting("SonarrUrl"), Settings.GetSetting("SonarrApiKey"));

            _ = GetSeries();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
        }

        private async Task GetSeries() {
            foreach (var serie in await this.Sonarr.GetSeries()) {
                this.SerieList.Add(serie);
            }
        }

        private void OnSerieClick(object sender, ItemClickEventArgs e) {
            var serie = e.ClickedItem as Serie;

            Frame.Navigate(typeof(SerieDetailedInfoPage), serie);
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

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace homarr {
    public sealed partial class MainWindow : Window {
        public MainWindow() {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            if (this.AppWindow.Presenter is OverlappedPresenter presenter) {
                presenter.Maximize();
            }

            var toto = new NavigationViewItem {
                Content = new StackPanel {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children = {
                        new FontIcon {
                            FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe UI Emoji"),
                            Glyph = "&#x1F3AC;",
                            FontSize = 32,
                        },
                        new TextBlock {
                            Text = "Settings",
                            TextAlignment = TextAlignment.Center,
                        },
                    },
                },
            };

            navigationView.SelectedItem = navigationView.MenuItems.First();
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
            var selected = (NavigationViewItem)args.SelectedItem;

            if (selected.Tag.ToString() != "Settings" && Settings.IsMissingSettings()) {
                navigationView.SelectedItem = navigationView.FooterMenuItems.First();
                return;
            }

            Type pageType = Type.GetType(GetPageName(selected.Tag.ToString()));

            navigationViewFrame.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
        }

        private string GetPageName(string tag) {
            return tag switch {
                "Series" => "homarr.Serie.Series",
                "Movies" => "homarr.Movie.Movies",
                "Settings" => "homarr.Settings",
                _ => throw new Exception("Page not found"),
            };
        }
    }
}

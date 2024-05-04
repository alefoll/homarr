using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using WinRT;

namespace homarr {
    public sealed partial class MainWindow : Window {
        public MainWindow() {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            this.SetAcrylicBackdrop();

            if (this.AppWindow.Presenter is OverlappedPresenter presenter) {
                presenter.Maximize();
            }

            new NavigationViewItem {
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

            this.NavigationView.SelectedItem = this.NavigationView.MenuItems.First();

            Type pageType = Type.GetType(GetPageName("Series"));

            this.NavigationViewFrame.Navigate(pageType);
        }

        private void SetAcrylicBackdrop() {
            if (!DesktopAcrylicController.IsSupported()) {
                return;
            }

            var dispatcherQueueHelper = new WindowsSystemDispatcherQueueHelper();
            dispatcherQueueHelper.EnsureWindowsSystemDispatcherQueueController();

            var backdropConfiguration = new SystemBackdropConfiguration {
                IsInputActive = true,
            };

            backdropConfiguration.Theme = ((FrameworkElement)this.Content).ActualTheme switch {
                ElementTheme.Dark => SystemBackdropTheme.Dark,
                ElementTheme.Light => SystemBackdropTheme.Light,
                ElementTheme.Default => SystemBackdropTheme.Default,
                _ => backdropConfiguration.Theme
            };

            this.Activated += (sender, args) => {
                backdropConfiguration.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
            };

            var backdropController = new DesktopAcrylicController {
                Kind = DesktopAcrylicKind.Thin,
            };

            this.Closed += (sender, args) => {
                backdropController.Dispose();
            };

            backdropController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
            backdropController.SetSystemBackdropConfiguration(backdropConfiguration);
        }

        private void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
            var selectedItem = (NavigationViewItem)args.SelectedItem;

            if (selectedItem == null) {
                return;
            }

            if (selectedItem.Tag.ToString() == "Settings") {
                return;
            }

            if (!Settings.IsMissingSettings()) {
                return;
            }

            this.NavigationView.SelectedItem = this.GetNavigationViewItem("Settings");

            Type pageType = Type.GetType(GetPageName("Settings"));

            this.NavigationViewFrame.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
        }

        private void OnNavigationViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            var clickedItem = (NavigationViewItem)args.InvokedItemContainer;

            if (clickedItem == null) {
                return;
            }

            if (this.NavigationViewFrame.SourcePageType.Name == clickedItem.Tag.ToString()) {
                return;
            }

            Type pageType = Type.GetType(GetPageName(clickedItem.Tag.ToString()));

            this.NavigationViewFrame.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
        }

        private void OnNavigationViewFrameNavigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e) {
            this.NavigationView.IsBackEnabled = this.NavigationViewFrame.CanGoBack;
        }

        private void OnNavigationViewBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) {
            this.NavigationViewFrame.GoBack();

            this.NavigationView.SelectedItem = GetNavigationViewItem(this.NavigationViewFrame.SourcePageType.Name);

            this.NavigationView.IsBackEnabled = this.NavigationViewFrame.CanGoBack;
        }

        private string GetPageName(string tag) {
            return tag switch {
                "Series" => "homarr.Serie.Series",
                "SerieDetailedInfoPage" => "homarr.Serie.SerieDetailedInfoPage",
                "Movies" => "homarr.Movie.Movies",
                "Settings" => "homarr.Settings",
                _ => throw new Exception("Page not found"),
            };
        }

        private object GetNavigationViewItem(string tag) {
            return tag switch {
                "Series" => this.NavigationView.MenuItems[0],
                "SerieDetailedInfoPage" => this.NavigationView.MenuItems[0],
                "Movies" => this.NavigationView.MenuItems[1],
                "Settings" => this.NavigationView.FooterMenuItems.First(),
                _ => throw new Exception("Navigation Item not found"),
            };
        }
    }
}

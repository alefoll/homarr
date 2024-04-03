using Microsoft.UI;
using Microsoft.UI.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI;

namespace homarr {
    public sealed partial class Settings : Page {
        public record class Setting(
            string key = null,
            string description = null
        );

        public static ReadOnlyCollection<Setting> mandatorySettings = new(
            new Setting[] {
                new Setting {
                    key = "SonarrUrl",
                    description = "Sonarr URL"
                },

                new Setting {
                    key = "SonarrApiKey",
                    description = "Sonarr API Key"
                },

                new Setting {
                    key = "RadarrUrl",
                    description = "Radarr URL"
                },

                new Setting {
                    key = "RadarrApiKey",
                    description = "Radarr API Key"
                }
            }
        );

        public Settings() {
            this.InitializeComponent();

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            for (var i = 0; i < mandatorySettings.Count; i++) {
                var setting = mandatorySettings[i];

                var textBlock = new TextBlock {
                    Text = setting.description,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10),
                };

                var textBox = new TextBox {
                    Margin = new Thickness(10),
                    Width = 350,
                };

                if (localSettings.Values[setting.key] != null) {
                    textBox.Text = localSettings.Values[setting.key] as string;
                }

                textBox.SelectionChanged += (object sender, RoutedEventArgs e) => {
                    localSettings.Values[setting.key] = textBox.Text;
                };

                var border = new Border {
                    Background = (Brush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"],
                    Padding = new Thickness(10),
                    CornerRadius = new CornerRadius(5),
                };

                Grid.SetRow(border, i);
                Grid.SetColumnSpan(border, 2);
                this.SettingsGrid.Children.Add(border);

                Grid.SetRow(textBlock, i);
                Grid.SetColumn(textBlock, 0);
                this.SettingsGrid.Children.Add(textBlock);

                Grid.SetRow(textBox, i);
                Grid.SetColumn(textBox, 1);
                this.SettingsGrid.Children.Add(textBox);
            }
        }

        public static bool IsMissingSettings() {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            foreach (var setting in mandatorySettings) {
                if (localSettings.Values[setting.key] == null) {
                    return true;
                }
            }

            return false;
        }

        public static string GetSetting(string key) {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            var value = localSettings.Values[key];

            if (value != null) {
                return value.ToString();
            }

            throw new Exception($"Setting '{key}' not found");
        }
    }
}

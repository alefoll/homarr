using homarr.Movie;
using homarr.Serie;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace homarr.History {
    public sealed partial class History : Page {
        public readonly ObservableCollection<HistoryGroup> HistoryRecordsGrouped = new();

        public readonly ObservableCollection<HistoryRecord> Testtt = new();

        private readonly Sonarr Sonarr;
        private readonly Radarr Radarr;

        public History() {
            this.InitializeComponent();

            this.Sonarr = new Sonarr(Settings.GetSetting("SonarrUrl"), Settings.GetSetting("SonarrApiKey"));
            this.Radarr = new Radarr(Settings.GetSetting("RadarrUrl"), Settings.GetSetting("RadarrApiKey"));

            _ = GetHistory();
        }

        private async Task GetHistory() {
            var last2Weeks = DateTime.Now.AddDays(-14);

            var sonarrHistory = await Sonarr.GetHistory(last2Weeks);
            var radarrHistory = await Radarr.GetHistory(last2Weeks);

            var allHistoryGroups = sonarrHistory;

            foreach (var group in radarrHistory) {
                var existingGroup = allHistoryGroups.Find(element => element.Date.ToShortDateString() == group.Date.ToShortDateString());

                if (existingGroup == null) {
                    group.Records.Sort((a, b) => string.Compare(a.Title, b.Title));

                    allHistoryGroups.Add(group);
                } else {
                    existingGroup.Records.AddRange(group.Records);

                    existingGroup.Records.Sort((a, b) => string.Compare(a.Title, b.Title));
                }
            }

            foreach (var group in allHistoryGroups.OrderByDescending(element => element.Date)) {
                HistoryRecordsGrouped.Add(group);
            }
        }
    }
}

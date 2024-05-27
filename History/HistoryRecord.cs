using System;
using System.Collections.Generic;

namespace homarr.History {
    public class HistoryRecord {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string ImagePoster { get; set; }
        public string IMdBLink { get; set; }
        // NOTE: parent = serie or movie, child = serie episode (and movie shouldn't have children)
        public List<HistoryRecordChildren> Children { get; set; }
        public bool IsSerie => Children.Count > 0;
        public bool IsMovie => !IsSerie;
    }

    public class HistoryRecordChildren {
        public int Id { get; set; }
        public string Title { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string SeasonEpisodeStringify => $"{SeasonNumber}x{EpisodeNumber.ToString("D2")}";
        public DateTime Date { get; set; }

        public static int Compare(HistoryRecordChildren a, HistoryRecordChildren b) {
            if (a.SeasonNumber > b.SeasonNumber) {
                return 1;
            }

            if (a.SeasonNumber < b.SeasonNumber) {
                return -1;
            }

            if (a.EpisodeNumber > b.EpisodeNumber) {
                return 1;
            }

            if (a.EpisodeNumber < b.EpisodeNumber) {
                return -1;
            }

            return string.Compare(a.Title, b.Title);
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace homarr.Serie {
    public class Serie : INotifyPropertyChanged {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImagePoster { get; set; }
        public string ImageFanart { get; set; }
        public string Path { get; set; }
        public string IMdBLink { get; set; }
        public Sonarr Sonarr { get; set; }
        public IEnumerable<Episode> Episodes { get; set; }
        public string EpisodesCount {
            get {
                var s = this.Episodes.Count() > 1 ? "s" : "";

                return $"{this.Episodes.Count()} episode{s}";
            }
        }
        public IEnumerable<Season> Seasons {
            get {
                return this.Episodes
                    .GroupBy(episode => episode.SeasonNumber)
                    .Select(episodes => {
                        return new Season {
                            SeasonNumber = episodes.First().SeasonNumber,
                            Episodes = episodes.ToList<Episode>(),
                        };
                    });
            }
            // HACK: use empty set to make the IEnumerable non read-only
            // to trigger the PropertyChanged in SerieDetailedInfoPage
            set { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task RefreshEpisodes() {
            this.Episodes = await this.Sonarr.GetEpisodes(Id);
        }

        public async Task DeleteEpisode(Episode episode) {
            await episode.Delete();

            if (this.Episodes.Count() <= 1) {
                await this.DeleteSerieFolder();
            }

            await this.Sonarr.DeleteEpisode(episode.Id);

            await this.RefreshEpisodes();

            this.PropertyChanged?.Invoke(this, new(nameof(Seasons)));
        }

        private async Task DeleteSerieFolder() {
            var folder = await StorageFolder.GetFolderFromPathAsync(this.Path);

            await folder.DeleteAsync(StorageDeleteOption.Default);
        }
    }
}
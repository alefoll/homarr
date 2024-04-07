using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace homarr.Serie {
    public class Serie {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImagePoster { get; set; }
        public string ImageFanart { get; set; }
        public string Path { get; set; }
        public string IMdBLink { get; set; }
        public Sonarr Sonarr { get; set; }

        public async Task<IEnumerable<Episode>> GetEpisodesOnDisk() {
            return await this.Sonarr.GetEpisodes(this.Id);
        }

        public async Task<bool> DeleteEpisode(Episode episode) {
            var episodes = await this.GetEpisodesOnDisk();

            await episode.Delete();

            if (episodes.Count() <= 1) {
                await this.DeleteSerieFolder();
            }

            return await this.Sonarr.DeleteEpisode(episode.Id);
        }

        private async Task DeleteSerieFolder() {
            var folder = await StorageFolder.GetFolderFromPathAsync(this.Path);

            await folder.DeleteAsync(StorageDeleteOption.Default);
        }
    }
}
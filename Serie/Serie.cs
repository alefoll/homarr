using System.Collections.Generic;
using System.Threading.Tasks;

namespace homarr.Serie {
    public class Serie {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImagePoster { get; set; }
        public string ImageFanart { get; set; }
        public string IMdBLink { get; set; }
        public Sonarr Sonarr { get; set; }

        public async Task<IEnumerable<Episode>> GetEpisodesOnDisk() {
            return await this.Sonarr.GetEpisodes(this.Id);
        }

        public async Task<bool> DeleteEpisode(Episode episode) {
            await episode.Delete();

            return await this.Sonarr.DeleteEpisode(episode.Id);
        }
    }
}
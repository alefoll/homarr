using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;

namespace homarr.Movie {
    public class Movie {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImagePoster { get; set; }
        public string Path { get; set; }
        public string FilePath { get; set; }
        public string Duration { get; set; }
        public IEnumerable<string> Genres { get; set; }
        public string GenresStringify {
            get { return String.Join(", ", this.Genres); }
        }
        public double? ImdbRating { get; set; }
        public Radarr Radarr { get; set; }

        public void Play() {
            Process.Start(new ProcessStartInfo("mpv") {
                ArgumentList = {
                    FilePath,
                    "--fullscreen",
                    "--sid=1", // Select the first subtitle
                },
                Verb = "runas"
            });
        }

        public async Task Delete() {
            var folder = await StorageFolder.GetFolderFromPathAsync(this.Path);

            await folder.DeleteAsync(StorageDeleteOption.Default);

            await this.Radarr.DeleteMovie(this.Id);
        }
    }
}
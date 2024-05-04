using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;

namespace homarr.Serie {
    public class Episode : INotifyPropertyChanged {
        public int Id { get; set; }
        public string Title { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Path { get; set; }
        public string Quality { get; set; }
        public string Duration { get; set; }

        private BitmapImage _thumbnail { get; set; }
        public bool IsThumbnailNotAvailable { get; set; } = true;
        private bool _thumbnailRetreiveCalled { get; set; } = false;
        public BitmapImage Thumbnail {
            get {
                if (_thumbnailRetreiveCalled == false) {
                    _ = RetreiveThumbnail();
                }

                return _thumbnail;
            }

            set {
                _thumbnail = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async Task RetreiveThumbnail() {
            _thumbnailRetreiveCalled = true;

            BitmapImage image = new BitmapImage();

            var file = await StorageFile.GetFileFromPathAsync(Path);

            var windowsThumbnail = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.VideosView, 120, Windows.Storage.FileProperties.ThumbnailOptions.None);

            image.SetSource(windowsThumbnail);

            IsThumbnailNotAvailable = false;
            Thumbnail = image;
        }

        public void Play() {
            Process.Start(new ProcessStartInfo("mpv") {
                ArgumentList = {
                    Path,
                    "--fullscreen",
                    "--sid=1", // Select the first subtitle
                },
                Verb = "runas"
            });
        }

        public async Task Delete() {
            var file = await StorageFile.GetFileFromPathAsync(Path);

            await file.DeleteAsync(StorageDeleteOption.Default);
        }
    };
}

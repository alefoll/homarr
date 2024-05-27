using homarr.History;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace homarr.Movie {
    public record class MovieAPI(
        int id = 0,
        string title = null,
        MovieAPIOriginalLanguage originalLanguage = null,
        IEnumerable<MovieAPIImage> images = null,
        string path = null,
        MovieAPIFile movieFile = null,
        IEnumerable<string> genres = null,
        string imdbId = null,
        MovieAPIRatings ratings = null,
        MovieAPIStatistics statistics = null
    );

    public record class MovieAPIOriginalLanguage(
        int id = 0,
        string name = null
    );

    public record class MovieAPIImage(
        string coverType = null,
        string url = null,
        string remoteUrl = null
    );

    public record class MovieAPIFile(
        string path = null,
        MovieAPIMediaInfo mediaInfo = null
    );

    public record class MovieAPIMediaInfo(
        string runTime = null
    );

    public record class MovieAPIRatings(
        MovieAPIRatingImdb imdb = null
    );

    public record class MovieAPIRatingImdb(
        double value = 0.0
    );

    public record class MovieAPIStatistics(
        ulong sizeOnDisk = 0
    );

    public record class HistoryAPI(
        IEnumerable<HistoryRecordAPI> records = null
    );

    public record class HistoryRecordAPI(
        DateTime date = default,
        MovieAPI movie = null,
        string eventType = null,
        HistoryRecordDataAPI data = null
    );

    public record class HistoryRecordDataAPI(
        string importedPath = null
    );

    public sealed partial class Radarr {
        private readonly string Url;
        private readonly HttpClient HttpClient;

        public Radarr(string url, string apiKey) {
            this.HttpClient = new HttpClient();

            this.HttpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

            this.Url = url;
        }

        public async Task<IEnumerable<Movie>> GetMovies() {
            var requestUri = new Uri(this.Url + "/api/v3/movie");

            var response = await this.HttpClient.GetFromJsonAsync<List<MovieAPI>>(requestUri);

            return response
                .Where(movie => movie.statistics.sizeOnDisk > 0 && movie.originalLanguage.name != "French")
                .OrderBy(movie => movie.title)
                .Select(movie => {
                    return new Movie {
                        Id = movie.id,
                        Title = movie.title,
                        ImagePoster = this.Url + movie.images.Where(image => image.coverType.Equals("poster")).FirstOrDefault().url,
                        Path = movie.path,
                        FilePath = movie.movieFile.path,
                        Duration = movie.movieFile.mediaInfo.runTime,
                        Genres = movie.genres,
                        ImdbRating = movie.ratings?.imdb?.value,
                        Radarr = this,
                    };
                });
        }

        public async Task<bool> DeleteMovie(int movieId) {
            var requestEpisodesUri = new Uri(this.Url + "/api/v3/movie/" + movieId);

            var responseEpisodes = await this.HttpClient.DeleteAsync(requestEpisodesUri);

            responseEpisodes.EnsureSuccessStatusCode();

            return true;
        }

        public async Task<List<HistoryGroup>> GetHistory(DateTime since) {
            // For eventType params, we only want the recently downloaded episodes aka "DownloadFolderImported"
            // SEE: https://github.com/Radarr/Radarr/blob/638f92495cea757ad07bed6df9819f0381c9abfc/src/NzbDrone.Core/History/History.cs#L33L47
            var requestUri = new Uri($"{this.Url}/api/v3/history/since?date={since.ToString("yyyy-MM-ddTHH:mm:ssZ")}&includeMovie=true&eventType=3");

            var response = await this.HttpClient.GetFromJsonAsync<List<HistoryRecordAPI>>(requestUri);

            var historyGroups = new List<HistoryGroup>();

            var cultureInfo = new CultureInfo("en-US");

            var processedHistory = new List<int>();

            foreach (var history in response.OrderByDescending(item => item.date)) {
                if (processedHistory.Contains(history.movie.id)) {
                    continue;
                }

                processedHistory.Add(history.movie.id);

                var historyGroup = historyGroups.Find(element => element.Date.ToShortDateString() == history.date.ToShortDateString());

                if (historyGroup == null) {
                    historyGroup = new HistoryGroup {
                        Date = history.date,
                        DateStringify = history.date.ToString("dddd d MMMM", cultureInfo),
                        Records = new List<HistoryRecord>(),
                    };

                    historyGroups.Add(historyGroup);
                }

                var historyRecord = historyGroup.Records.Find(element => element.Id == history.movie.id);

                if (historyRecord == null) {
                    // HACK: API doesn't return local url image, only remote
                    var imagePoster = $"{this.Url}/MediaCover/{history.movie.id}/poster.jpg";

                    var newRecord = new HistoryRecord {
                        Id = history.movie.id,
                        Date = history.date,
                        Title = history.movie.title,
                        ImagePoster = imagePoster,
                        IMdBLink = $"https://www.imdb.com/title/{history.movie.imdbId}/",
                        Children = new List<HistoryRecordChildren>(),
                    };

                    historyGroup.Records.Add(newRecord);
                } else {
                    historyRecord.Date = history.date;
                }
            }

            return historyGroups;
        }
    }
}

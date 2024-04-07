using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace homarr.Movie {
    public record class MovieAPI(
        int id = 0,
        string title = null,
        List<MovieAPIImage> images = null,
        string path = null,
        MovieAPIFile movieFile = null,
        MovieAPIRatings ratings = null,
        MovieAPIStatistics statistics = null
    );

    public record class MovieAPIImage(
        string coverType = null,
        string url = null
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
                .Where(movie => movie.statistics.sizeOnDisk > 0)
                .OrderBy(movie => movie.title)
                .Select(movie => {
                    return new Movie {
                        Id = movie.id,
                        Title = movie.title,
                        ImagePoster = this.Url + movie.images.Where(image => image.coverType.Equals("poster")).FirstOrDefault().url,
                        Path = movie.path,
                        FilePath = movie.movieFile.path,
                        Duration = movie.movieFile.mediaInfo.runTime,
                        ImdbRating = movie.ratings.imdb.value,
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
    }
}

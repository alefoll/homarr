using homarr.History;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace homarr.Serie {
    public record class SerieAPI(
        int id = 0,
        string title = null,
        SerieAPIOriginalLanguage originalLanguage = null,
        IEnumerable<SerieAPIImage> images = null,
        string path = null,
        string imdbId = null,
        SerieAPIStatistics statistics = null
    );

    public record class SerieAPIOriginalLanguage(
        int id = 0,
        string name = null
    );

    public record class SerieAPIImage(
        string coverType = null,
        string url = null,
        string remoteUrl = null
    );

    public record class SerieAPIStatistics(
        int episodeFileCount = 0,
        ulong sizeOnDisk = 0
    );

    public record class EpisodeAPI(
        int id = 0,
        int episodeFileId = 0,
        string title = null,
        int seasonNumber = 0,
        int episodeNumber = 0,
        bool hasFile = false
    );

    public record class EpisodeFileAPI(
        int id = 0,
        string path = null,
        string relativePath = null,
        EpisodeFileAPIQuality quality = null,
        EpisodeFileAPIMediaInfo mediaInfo = null
    );

    public record class EpisodeFileAPIQuality(
        EpisodeFileAPIQualityQuality quality = null
    );

    public record class EpisodeFileAPIQualityQuality(
        string name = null
    );

    public record class EpisodeFileAPIMediaInfo(
        string runTime = null
    );

    public record class HistoryRecordAPI(
        DateTime date = default,
        EpisodeAPI episode = null,
        SerieAPI series = null,
        string eventType = null,
        HistoryRecordDataAPI data = null
    );

    public record class HistoryRecordDataAPI(
        string importedPath = null
    );

    public sealed partial class Sonarr {
        private readonly string Url;
        private readonly HttpClient HttpClient;

        public Sonarr(string url, string apiKey) {
            this.HttpClient = new HttpClient();

            this.HttpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

            this.Url = url;
        }

        public async Task<IEnumerable<Serie>> GetSeries() {
            var requestUri = new Uri(this.Url + "/api/v3/series?includeSeasonImages=false");

            var response = await this.HttpClient.GetFromJsonAsync<List<SerieAPI>>(requestUri);

            return await Task.WhenAll(
                response
                    .Where(serie => serie.statistics.sizeOnDisk > 0 && serie.originalLanguage.name != "French")
                    .OrderBy(serie => serie.title)
                    .Select(async serie => {
                        var episodes = await this.GetEpisodes(serie.id);

                        return new Serie {
                            Id = serie.id,
                            Title = serie.title,
                            ImagePoster = this.Url + serie.images.Where(image => image.coverType.Equals("poster")).FirstOrDefault().url,
                            ImageFanart = this.Url + serie.images.Where(image => image.coverType.Equals("fanart")).FirstOrDefault().url,
                            Path = serie.path,
                            IMdBLink = $"https://www.imdb.com/title/{serie.imdbId}/",
                            Episodes = episodes,
                            Sonarr = this,
                        };
                    })
                );
        }

        public async Task<IEnumerable<Episode>> GetEpisodes(int serieId) {
            var requestEpisodesUri = new Uri(this.Url + "/api/v3/episode?seriesId=" + serieId);

            var responseEpisodes = await this.HttpClient.GetFromJsonAsync<List<EpisodeAPI>>(requestEpisodesUri);

            var requestEpisodeFilesUri = new Uri(this.Url + "/api/v3/episodefile?seriesId=" + serieId);

            var responseEpisodeFiles = await this.HttpClient.GetFromJsonAsync<List<EpisodeFileAPI>>(requestEpisodeFilesUri);

            return responseEpisodes
                .Where(episode => episode.hasFile)
                .Select(episode => {
                    var episodeFile = responseEpisodeFiles.Find(episodeFile => episodeFile.id == episode.episodeFileId);

                    return new Episode {
                        Id = episodeFile.id,
                        Title = episode.title,
                        SeasonNumber = episode.seasonNumber,
                        EpisodeNumber = episode.episodeNumber,
                        Path = episodeFile.path,
                        Quality = episodeFile.quality.quality.name,
                        Duration = episodeFile.mediaInfo.runTime,
                    };
                });
        }

        public async Task<bool> DeleteEpisode(int episodeId) {
            var requestUri = new Uri(this.Url + "/api/v3/episodefile/" + episodeId);

            var response = await this.HttpClient.DeleteAsync(requestUri);

            response.EnsureSuccessStatusCode();

            return true;
        }

        public async Task<List<HistoryGroup>> GetHistory(DateTime since) {
            // NOTE: For eventType params, we only want the recently downloaded episodes aka "DownloadFolderImported"
            // SEE: https://github.com/Sonarr/Sonarr/blob/627b2a4289ecdd5558d37940624289708e01e10a/src/NzbDrone.Core/History/EpisodeHistory.cs#L34L44
            var requestUri = new Uri($"{this.Url}/api/v3/history/since?date={since.ToString("yyyy-MM-ddTHH:mm:ssZ")}&includeSeries=true&includeEpisode=true&eventType=3");

            var response = await this.HttpClient.GetFromJsonAsync<List<HistoryRecordAPI>>(requestUri);

            var historyGroups = new List<HistoryGroup>();

            var cultureInfo = new CultureInfo("en-US");

            var processedHistory = new List<int>();

            foreach (var history in response.OrderByDescending(item => item.date)) {
                var uniqueId = int.Parse(history.series.id.ToString() + history.episode.id.ToString());

                if (processedHistory.Contains(uniqueId)) {
                    continue;
                }

                processedHistory.Add(uniqueId);

                var historyGroup = historyGroups.Find(element => element.Date.ToShortDateString() == history.date.ToShortDateString());

                if (historyGroup == null) {
                    historyGroup = new HistoryGroup {
                        Date = history.date,
                        DateStringify = history.date.ToString("dddd d MMMM", cultureInfo),
                        Records = new List<HistoryRecord>(),
                    };

                    historyGroups.Add(historyGroup);
                }

                var historyRecordSerie = historyGroup.Records.Find(element => element.Id == history.series.id);

                if (historyRecordSerie == null) {
                    // HACK: API doesn't return local url image, only remote
                    var imagePoster = $"{this.Url}/MediaCover/{history.series.id}/poster.jpg";

                    historyRecordSerie = new HistoryRecord {
                        Id = history.series.id,
                        Title = history.series.title,
                        Date = history.date,
                        ImagePoster = imagePoster,
                        IMdBLink = $"https://www.imdb.com/title/{history.series.imdbId}/",
                        Children = new List<HistoryRecordChildren>(),
                    };

                    historyGroup.Records.Add(historyRecordSerie);
                }

                var historyRecordEpisode = historyRecordSerie.Children.Find(element => element.Id == history.episode.id);

                if (historyRecordEpisode == null) {
                    historyRecordEpisode = new HistoryRecordChildren {
                        Id = history.episode.id,
                        Title = history.episode.title,
                        SeasonNumber = history.episode.seasonNumber,
                        EpisodeNumber = history.episode.episodeNumber,
                        Date = history.date,
                    };

                    historyRecordSerie.Children.Add(historyRecordEpisode);
                } else if (historyRecordEpisode.Date.CompareTo(history.date) > 0) {
                    historyRecordEpisode.Date = history.date;
                }
            }

            foreach (var group in historyGroups) {
                foreach (var record in group.Records) {
                    record.Children.Sort(HistoryRecordChildren.Compare);
                }
            }

            return historyGroups;
        }
    }
}

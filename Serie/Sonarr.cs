﻿using System;
using System.Collections.Generic;
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
        string url = null
    );

    public record class SerieAPIStatistics(
        int episodeFileCount = 0,
        ulong sizeOnDisk = 0
    );

    public record class EpisodeAPI(
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
            var requestEpisodesUri = new Uri(this.Url + "/api/v3/episodefile/" + episodeId);

            var responseEpisodes = await this.HttpClient.DeleteAsync(requestEpisodesUri);

            responseEpisodes.EnsureSuccessStatusCode();

            return true;
        }
    }
}

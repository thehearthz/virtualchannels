using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.VirtualChannels.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.VirtualChannels.Services
{
    /// <summary>
    /// Automatically generates virtual channels based on library content.
    /// </summary>
    public class AutoChannelGenerator
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<AutoChannelGenerator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoChannelGenerator"/> class.
        /// </summary>
        /// <param name="libraryManager">The library manager.</param>
        /// <param name="logger">The logger.</param>
        public AutoChannelGenerator(
            ILibraryManager libraryManager,
            ILogger<AutoChannelGenerator> logger)
        {
            _libraryManager = libraryManager;
            _logger = logger;
        }

        /// <summary>
        /// Generates channels automatically based on configuration.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of auto-generated channels.</returns>
        public async Task<List<VirtualChannelConfig>> GenerateAutoChannels(CancellationToken cancellationToken)
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null || !config.EnableAutoChannelGeneration)
            {
                return new List<VirtualChannelConfig>();
            }

            var channels = new List<VirtualChannelConfig>();
            var currentChannelNumber = config.BaseChannelNumber;

            // Generate genre-based channels
            if (config.AutoGenerateGenreChannels)
            {
                var genreChannels = await GenerateGenreChannels(currentChannelNumber, cancellationToken);
                channels.AddRange(genreChannels);
                currentChannelNumber += genreChannels.Count;
            }

            // Generate year-based channels
            if (config.AutoGenerateYearChannels)
            {
                var yearChannels = await GenerateYearChannels(currentChannelNumber, cancellationToken);
                channels.AddRange(yearChannels);
                currentChannelNumber += yearChannels.Count;
            }

            _logger.LogInformation("Auto-generated {Count} channels", channels.Count);
            return channels;
        }

        /// <summary>
        /// Generates channels based on genres.
        /// </summary>
        private async Task<List<VirtualChannelConfig>> GenerateGenreChannels(
            int startingChannelNumber,
            CancellationToken cancellationToken)
        {
            var channels = new List<VirtualChannelConfig>();

            // Get all genres from the library
            var query = new InternalItemsQuery
            {
                Recursive = true,
                IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Episode }
            };

            var items = _libraryManager.GetItemList(query);
            var genres = items
                .Where(i => i.Genres != null && i.Genres.Length > 0)
                .SelectMany(i => i.Genres)
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            _logger.LogInformation("Found {Count} genres for auto-channel generation", genres.Count);

            var channelNumber = startingChannelNumber;
            foreach (var genre in genres)
            {
                // Only create channels for genres with sufficient content
                var genreItemCount = items.Count(i => i.Genres != null && i.Genres.Contains(genre));
                if (genreItemCount < 5)
                {
                    continue;
                }

                channels.Add(new VirtualChannelConfig
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"{genre} Channel",
                    ChannelNumber = channelNumber++,
                    Type = "Genre",
                    ContentFilters = new List<string> { genre },
                    ShuffleMode = true,
                    RespectEpisodeOrder = false,
                    CommercialIntervalSeconds = 900,
                    EnablePreRolls = true,
                    Enabled = true
                });
            }

            return channels;
        }

        /// <summary>
        /// Generates channels based on release years.
        /// </summary>
        private async Task<List<VirtualChannelConfig>> GenerateYearChannels(
            int startingChannelNumber,
            CancellationToken cancellationToken)
        {
            var channels = new List<VirtualChannelConfig>();

            // Get all movies with production years
            var query = new InternalItemsQuery
            {
                Recursive = true,
                IncludeItemTypes = new[] { BaseItemKind.Movie }
            };

            var items = _libraryManager.GetItemList(query);
            var years = items
                .Where(i => i.ProductionYear.HasValue)
                .Select(i => i.ProductionYear!.Value)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            _logger.LogInformation("Found {Count} years for auto-channel generation", years.Count);

            var channelNumber = startingChannelNumber;

            // Create decade channels instead of individual years
            var decades = years
                .Select(y => (y / 10) * 10)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            foreach (var decade in decades)
            {
                var decadeItems = items.Count(i =>
                    i.ProductionYear.HasValue &&
                    i.ProductionYear.Value >= decade &&
                    i.ProductionYear.Value < decade + 10);

                if (decadeItems < 10)
                {
                    continue;
                }

                channels.Add(new VirtualChannelConfig
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"{decade}s Movies",
                    ChannelNumber = channelNumber++,
                    Type = "Year",
                    ContentFilters = new List<string> { decade.ToString() },
                    ShuffleMode = true,
                    RespectEpisodeOrder = false,
                    CommercialIntervalSeconds = 900,
                    EnablePreRolls = true,
                    Enabled = true
                });
            }

            return channels;
        }

        /// <summary>
        /// Updates auto-generated channels in the configuration.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task UpdateAutoChannels(CancellationToken cancellationToken)
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                return;
            }

            // Remove old auto-generated channels
            config.Channels.RemoveAll(c =>
                c.Type == "Genre" || c.Type == "Year");

            // Generate new auto channels
            var autoChannels = await GenerateAutoChannels(cancellationToken);
            config.Channels.AddRange(autoChannels);

            Plugin.Instance?.SaveConfiguration();
            _logger.LogInformation("Updated auto-generated channels");
        }
    }
}

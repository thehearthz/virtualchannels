using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.VirtualChannels.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.VirtualChannels.Services
{
    /// <summary>
    /// Manages scheduling and content queuing for virtual channels.
    /// </summary>
    public class ChannelScheduler
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<ChannelScheduler> _logger;
        private readonly Dictionary<string, Queue<BaseItem>> _channelQueues;
        private readonly Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelScheduler"/> class.
        /// </summary>
        /// <param name="libraryManager">The library manager.</param>
        /// <param name="logger">The logger.</param>
        public ChannelScheduler(
            ILibraryManager libraryManager,
            ILogger<ChannelScheduler> logger)
        {
            _libraryManager = libraryManager;
            _logger = logger;
            _channelQueues = new Dictionary<string, Queue<BaseItem>>();
            _random = new Random();
        }

        /// <summary>
        /// Gets the next item for a channel.
        /// </summary>
        /// <param name="channelId">The channel ID.</param>
        /// <param name="config">The channel configuration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The next item to play.</returns>
        public async Task<BaseItem?> GetNextItemForChannel(
            string channelId,
            VirtualChannelConfig config,
            CancellationToken cancellationToken)
        {
            if (!_channelQueues.ContainsKey(channelId) || _channelQueues[channelId].Count < 3)
            {
                await RefillQueue(channelId, config, cancellationToken);
            }

            return _channelQueues.ContainsKey(channelId) && _channelQueues[channelId].Count > 0
                ? _channelQueues[channelId].Dequeue()
                : null;
        }

        /// <summary>
        /// Refills the content queue for a channel.
        /// </summary>
        private async Task RefillQueue(
            string channelId,
            VirtualChannelConfig config,
            CancellationToken cancellationToken)
        {
            var items = await GetChannelContent(config, cancellationToken);

            if (items.Count == 0)
            {
                _logger.LogWarning("No content found for channel {ChannelId}", channelId);
                return;
            }

            if (config.ShuffleMode)
            {
                items = items.OrderBy(_ => _random.Next()).ToList();
            }

            var queue = new Queue<BaseItem>(items);
            _channelQueues[channelId] = queue;

            _logger.LogInformation(
                "Refilled queue for channel {ChannelId} with {Count} items",
                channelId,
                items.Count);
        }

        /// <summary>
        /// Gets content for a channel based on its configuration.
        /// </summary>
        private async Task<List<BaseItem>> GetChannelContent(
            VirtualChannelConfig config,
            CancellationToken cancellationToken)
        {
            var query = new InternalItemsQuery
            {
                Recursive = true,
                IsVirtualItem = false,
                HasPath = true
            };

            // Apply filters based on channel type
            switch (config.Type)
            {
                case "Genre":
                    query.Genres = config.ContentFilters.ToArray();
                    query.IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Episode };
                    break;

                case "Year":
                    if (config.ContentFilters.Any() && int.TryParse(config.ContentFilters.First(), out var year))
                    {
                        query.Years = new[] { year };
                        query.IncludeItemTypes = new[] { BaseItemKind.Movie };
                    }
                    break;

                case "Series":
                    // Get specific series by ID
                    if (config.ContentFilters.Any() && Guid.TryParse(config.ContentFilters.First(), out var seriesId))
                    {
                        var series = _libraryManager.GetItemById(seriesId);
                        if (series != null)
                        {
                            query.AncestorIds = new[] { seriesId };
                            query.IncludeItemTypes = new[] { BaseItemKind.Episode };

                            if (config.RespectEpisodeOrder)
                            {
                                query.OrderBy = new[]
                                {
                                    (ItemSortBy.ParentIndexNumber, SortOrder.Ascending),
                                    (ItemSortBy.IndexNumber, SortOrder.Ascending)
                                };
                            }
                        }
                    }
                    break;

                case "Custom":
                    // Custom query based on tags or collection
                    if (config.ContentFilters.Any())
                    {
                        query.Tags = config.ContentFilters.ToArray();
                    }
                    query.IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Episode };
                    break;

                default:
                    query.IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Episode };
                    break;
            }

            var items = _libraryManager.GetItemList(query);
            return items.Where(i => !string.IsNullOrEmpty(i.Path) && i.RunTimeTicks.HasValue && i.RunTimeTicks > 0)
                        .ToList();
        }

        /// <summary>
        /// Gets commercial content from the designated folder.
        /// </summary>
        /// <param name="commercialPath">Path to commercials.</param>
        /// <param name="count">Number of commercials to get.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of commercial items.</returns>
        public async Task<List<BaseItem>> GetCommercials(
            string commercialPath,
            int count,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(commercialPath))
            {
                return new List<BaseItem>();
            }

            var query = new InternalItemsQuery
            {
                Path = commercialPath,
                Recursive = true,
                IncludeItemTypes = new[] { BaseItemKind.Movie },
                MediaTypes = new[] { MediaType.Video }
            };

            var commercials = _libraryManager.GetItemList(query);
            return commercials
                .Where(c => c.RunTimeTicks.HasValue && c.RunTimeTicks > 0)
                .OrderBy(_ => _random.Next())
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Clears the queue for a channel.
        /// </summary>
        /// <param name="channelId">The channel ID.</param>
        public void ClearQueue(string channelId)
        {
            if (_channelQueues.ContainsKey(channelId))
            {
                _channelQueues[channelId].Clear();
                _logger.LogInformation("Cleared queue for channel {ChannelId}", channelId);
            }
        }

        /// <summary>
        /// Gets the current queue size for a channel.
        /// </summary>
        /// <param name="channelId">The channel ID.</param>
        /// <returns>Queue size.</returns>
        public int GetQueueSize(string channelId)
        {
            return _channelQueues.ContainsKey(channelId) ? _channelQueues[channelId].Count : 0;
        }
    }
}

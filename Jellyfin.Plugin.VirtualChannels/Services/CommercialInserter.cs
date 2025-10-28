using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.VirtualChannels.Configuration;
using Jellyfin.Plugin.VirtualChannels.Models;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.VirtualChannels.Services
{
    /// <summary>
    /// Handles commercial insertion logic for virtual channels.
    /// </summary>
    public class CommercialInserter
    {
        private readonly ChannelScheduler _scheduler;
        private readonly ILogger<CommercialInserter> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommercialInserter"/> class.
        /// </summary>
        /// <param name="scheduler">The channel scheduler.</param>
        /// <param name="logger">The logger.</param>
        public CommercialInserter(
            ChannelScheduler scheduler,
            ILogger<CommercialInserter> logger)
        {
            _scheduler = scheduler;
            _logger = logger;
        }

        /// <summary>
        /// Builds a playlist with commercial breaks inserted.
        /// </summary>
        /// <param name="mainContent">The main content item.</param>
        /// <param name="channelConfig">The channel configuration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of scheduled segments.</returns>
        public async Task<List<ScheduledSegment>> BuildPlaylistWithCommercials(
            BaseItem mainContent,
            VirtualChannelConfig channelConfig,
            CancellationToken cancellationToken)
        {
            var segments = new List<ScheduledSegment>();
            var config = Plugin.Instance?.Configuration;

            if (mainContent.RunTimeTicks == null || mainContent.RunTimeTicks <= 0)
            {
                _logger.LogWarning("Content {ContentName} has no valid runtime", mainContent.Name);
                return segments;
            }

            // Pre-roll commercials
            if (channelConfig.EnablePreRolls && !string.IsNullOrEmpty(config?.CommercialFolderPath))
            {
                var preRolls = await _scheduler.GetCommercials(
                    config.CommercialFolderPath,
                    2,
                    cancellationToken);

                foreach (var commercial in preRolls)
                {
                    segments.Add(new ScheduledSegment
                    {
                        Content = commercial,
                        Type = SegmentType.PreRoll,
                        Duration = TimeSpan.FromTicks(commercial.RunTimeTicks ?? 0)
                    });
                }
            }

            // Calculate mid-roll positions
            var contentDuration = TimeSpan.FromTicks(mainContent.RunTimeTicks.Value);
            var commercialInterval = TimeSpan.FromSeconds(channelConfig.CommercialIntervalSeconds);
            var midRollPositions = CalculateMidRollPositions(contentDuration, commercialInterval);

            // Split content with mid-rolls
            var lastPosition = TimeSpan.Zero;
            foreach (var position in midRollPositions)
            {
                // Add content segment
                segments.Add(new ScheduledSegment
                {
                    Content = mainContent,
                    Type = SegmentType.Content,
                    StartOffset = lastPosition,
                    Duration = position - lastPosition
                });

                // Add commercial break
                if (!string.IsNullOrEmpty(config?.CommercialFolderPath))
                {
                    var midRolls = await _scheduler.GetCommercials(
                        config.CommercialFolderPath,
                        3,
                        cancellationToken);

                    foreach (var commercial in midRolls)
                    {
                        segments.Add(new ScheduledSegment
                        {
                            Content = commercial,
                            Type = SegmentType.Commercial,
                            Duration = TimeSpan.FromTicks(commercial.RunTimeTicks ?? 0)
                        });
                    }
                }

                lastPosition = position;
            }

            // Add remaining content
            if (lastPosition < contentDuration)
            {
                segments.Add(new ScheduledSegment
                {
                    Content = mainContent,
                    Type = SegmentType.Content,
                    StartOffset = lastPosition,
                    Duration = contentDuration - lastPosition
                });
            }

            _logger.LogInformation(
                "Built playlist for {ContentName} with {SegmentCount} segments ({CommercialCount} commercial breaks)",
                mainContent.Name,
                segments.Count,
                midRollPositions.Count);

            return segments;
        }

        /// <summary>
        /// Calculates mid-roll commercial positions.
        /// </summary>
        private List<TimeSpan> CalculateMidRollPositions(
            TimeSpan contentDuration,
            TimeSpan interval)
        {
            var positions = new List<TimeSpan>();

            // Don't insert commercials if content is too short
            if (contentDuration < TimeSpan.FromMinutes(10))
            {
                return positions;
            }

            var currentPosition = interval;

            // Leave at least 5 minutes at the end without commercials
            var maxPosition = contentDuration - TimeSpan.FromMinutes(5);

            while (currentPosition < maxPosition)
            {
                positions.Add(currentPosition);
                currentPosition += interval;
            }

            return positions;
        }

        /// <summary>
        /// Calculates the total duration including commercials.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns>Total duration.</returns>
        public TimeSpan CalculateTotalDuration(List<ScheduledSegment> segments)
        {
            return TimeSpan.FromTicks(segments.Sum(s => s.Duration.Ticks));
        }

        /// <summary>
        /// Gets the segment at a specific time offset.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <param name="offset">The time offset.</param>
        /// <returns>The segment and relative offset within it.</returns>
        public (ScheduledSegment? Segment, TimeSpan RelativeOffset) GetSegmentAtOffset(
            List<ScheduledSegment> segments,
            TimeSpan offset)
        {
            var currentTime = TimeSpan.Zero;

            foreach (var segment in segments)
            {
                var segmentEnd = currentTime + segment.Duration;

                if (offset >= currentTime && offset < segmentEnd)
                {
                    return (segment, offset - currentTime);
                }

                currentTime = segmentEnd;
            }

            return (null, TimeSpan.Zero);
        }
    }
}

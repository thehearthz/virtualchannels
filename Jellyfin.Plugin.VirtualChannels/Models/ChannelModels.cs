using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.VirtualChannels.Models
{
    /// <summary>
    /// Represents a scheduled segment in a virtual channel.
    /// </summary>
    public class ScheduledSegment
    {
        /// <summary>
        /// Gets or sets the content item.
        /// </summary>
        public BaseItem? Content { get; set; }

        /// <summary>
        /// Gets or sets the segment type.
        /// </summary>
        public SegmentType Type { get; set; }

        /// <summary>
        /// Gets or sets the start offset in the content.
        /// </summary>
        public TimeSpan StartOffset { get; set; }

        /// <summary>
        /// Gets or sets the duration of the segment.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the scheduled start time.
        /// </summary>
        public DateTime ScheduledStart { get; set; }
    }

    /// <summary>
    /// Segment type enumeration.
    /// </summary>
    public enum SegmentType
    {
        /// <summary>
        /// Main content.
        /// </summary>
        Content,

        /// <summary>
        /// Commercial break.
        /// </summary>
        Commercial,

        /// <summary>
        /// Pre-roll content.
        /// </summary>
        PreRoll,

        /// <summary>
        /// Post-roll content.
        /// </summary>
        PostRoll
    }

    /// <summary>
    /// Represents a scheduled program in the EPG.
    /// </summary>
    public class ScheduledProgram
    {
        /// <summary>
        /// Gets or sets the program ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the content item.
        /// </summary>
        public BaseItem? Item { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the channel ID.
        /// </summary>
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the segments making up this program.
        /// </summary>
        public List<ScheduledSegment> Segments { get; set; } = new List<ScheduledSegment>();
    }

    /// <summary>
    /// Represents a virtual channel's current state.
    /// </summary>
    public class ChannelState
    {
        /// <summary>
        /// Gets or sets the channel ID.
        /// </summary>
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current program.
        /// </summary>
        public ScheduledProgram? CurrentProgram { get; set; }

        /// <summary>
        /// Gets or sets the content queue.
        /// </summary>
        public Queue<BaseItem> ContentQueue { get; set; } = new Queue<BaseItem>();

        /// <summary>
        /// Gets or sets the last update time.
        /// </summary>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the channel is currently streaming.
        /// </summary>
        public bool IsStreaming { get; set; }
    }

    /// <summary>
    /// Commercial break information.
    /// </summary>
    public class CommercialBreak
    {
        /// <summary>
        /// Gets or sets the position in the content.
        /// </summary>
        public TimeSpan Position { get; set; }

        /// <summary>
        /// Gets or sets the commercial items.
        /// </summary>
        public List<BaseItem> Commercials { get; set; } = new List<BaseItem>();

        /// <summary>
        /// Gets or sets the total duration.
        /// </summary>
        public TimeSpan Duration { get; set; }
    }
}

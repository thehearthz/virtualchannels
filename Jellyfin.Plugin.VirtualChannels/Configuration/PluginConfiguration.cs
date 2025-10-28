using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.VirtualChannels.Configuration
{
    /// <summary>
    /// Plugin configuration.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
            Channels = new List<VirtualChannelConfig>();
            CommercialFolderPath = string.Empty;
            EnableHardwareAcceleration = true;
            TranscodingPreset = "veryfast";
            DefaultCommercialInterval = 900; // 15 minutes
            BaseChannelNumber = 1000;
            EnableAutoChannelGeneration = true;
            AutoGenerateGenreChannels = true;
            AutoGenerateYearChannels = false;
        }

        /// <summary>
        /// Gets or sets the list of configured virtual channels.
        /// </summary>
        public List<VirtualChannelConfig> Channels { get; set; }

        /// <summary>
        /// Gets or sets the folder path containing commercial videos.
        /// </summary>
        public string CommercialFolderPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether hardware acceleration is enabled.
        /// </summary>
        public bool EnableHardwareAcceleration { get; set; }

        /// <summary>
        /// Gets or sets the FFmpeg transcoding preset.
        /// </summary>
        public string TranscodingPreset { get; set; }

        /// <summary>
        /// Gets or sets the default commercial interval in seconds.
        /// </summary>
        public int DefaultCommercialInterval { get; set; }

        /// <summary>
        /// Gets or sets the base channel number for virtual channels.
        /// </summary>
        public int BaseChannelNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether automatic channel generation is enabled.
        /// </summary>
        public bool EnableAutoChannelGeneration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to auto-generate genre-based channels.
        /// </summary>
        public bool AutoGenerateGenreChannels { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to auto-generate year-based channels.
        /// </summary>
        public bool AutoGenerateYearChannels { get; set; }

        /// <summary>
        /// Gets or sets the EPG days ahead to generate.
        /// </summary>
        public int EpgDaysAhead { get; set; } = 3;

        /// <summary>
        /// Gets or sets the streaming port.
        /// </summary>
        public int StreamingPort { get; set; } = 8097;
    }

    /// <summary>
    /// Configuration for a virtual channel.
    /// </summary>
    public class VirtualChannelConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualChannelConfig"/> class.
        /// </summary>
        public VirtualChannelConfig()
        {
            Name = string.Empty;
            Type = "Custom";
            ContentFilters = new List<string>();
            ShuffleMode = false;
            RespectEpisodeOrder = true;
            CommercialIntervalSeconds = 900;
            EnablePreRolls = true;
            LogoPath = string.Empty;
            Id = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets or sets the unique identifier for the channel.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the channel name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the channel number.
        /// </summary>
        public int ChannelNumber { get; set; }

        /// <summary>
        /// Gets or sets the channel type (Custom, Genre, Year, Series).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the content filters (genres, years, series IDs, tags).
        /// </summary>
        public List<string> ContentFilters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether shuffle mode is enabled.
        /// </summary>
        public bool ShuffleMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to respect episode order.
        /// </summary>
        public bool RespectEpisodeOrder { get; set; }

        /// <summary>
        /// Gets or sets the commercial interval in seconds.
        /// </summary>
        public int CommercialIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether pre-rolls are enabled.
        /// </summary>
        public bool EnablePreRolls { get; set; }

        /// <summary>
        /// Gets or sets the channel logo path/URL.
        /// </summary>
        public string LogoPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the channel is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}

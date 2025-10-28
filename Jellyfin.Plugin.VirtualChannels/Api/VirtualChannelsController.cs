using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.VirtualChannels.Services;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.VirtualChannels.Api
{
    /// <summary>
    /// API controller for virtual channels.
    /// </summary>
    [ApiController]
    [Route("api/virtualchannels")]
    [Authorize(Policy = "DefaultAuthorization")]
    public class VirtualChannelsController : ControllerBase
    {
        private readonly EpgGenerator _epgGenerator;
        private readonly ChannelScheduler _scheduler;
        private readonly ChannelStateManager _stateManager;
        private readonly AutoChannelGenerator _autoChannelGenerator;
        private readonly ILibraryManager _libraryManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualChannelsController"/> class.
        /// </summary>
        /// <param name="epgGenerator">The EPG generator.</param>
        /// <param name="scheduler">The channel scheduler.</param>
        /// <param name="stateManager">The state manager.</param>
        /// <param name="autoChannelGenerator">The auto channel generator.</param>
        /// <param name="libraryManager">The library manager.</param>
        public VirtualChannelsController(
            EpgGenerator epgGenerator,
            ChannelScheduler scheduler,
            ChannelStateManager stateManager,
            AutoChannelGenerator autoChannelGenerator,
            ILibraryManager libraryManager)
        {
            _epgGenerator = epgGenerator;
            _scheduler = scheduler;
            _stateManager = stateManager;
            _autoChannelGenerator = autoChannelGenerator;
            _libraryManager = libraryManager;
        }

        /// <summary>
        /// Gets the XMLTV EPG data.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>XMLTV formatted EPG data.</returns>
        [HttpGet("epg/xmltv")]
        [Produces("application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetXmltvGuide(CancellationToken cancellationToken)
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                return NotFound("Plugin configuration not found");
            }

            var xmltv = await _epgGenerator.GenerateXmltvGuide(
                config.Channels,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(config.EpgDaysAhead),
                cancellationToken);

            return Content(xmltv, "application/xml");
        }

        /// <summary>
        /// Gets an M3U playlist for a specific channel.
        /// </summary>
        /// <param name="channelNumber">The channel number.</param>
        /// <returns>M3U playlist.</returns>
        [HttpGet("{channelNumber}/playlist.m3u")]
        [Produces("application/x-mpegURL")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetM3uPlaylist(int channelNumber)
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                return NotFound();
            }

            var channel = config.Channels.Find(c => c.ChannelNumber == channelNumber);
            if (channel == null)
            {
                return NotFound($"Channel {channelNumber} not found");
            }

            var m3u = $"#EXTM3U\n" +
                      $"#EXTINF:-1 tvg-id=\"virtual_{channelNumber}\" tvg-name=\"{channel.Name}\" tvg-logo=\"{channel.LogoPath}\",{channel.Name}\n" +
                      $"http://localhost:{config.StreamingPort}/virtualchannels/{channelNumber}/stream.m3u8\n";

            return Content(m3u, "application/x-mpegURL");
        }

        /// <summary>
        /// Gets a master M3U playlist for all channels.
        /// </summary>
        /// <returns>M3U playlist with all channels.</returns>
        [HttpGet("playlist.m3u")]
        [Produces("application/x-mpegURL")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetMasterPlaylist()
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                return NotFound();
            }

            var m3u = "#EXTM3U\n";
            foreach (var channel in config.Channels)
            {
                if (!channel.Enabled)
                {
                    continue;
                }

                m3u += $"#EXTINF:-1 tvg-id=\"virtual_{channel.ChannelNumber}\" tvg-name=\"{channel.Name}\" tvg-logo=\"{channel.LogoPath}\",{channel.Name}\n";
                m3u += $"http://localhost:{config.StreamingPort}/virtualchannels/{channel.ChannelNumber}/stream.m3u8\n";
            }

            return Content(m3u, "application/x-mpegURL");
        }

        /// <summary>
        /// Refreshes channel queues.
        /// </summary>
        /// <param name="channelId">Optional channel ID to refresh. If null, refreshes all.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Action result.</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshChannels([FromQuery] string? channelId, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(channelId))
            {
                _scheduler.ClearQueue(channelId);
                return Ok(new { message = $"Channel {channelId} refreshed" });
            }

            // Refresh all channels
            var config = Plugin.Instance?.Configuration;
            if (config != null)
            {
                foreach (var channel in config.Channels)
                {
                    _scheduler.ClearQueue($"virtual_{channel.ChannelNumber}");
                }
            }

            return Ok(new { message = "All channels refreshed" });
        }

        /// <summary>
        /// Gets channel statistics.
        /// </summary>
        /// <returns>Statistics data.</returns>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetStatistics()
        {
            var stats = _stateManager.GetStatistics();
            return Ok(stats);
        }

        /// <summary>
        /// Generates auto channels.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Action result.</returns>
        [HttpPost("auto-generate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateAutoChannels(CancellationToken cancellationToken)
        {
            await _autoChannelGenerator.UpdateAutoChannels(cancellationToken);
            return Ok(new { message = "Auto channels generated successfully" });
        }

        /// <summary>
        /// Gets available genres for channel creation.
        /// </summary>
        /// <returns>List of genres.</returns>
        [HttpGet("genres")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetGenres()
        {
            var query = new MediaBrowser.Model.Querying.InternalItemsQuery
            {
                Recursive = true,
                IncludeItemTypes = new[] { Jellyfin.Data.Enums.BaseItemKind.Movie, Jellyfin.Data.Enums.BaseItemKind.Episode }
            };

            var items = _libraryManager.GetItemList(query);
            var genres = items
                .Where(i => i.Genres != null && i.Genres.Length > 0)
                .SelectMany(i => i.Genres)
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            return Ok(genres);
        }
    }
}

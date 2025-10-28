using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Jellyfin.Plugin.VirtualChannels.Configuration;
using Jellyfin.Plugin.VirtualChannels.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.VirtualChannels.Services
{
    /// <summary>
    /// Generates Electronic Program Guide (EPG) data in XMLTV format.
    /// </summary>
    public class EpgGenerator
    {
        private readonly ChannelScheduler _scheduler;
        private readonly CommercialInserter _commercialInserter;
        private readonly ILogger<EpgGenerator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EpgGenerator"/> class.
        /// </summary>
        /// <param name="scheduler">The channel scheduler.</param>
        /// <param name="commercialInserter">The commercial inserter.</param>
        /// <param name="logger">The logger.</param>
        public EpgGenerator(
            ChannelScheduler scheduler,
            CommercialInserter commercialInserter,
            ILogger<EpgGenerator> logger)
        {
            _scheduler = scheduler;
            _commercialInserter = commercialInserter;
            _logger = logger;
        }

        /// <summary>
        /// Generates XMLTV format EPG data.
        /// </summary>
        /// <param name="channels">The channel configurations.</param>
        /// <param name="startDate">Start date for EPG.</param>
        /// <param name="endDate">End date for EPG.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>XMLTV formatted string.</returns>
        public async Task<string> GenerateXmltvGuide(
            List<VirtualChannelConfig> channels,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                Async = true
            };

            using var stringWriter = new StringWriter();
            using var writer = XmlWriter.Create(stringWriter, settings);

            await writer.WriteStartDocumentAsync();
            await writer.WriteStartElementAsync(null, "tv", null);
            await writer.WriteAttributeStringAsync(null, "generator-info-name", null, "Jellyfin Virtual Channels");
            await writer.WriteAttributeStringAsync(null, "generator-info-url", null, "https://github.com/jellyfin/jellyfin-plugin-virtualchannels");

            // Write channel definitions
            foreach (var channel in channels.Where(c => c.Enabled))
            {
                await WriteChannelDefinition(writer, channel);
            }

            // Write program entries
            foreach (var channel in channels.Where(c => c.Enabled))
            {
                try
                {
                    var programs = await GenerateProgramSchedule(channel, startDate, endDate, cancellationToken);

                    foreach (var program in programs)
                    {
                        await WriteProgramEntry(writer, channel, program);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating programs for channel {ChannelName}", channel.Name);
                }
            }

            await writer.WriteEndElementAsync(); // tv
            await writer.WriteEndDocumentAsync();
            await writer.FlushAsync();

            return stringWriter.ToString();
        }

        /// <summary>
        /// Writes a channel definition to the XML.
        /// </summary>
        private async Task WriteChannelDefinition(XmlWriter writer, VirtualChannelConfig channel)
        {
            await writer.WriteStartElementAsync(null, "channel", null);
            await writer.WriteAttributeStringAsync(null, "id", null, $"virtual_{channel.ChannelNumber}");

            await writer.WriteElementStringAsync(null, "display-name", null, channel.Name);

            if (!string.IsNullOrEmpty(channel.LogoPath))
            {
                await writer.WriteStartElementAsync(null, "icon", null);
                await writer.WriteAttributeStringAsync(null, "src", null, channel.LogoPath);
                await writer.WriteEndElementAsync();
            }

            await writer.WriteEndElementAsync(); // channel
        }

        /// <summary>
        /// Generates the program schedule for a channel.
        /// </summary>
        private async Task<List<ScheduledProgram>> GenerateProgramSchedule(
            VirtualChannelConfig channel,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken)
        {
            var programs = new List<ScheduledProgram>();
            var currentTime = startDate;

            while (currentTime < endDate)
            {
                var item = await _scheduler.GetNextItemForChannel(
                    $"virtual_{channel.ChannelNumber}",
                    channel,
                    cancellationToken);

                if (item == null)
                {
                    _logger.LogWarning("No content available for channel {ChannelName}", channel.Name);
                    break;
                }

                // Build segments with commercials
                var segments = await _commercialInserter.BuildPlaylistWithCommercials(
                    item,
                    channel,
                    cancellationToken);

                var duration = _commercialInserter.CalculateTotalDuration(segments);

                programs.Add(new ScheduledProgram
                {
                    Id = Guid.NewGuid().ToString(),
                    Item = item,
                    StartTime = currentTime,
                    EndTime = currentTime + duration,
                    ChannelId = $"virtual_{channel.ChannelNumber}",
                    Segments = segments
                });

                currentTime += duration;
            }

            return programs;
        }

        /// <summary>
        /// Writes a program entry to the XML.
        /// </summary>
        private async Task WriteProgramEntry(
            XmlWriter writer,
            VirtualChannelConfig channel,
            ScheduledProgram program)
        {
            if (program.Item == null)
            {
                return;
            }

            await writer.WriteStartElementAsync(null, "programme", null);

            await writer.WriteAttributeStringAsync(null, "start", null,
                program.StartTime.ToString("yyyyMMddHHmmss zzz").Replace(":", ""));
            await writer.WriteAttributeStringAsync(null, "stop", null,
                program.EndTime.ToString("yyyyMMddHHmmss zzz").Replace(":", ""));
            await writer.WriteAttributeStringAsync(null, "channel", null,
                $"virtual_{channel.ChannelNumber}");

            // Title
            await writer.WriteElementStringAsync(null, "title", null, program.Item.Name ?? "Unknown");

            // Description
            if (!string.IsNullOrEmpty(program.Item.Overview))
            {
                await writer.WriteElementStringAsync(null, "desc", null, program.Item.Overview);
            }

            // Episode info for TV shows
            if (program.Item is Episode episode)
            {
                if (episode.ParentIndexNumber.HasValue && episode.IndexNumber.HasValue)
                {
                    await writer.WriteStartElementAsync(null, "episode-num", null);
                    await writer.WriteAttributeStringAsync(null, "system", null, "onscreen");
                    await writer.WriteStringAsync(
                        $"S{episode.ParentIndexNumber:D2}E{episode.IndexNumber:D2}");
                    await writer.WriteEndElementAsync();

                    // XMLTV format (season.episode)
                    await writer.WriteStartElementAsync(null, "episode-num", null);
                    await writer.WriteAttributeStringAsync(null, "system", null, "xmltv_ns");
                    await writer.WriteStringAsync(
                        $"{episode.ParentIndexNumber - 1}.{episode.IndexNumber - 1}.");
                    await writer.WriteEndElementAsync();
                }
            }

            // Genres
            if (program.Item.Genres != null && program.Item.Genres.Length > 0)
            {
                foreach (var genre in program.Item.Genres.Take(3))
                {
                    await writer.WriteElementStringAsync(null, "category", null, genre);
                }
            }

            // Year
            if (program.Item.ProductionYear.HasValue)
            {
                await writer.WriteElementStringAsync(null, "date", null,
                    program.Item.ProductionYear.Value.ToString());
            }

            await writer.WriteEndElementAsync(); // programme
        }
    }
}

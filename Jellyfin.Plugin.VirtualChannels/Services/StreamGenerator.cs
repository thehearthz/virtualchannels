using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.VirtualChannels.Services
{
    /// <summary>
    /// Generates HLS streams for virtual channels using FFmpeg.
    /// </summary>
    public class StreamGenerator
    {
        private readonly ILogger<StreamGenerator> _logger;
        private readonly string _transcodePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamGenerator"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public StreamGenerator(ILogger<StreamGenerator> logger)
        {
            _logger = logger;
            _transcodePath = Path.Combine(Path.GetTempPath(), "jellyfin-virtualchannels");
            Directory.CreateDirectory(_transcodePath);
        }

        /// <summary>
        /// Generates an HLS stream for an item.
        /// </summary>
        /// <param name="item">The media item.</param>
        /// <param name="channelId">The channel ID.</param>
        /// <param name="startOffset">Start offset in the video.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Path to the HLS playlist.</returns>
        public async Task<string> GenerateHlsStream(
            BaseItem item,
            string channelId,
            TimeSpan startOffset,
            CancellationToken cancellationToken)
        {
            var outputPath = Path.Combine(_transcodePath, channelId);
            Directory.CreateDirectory(outputPath);

            var playlistPath = Path.Combine(outputPath, "stream.m3u8");

            // Build FFmpeg arguments
            var ffmpegArgs = BuildFfmpegArgs(item.Path, outputPath, startOffset);

            _logger.LogInformation(
                "Starting FFmpeg transcode for {ItemName} on channel {ChannelId}",
                item.Name,
                channelId);

            var processInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var process = Process.Start(processInfo);
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start FFmpeg process");
                }

                // Monitor process output asynchronously
                _ = Task.Run(async () =>
                {
                    while (!process.StandardError.EndOfStream)
                    {
                        var line = await process.StandardError.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line))
                        {
                            _logger.LogDebug("FFmpeg [{ChannelId}]: {Line}", channelId, line);
                        }
                    }
                }, cancellationToken);

                return playlistPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting FFmpeg for channel {ChannelId}", channelId);
                throw;
            }
        }

        /// <summary>
        /// Builds FFmpeg arguments for transcoding.
        /// </summary>
        private string BuildFfmpegArgs(string inputPath, string outputPath, TimeSpan startOffset)
        {
            var config = Plugin.Instance?.Configuration;
            var preset = config?.TranscodingPreset ?? "veryfast";
            var hwAccel = config?.EnableHardwareAcceleration == true;

            var args = string.Empty;

            // Hardware acceleration if enabled
            if (hwAccel)
            {
                args += "-hwaccel auto ";
            }

            // Start offset
            if (startOffset > TimeSpan.Zero)
            {
                args += $"-ss {startOffset.TotalSeconds:F3} ";
            }

            // Input file
            args += $"-i \"{inputPath}\" ";

            // Video encoding: H.264, 1920x1080 max, 30fps, 4Mbps
            args += "-c:v libx264 " +
                    $"-preset {preset} " +
                    "-b:v 4M " +
                    "-maxrate 4M " +
                    "-bufsize 8M " +
                    "-vf \"scale='min(1920,iw)':'min(1080,ih)':force_original_aspect_ratio=decrease,pad=1920:1080:(ow-iw)/2:(oh-ih)/2,fps=30\" " +
                    "-g 60 " +  // Keyframe every 2 seconds at 30fps
                    "-sc_threshold 0 " +  // Disable scene change detection
                    "-force_key_frames \"expr:gte(t,n_forced*2)\" ";  // Force keyframes

            // Audio encoding: AAC, stereo, 128kbps
            args += "-c:a aac " +
                    "-b:a 128k " +
                    "-ac 2 " +
                    "-ar 48000 ";

            // HLS output settings
            args += "-f hls " +
                    "-hls_time 2 " +  // 2-second segments
                    "-hls_list_size 10 " +  // Keep last 10 segments in playlist
                    "-hls_flags delete_segments+append_list+omit_endlist " +
                    "-hls_segment_type mpegts " +
                    $"-hls_segment_filename \"{Path.Combine(outputPath, "segment_%03d.ts")}\" " +
                    $"\"{Path.Combine(outputPath, "stream.m3u8")}\"";

            return args;
        }

        /// <summary>
        /// Cleans up old transcode files.
        /// </summary>
        /// <param name="olderThan">Delete files older than this timespan.</param>
        public void CleanupOldFiles(TimeSpan olderThan)
        {
            try
            {
                if (!Directory.Exists(_transcodePath))
                {
                    return;
                }

                var cutoffTime = DateTime.Now - olderThan;
                var directories = Directory.GetDirectories(_transcodePath);

                foreach (var dir in directories)
                {
                    var dirInfo = new DirectoryInfo(dir);
                    if (dirInfo.LastWriteTime < cutoffTime)
                    {
                        _logger.LogDebug("Cleaning up old transcode directory: {Directory}", dir);
                        Directory.Delete(dir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old transcode files");
            }
        }

        /// <summary>
        /// Gets the transcode path for a channel.
        /// </summary>
        /// <param name="channelId">The channel ID.</param>
        /// <returns>The transcode path.</returns>
        public string GetTranscodePath(string channelId)
        {
            return Path.Combine(_transcodePath, channelId);
        }
    }
}

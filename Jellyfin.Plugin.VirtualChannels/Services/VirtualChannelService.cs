using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.VirtualChannels.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.VirtualChannels
{
    /// <summary>
    /// Background service for managing virtual channels.
    /// </summary>
    public class VirtualChannelService : IHostedService, IDisposable
    {
        private readonly ChannelScheduler _scheduler;
        private readonly StreamGenerator _streamGenerator;
        private readonly AutoChannelGenerator _autoChannelGenerator;
        private readonly ChannelStateManager _stateManager;
        private readonly ILogger<VirtualChannelService> _logger;
        private Timer? _maintenanceTimer;
        private Timer? _autoChannelTimer;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualChannelService"/> class.
        /// </summary>
        /// <param name="scheduler">The channel scheduler.</param>
        /// <param name="streamGenerator">The stream generator.</param>
        /// <param name="autoChannelGenerator">The auto channel generator.</param>
        /// <param name="stateManager">The state manager.</param>
        /// <param name="logger">The logger.</param>
        public VirtualChannelService(
            ChannelScheduler scheduler,
            StreamGenerator streamGenerator,
            AutoChannelGenerator autoChannelGenerator,
            ChannelStateManager stateManager,
            ILogger<VirtualChannelService> logger)
        {
            _scheduler = scheduler;
            _streamGenerator = streamGenerator;
            _autoChannelGenerator = autoChannelGenerator;
            _stateManager = stateManager;
            _logger = logger;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Virtual Channel Service starting");

            // Run maintenance every 5 minutes
            _maintenanceTimer = new Timer(
                DoMaintenance,
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(5));

            // Update auto-generated channels every hour
            _autoChannelTimer = new Timer(
                DoAutoChannelUpdate,
                null,
                TimeSpan.FromMinutes(1), // First run after 1 minute
                TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs maintenance tasks.
        /// </summary>
        private void DoMaintenance(object? state)
        {
            try
            {
                _logger.LogDebug("Running maintenance tasks");

                // Clean up old transcode files (older than 1 hour)
                _streamGenerator.CleanupOldFiles(TimeSpan.FromHours(1));

                // Log statistics
                var stats = _stateManager.GetStatistics();
                _logger.LogInformation(
                    "Channel Statistics - Total: {Total}, Streaming: {Streaming}, Active: {Active}",
                    stats["TotalChannels"],
                    stats["StreamingChannels"],
                    stats["ActiveChannels"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during maintenance");
            }
        }

        /// <summary>
        /// Updates auto-generated channels.
        /// </summary>
        private async void DoAutoChannelUpdate(object? state)
        {
            try
            {
                var config = Plugin.Instance?.Configuration;
                if (config?.EnableAutoChannelGeneration == true)
                {
                    _logger.LogInformation("Updating auto-generated channels");
                    await _autoChannelGenerator.UpdateAutoChannels(CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating auto-generated channels");
            }
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Virtual Channel Service stopping");

            _maintenanceTimer?.Change(Timeout.Infinite, 0);
            _autoChannelTimer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _maintenanceTimer?.Dispose();
            _autoChannelTimer?.Dispose();
            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}

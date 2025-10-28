using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.VirtualChannels.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.VirtualChannels.Services
{
    /// <summary>
    /// Manages the state of all virtual channels.
    /// </summary>
    public class ChannelStateManager
    {
        private readonly ILogger<ChannelStateManager> _logger;
        private readonly ConcurrentDictionary<string, ChannelState> _channelStates;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelStateManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ChannelStateManager(ILogger<ChannelStateManager> logger)
        {
            _logger = logger;
            _channelStates = new ConcurrentDictionary<string, ChannelState>();
        }

        /// <summary>
        /// Gets or creates the state for a channel.
        /// </summary>
        /// <param name="channelId">The channel ID.</param>
        /// <returns>The channel state.</returns>
        public ChannelState GetOrCreateState(string channelId)
        {
            return _channelStates.GetOrAdd(channelId, id => new ChannelState
            {
                ChannelId = id,
                LastUpdate = DateTime.UtcNow,
                IsStreaming = false
            });
        }

        /// <summary>
        /// Updates the current program for a channel.
        /// </summary>
        /// <param name="channelId">The channel ID.</param>
        /// <param name="program">The current program.</param>
        public void UpdateCurrentProgram(string channelId, ScheduledProgram program)
        {
            var state = GetOrCreateState(channelId);
            state.CurrentProgram = program;
            state.LastUpdate = DateTime.UtcNow;

            _logger.LogDebug(
                "Updated current program for channel {ChannelId}: {ProgramName}",
                channelId,
                program.Item?.Name ?? "Unknown");
        }

        /// <summary>
        /// Gets the current program for a channel.
        /// </summary>
        /// <param name="channelId">The channel ID.</param>
        /// <returns>The current program, or null if none.</returns>
        public ScheduledProgram? GetCurrentProgram(string channelId)
        {
            if (_channelStates.TryGetValue(channelId, out var state))
            {
                return state.CurrentProgram;
            }

            return null;
        }

        /// <summary>
        /// Sets the streaming state for a channel.
        /// </summary>
        /// <param name="channelId">The channel ID.</param>
        /// <param name="isStreaming">Whether the channel is streaming.</param>
        public void SetStreamingState(string channelId, bool isStreaming)
        {
            var state = GetOrCreateState(channelId);
            state.IsStreaming = isStreaming;
            state.LastUpdate = DateTime.UtcNow;

            _logger.LogInformation(
                "Channel {ChannelId} streaming state: {IsStreaming}",
                channelId,
                isStreaming);
        }

        /// <summary>
        /// Gets whether a channel is currently streaming.
        /// </summary>
        /// <param name="channelId">The channel ID.</param>
        /// <returns>True if streaming, false otherwise.</returns>
        public bool IsChannelStreaming(string channelId)
        {
            if (_channelStates.TryGetValue(channelId, out var state))
            {
                return state.IsStreaming;
            }

            return false;
        }

        /// <summary>
        /// Gets all channel states.
        /// </summary>
        /// <returns>Dictionary of channel states.</returns>
        public Dictionary<string, ChannelState> GetAllStates()
        {
            return _channelStates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Clears the state for a channel.
        /// </summary>
        /// <param name="channelId">The channel ID.</param>
        public void ClearState(string channelId)
        {
            if (_channelStates.TryRemove(channelId, out _))
            {
                _logger.LogInformation("Cleared state for channel {ChannelId}", channelId);
            }
        }

        /// <summary>
        /// Clears all channel states.
        /// </summary>
        public void ClearAllStates()
        {
            _channelStates.Clear();
            _logger.LogInformation("Cleared all channel states");
        }

        /// <summary>
        /// Gets statistics about channel states.
        /// </summary>
        /// <returns>Statistics dictionary.</returns>
        public Dictionary<string, object> GetStatistics()
        {
            var stats = new Dictionary<string, object>
            {
                ["TotalChannels"] = _channelStates.Count,
                ["StreamingChannels"] = _channelStates.Count(kvp => kvp.Value.IsStreaming),
                ["ActiveChannels"] = _channelStates.Count(kvp =>
                    kvp.Value.LastUpdate > DateTime.UtcNow.AddMinutes(-5))
            };

            return stats;
        }
    }
}

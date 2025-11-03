using Jellyfin.Plugin.VirtualChannels.Api;
using Jellyfin.Plugin.VirtualChannels.LiveTV;
using Jellyfin.Plugin.VirtualChannels.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.VirtualChannels
{
    /// <summary>
    /// Register plugin services.
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            // Register core services as singletons
            serviceCollection.AddSingleton<ChannelScheduler>();
            serviceCollection.AddSingleton<StreamGenerator>();
            serviceCollection.AddSingleton<CommercialInserter>();
            serviceCollection.AddSingleton<EpgGenerator>();
            serviceCollection.AddSingleton<AutoChannelGenerator>();
            serviceCollection.AddSingleton<ChannelStateManager>();

            // Register Live TV provider
            serviceCollection.AddSingleton<VirtualChannelProvider>();

            // Register background service for channel management
            serviceCollection.AddHostedService<VirtualChannelService>();

            // Register API controller
            serviceCollection.AddSingleton<VirtualChannelsController>();
        }
    }
}

const pluginId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

// Load configuration on page show
document.querySelector('#VirtualChannelsConfigPage').addEventListener('pageshow', async function() {
    Dashboard.showLoadingMsg();
    
    try {
        const config = await ApiClient.getPluginConfiguration(pluginId);
        loadConfiguration(config);
        renderChannelList(config.Channels || []);
        Dashboard.hideLoadingMsg();
    } catch (err) {
        console.error('Error loading configuration:', err);
        Dashboard.alert('Error loading configuration: ' + err.message);
        Dashboard.hideLoadingMsg();
    }
});

// Load configuration values into form
function loadConfiguration(config) {
    document.querySelector('#CommercialFolderPath').value = config.CommercialFolderPath || '';
    document.querySelector('#DefaultCommercialInterval').value = config.DefaultCommercialInterval || 900;
    document.querySelector('#BaseChannelNumber').value = config.BaseChannelNumber || 1000;
    document.querySelector('#StreamingPort').value = config.StreamingPort || 8097;
    document.querySelector('#EpgDaysAhead').value = config.EpgDaysAhead || 3;
    document.querySelector('#EnableHardwareAcceleration').checked = config.EnableHardwareAcceleration !== false;
    document.querySelector('#TranscodingPreset').value = config.TranscodingPreset || 'veryfast';
    document.querySelector('#EnableAutoChannelGeneration').checked = config.EnableAutoChannelGeneration === true;
    document.querySelector('#AutoGenerateGenreChannels').checked = config.AutoGenerateGenreChannels === true;
    document.querySelector('#AutoGenerateYearChannels').checked = config.AutoGenerateYearChannels === true;
}

// Save configuration on form submit
document.querySelector('#VirtualChannelsConfigForm').addEventListener('submit', async function(e) {
    e.preventDefault();
    Dashboard.showLoadingMsg();
    
    try {
        const config = await ApiClient.getPluginConfiguration(pluginId);
        
        // Update configuration with form values
        config.CommercialFolderPath = document.querySelector('#CommercialFolderPath').value;
        config.DefaultCommercialInterval = parseInt(document.querySelector('#DefaultCommercialInterval').value);
        config.BaseChannelNumber = parseInt(document.querySelector('#BaseChannelNumber').value);
        config.StreamingPort = parseInt(document.querySelector('#StreamingPort').value);
        config.EpgDaysAhead = parseInt(document.querySelector('#EpgDaysAhead').value);
        config.EnableHardwareAcceleration = document.querySelector('#EnableHardwareAcceleration').checked;
        config.TranscodingPreset = document.querySelector('#TranscodingPreset').value;
        config.EnableAutoChannelGeneration = document.querySelector('#EnableAutoChannelGeneration').checked;
        config.AutoGenerateGenreChannels = document.querySelector('#AutoGenerateGenreChannels').checked;
        config.AutoGenerateYearChannels = document.querySelector('#AutoGenerateYearChannels').checked;
        
        await ApiClient.updatePluginConfiguration(pluginId, config);
        Dashboard.processPluginConfigurationUpdateResult();
    } catch (err) {
        console.error('Error saving configuration:', err);
        Dashboard.alert('Error saving configuration: ' + err.message);
    } finally {
        Dashboard.hideLoadingMsg();
    }
});

// Add channel button handler
document.querySelector('#addChannel').addEventListener('click', async function() {
    const channelName = prompt('Enter channel name:');
    if (!channelName) return;
    
    const channelNumber = prompt('Enter channel number:', '1000');
    if (!channelNumber) return;
    
    const channelType = prompt('Enter channel type (Custom, Genre, Year, Series):', 'Custom');
    if (!channelType) return;
    
    try {
        const config = await ApiClient.getPluginConfiguration(pluginId);
        
        config.Channels.push({
            Id: generateGuid(),
            Name: channelName,
            ChannelNumber: parseInt(channelNumber),
            Type: channelType,
            ContentFilters: [],
            ShuffleMode: false,
            RespectEpisodeOrder: true,
            CommercialIntervalSeconds: config.DefaultCommercialInterval,
            EnablePreRolls: true,
            LogoPath: '',
            Enabled: true
        });
        
        await ApiClient.updatePluginConfiguration(pluginId, config);
        renderChannelList(config.Channels);
        Dashboard.alert('Channel added successfully!');
    } catch (err) {
        console.error('Error adding channel:', err);
        Dashboard.alert('Error adding channel: ' + err.message);
    }
});

// Render channel list
function renderChannelList(channels) {
    const container = document.querySelector('#channelList');
    container.innerHTML = '';
    
    if (channels.length === 0) {
        container.innerHTML = '<div class="listItem"><div class="listItemBody">No channels configured. Add a custom channel or enable auto-generation.</div></div>';
        return;
    }
    
    channels.forEach((channel, index) => {
        const channelDiv = document.createElement('div');
        channelDiv.className = 'listItem';
        channelDiv.innerHTML = `
            <div class="listItemBody">
                <h3 class="listItemBodyText">${channel.Name}</h3>
                <div class="secondary listItemBodyText">
                    Channel ${channel.ChannelNumber} - ${channel.Type}
                    ${channel.Enabled ? '' : ' (Disabled)'}
                </div>
                <div class="secondary listItemBodyText">
                    ${channel.ShuffleMode ? 'Shuffle' : 'Sequential'}
                    ${channel.RespectEpisodeOrder ? ' - Episode Order' : ''}
                    - Commercials every ${Math.floor(channel.CommercialIntervalSeconds / 60)} min
                </div>
            </div>
            <button is="emby-button" class="paper-icon-button-light btnEditChannel" data-index="${index}" title="Edit">
                <span class="material-icons edit"></span>
            </button>
            <button is="emby-button" class="paper-icon-button-light btnDeleteChannel" data-index="${index}" title="Delete">
                <span class="material-icons delete"></span>
            </button>
        `;
        container.appendChild(channelDiv);
    });
    
    // Attach event handlers
    document.querySelectorAll('.btnEditChannel').forEach(btn => {
        btn.addEventListener('click', async function() {
            const index = parseInt(this.getAttribute('data-index'));
            await editChannel(index);
        });
    });
    
    document.querySelectorAll('.btnDeleteChannel').forEach(btn => {
        btn.addEventListener('click', async function() {
            const index = parseInt(this.getAttribute('data-index'));
            await deleteChannel(index);
        });
    });
}

// Edit channel
async function editChannel(index) {
    try {
        const config = await ApiClient.getPluginConfiguration(pluginId);
        const channel = config.Channels[index];
        
        const newName = prompt('Channel name:', channel.Name);
        if (newName === null) return;
        
        const newNumber = prompt('Channel number:', channel.ChannelNumber);
        if (newNumber === null) return;
        
        const enabledStr = prompt('Enabled? (yes/no):', channel.Enabled ? 'yes' : 'no');
        if (enabledStr === null) return;
        
        channel.Name = newName;
        channel.ChannelNumber = parseInt(newNumber);
        channel.Enabled = enabledStr.toLowerCase() === 'yes';
        
        await ApiClient.updatePluginConfiguration(pluginId, config);
        renderChannelList(config.Channels);
        Dashboard.alert('Channel updated successfully!');
    } catch (err) {
        console.error('Error editing channel:', err);
        Dashboard.alert('Error editing channel: ' + err.message);
    }
}

// Delete channel
async function deleteChannel(index) {
    if (!confirm('Are you sure you want to delete this channel?')) {
        return;
    }
    
    try {
        const config = await ApiClient.getPluginConfiguration(pluginId);
        config.Channels.splice(index, 1);
        
        await ApiClient.updatePluginConfiguration(pluginId, config);
        renderChannelList(config.Channels);
        Dashboard.alert('Channel deleted successfully!');
    } catch (err) {
        console.error('Error deleting channel:', err);
        Dashboard.alert('Error deleting channel: ' + err.message);
    }
}

// Generate auto channels
document.querySelector('#generateAutoChannels').addEventListener('click', async function() {
    if (!confirm('This will regenerate all auto-generated channels. Continue?')) {
        return;
    }
    
    Dashboard.showLoadingMsg();
    
    try {
        await ApiClient.fetch({
            url: ApiClient.getUrl('/api/virtualchannels/auto-generate'),
            type: 'POST'
        });
        
        const config = await ApiClient.getPluginConfiguration(pluginId);
        renderChannelList(config.Channels);
        Dashboard.alert('Auto channels generated successfully!');
    } catch (err) {
        console.error('Error generating auto channels:', err);
        Dashboard.alert('Error generating auto channels: ' + err.message);
    } finally {
        Dashboard.hideLoadingMsg();
    }
});

// Refresh channels
document.querySelector('#refreshChannels').addEventListener('click', async function() {
    Dashboard.showLoadingMsg();
    
    try {
        await ApiClient.fetch({
            url: ApiClient.getUrl('/api/virtualchannels/refresh'),
            type: 'POST'
        });
        
        Dashboard.alert('All channels refreshed successfully!');
    } catch (err) {
        console.error('Error refreshing channels:', err);
        Dashboard.alert('Error refreshing channels: ' + err.message);
    } finally {
        Dashboard.hideLoadingMsg();
    }
});

// Utility function to generate GUID
function generateGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        var r = Math.random() * 16 | 0;
        var v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

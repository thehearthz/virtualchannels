const pluginId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
let currentConfig = null;
let editingChannelIndex = -1;

// Load configuration on page show
document.querySelector('#VirtualChannelsConfigPage').addEventListener('pageshow', async function() {
    Dashboard.showLoadingMsg();
    
    try {
        currentConfig = await ApiClient.getPluginConfiguration(pluginId);
        loadConfiguration(currentConfig);
        renderChannelList(currentConfig.Channels || []);
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
        // Get fresh config to preserve channels
        const config = await ApiClient.getPluginConfiguration(pluginId);
        
        // Update configuration with form values
        config.CommercialFolderPath = document.querySelector('#CommercialFolderPath').value;
        config.DefaultCommercialInterval = parseInt(document.querySelector('#DefaultCommercialInterval').value) || 900;
        config.BaseChannelNumber = parseInt(document.querySelector('#BaseChannelNumber').value) || 1000;
        config.StreamingPort = parseInt(document.querySelector('#StreamingPort').value) || 8097;
        config.EpgDaysAhead = parseInt(document.querySelector('#EpgDaysAhead').value) || 3;
        config.EnableHardwareAcceleration = document.querySelector('#EnableHardwareAcceleration').checked;
        config.TranscodingPreset = document.querySelector('#TranscodingPreset').value;
        config.EnableAutoChannelGeneration = document.querySelector('#EnableAutoChannelGeneration').checked;
        config.AutoGenerateGenreChannels = document.querySelector('#AutoGenerateGenreChannels').checked;
        config.AutoGenerateYearChannels = document.querySelector('#AutoGenerateYearChannels').checked;
        
        await ApiClient.updatePluginConfiguration(pluginId, config);
        
        // Reload to verify save
        currentConfig = await ApiClient.getPluginConfiguration(pluginId);
        loadConfiguration(currentConfig);
        
        Dashboard.alert('Configuration saved successfully! Restart Jellyfin for changes to take effect.');
        Dashboard.hideLoadingMsg();
    } catch (err) {
        console.error('Error saving configuration:', err);
        Dashboard.alert('Error saving configuration: ' + err.message);
        Dashboard.hideLoadingMsg();
    }
});

// Render channel list
function renderChannelList(channels) {
    const container = document.querySelector('#channelList');
    container.innerHTML = '';
    
    if (channels.length === 0) {
        container.innerHTML = '<div class="emptyMessage">No channels configured. Add a custom channel or enable auto-generation.</div>';
        return;
    }
    
    channels.forEach((channel, index) => {
        const channelCard = document.createElement('div');
        channelCard.className = 'channelCard';
        
        const details = [];
        details.push(`Channel ${channel.ChannelNumber}`);
        details.push(channel.Type);
        if (channel.ShuffleMode) details.push('Shuffle');
        if (channel.RespectEpisodeOrder) details.push('Episode Order');
        details.push(`Commercials: ${Math.floor(channel.CommercialIntervalSeconds / 60)}min`);
        if (!channel.Enabled) details.push('DISABLED');
        
        channelCard.innerHTML = `
            <div class="channelInfo">
                <div class="channelTitle">${escapeHtml(channel.Name)}</div>
                <div class="channelDetails">${details.join(' • ')}</div>
                ${channel.ContentFilters && channel.ContentFilters.length > 0 ? 
                    `<div class="channelDetails">Filters: ${channel.ContentFilters.join(', ')}</div>` : ''}
            </div>
            <div class="channelActions">
                <button is="emby-button" class="paper-icon-button-light btnEditChannel" data-index="${index}" title="Edit">
                    <span class="material-icons edit"></span>
                </button>
                <button is="emby-button" class="paper-icon-button-light btnDeleteChannel" data-index="${index}" title="Delete">
                    <span class="material-icons delete"></span>
                </button>
            </div>
        `;
        
        container.appendChild(channelCard);
    });
    
    // Attach event handlers
    document.querySelectorAll('.btnEditChannel').forEach(btn => {
        btn.addEventListener('click', function() {
            const index = parseInt(this.getAttribute('data-index'));
            editChannel(index);
        });
    });
    
    document.querySelectorAll('.btnDeleteChannel').forEach(btn => {
        btn.addEventListener('click', function() {
            const index = parseInt(this.getAttribute('data-index'));
            deleteChannel(index);
        });
    });
}

// Add channel button handler
document.querySelector('#addChannel').addEventListener('click', function() {
    editingChannelIndex = -1;
    showChannelModal();
});

// Show channel modal
function showChannelModal(channel = null) {
    const modal = document.querySelector('#channelModal');
    
    if (channel) {
        // Edit mode
        document.querySelector('#modalChannelName').value = channel.Name || '';
        document.querySelector('#modalChannelNumber').value = channel.ChannelNumber || 1000;
        document.querySelector('#modalChannelType').value = channel.Type || 'Custom';
        document.querySelector('#modalContentFilters').value = (channel.ContentFilters || []).join(', ');
        document.querySelector('#modalCommercialInterval').value = channel.CommercialIntervalSeconds || 900;
        document.querySelector('#modalShuffleMode').checked = channel.ShuffleMode === true;
        document.querySelector('#modalRespectEpisodeOrder').checked = channel.RespectEpisodeOrder !== false;
        document.querySelector('#modalEnablePreRolls').checked = channel.EnablePreRolls !== false;
        document.querySelector('#modalEnabled').checked = channel.Enabled !== false;
    } else {
        // Add mode
        document.querySelector('#modalChannelName').value = '';
        document.querySelector('#modalChannelNumber').value = (currentConfig?.BaseChannelNumber || 1000) + (currentConfig?.Channels?.length || 0);
        document.querySelector('#modalChannelType').value = 'Genre';
        document.querySelector('#modalContentFilters').value = '';
        document.querySelector('#modalCommercialInterval').value = currentConfig?.DefaultCommercialInterval || 900;
        document.querySelector('#modalShuffleMode').checked = false;
        document.querySelector('#modalRespectEpisodeOrder').checked = true;
        document.querySelector('#modalEnablePreRolls').checked = true;
        document.querySelector('#modalEnabled').checked = true;
    }
    
    modal.classList.add('active');
}

// Hide channel modal
function hideChannelModal() {
    document.querySelector('#channelModal').classList.remove('active');
}

// Cancel button
document.querySelector('#cancelChannelBtn').addEventListener('click', hideChannelModal);

// Close modal on background click
document.querySelector('#channelModal').addEventListener('click', function(e) {
    if (e.target === this) {
        hideChannelModal();
    }
});

// Channel form submit
document.querySelector('#channelForm').addEventListener('submit', async function(e) {
    e.preventDefault();
    Dashboard.showLoadingMsg();
    
    try {
        const config = await ApiClient.getPluginConfiguration(pluginId);
        
        const filters = document.querySelector('#modalContentFilters').value
            .split(',')
            .map(f => f.trim())
            .filter(f => f.length > 0);
        
        const channelData = {
            Id: editingChannelIndex >= 0 ? config.Channels[editingChannelIndex].Id : generateGuid(),
            Name: document.querySelector('#modalChannelName').value,
            ChannelNumber: parseInt(document.querySelector('#modalChannelNumber').value),
            Type: document.querySelector('#modalChannelType').value,
            ContentFilters: filters,
            ShuffleMode: document.querySelector('#modalShuffleMode').checked,
            RespectEpisodeOrder: document.querySelector('#modalRespectEpisodeOrder').checked,
            CommercialIntervalSeconds: parseInt(document.querySelector('#modalCommercialInterval').value),
            EnablePreRolls: document.querySelector('#modalEnablePreRolls').checked,
            LogoPath: '',
            Enabled: document.querySelector('#modalEnabled').checked
        };
        
        if (editingChannelIndex >= 0) {
            // Update existing channel
            config.Channels[editingChannelIndex] = channelData;
        } else {
            // Add new channel
            if (!config.Channels) {
                config.Channels = [];
            }
            config.Channels.push(channelData);
        }
        
        await ApiClient.updatePluginConfiguration(pluginId, config);
        currentConfig = config;
        renderChannelList(config.Channels);
        hideChannelModal();
        
        Dashboard.alert('Channel saved successfully!');
        Dashboard.hideLoadingMsg();
    } catch (err) {
        console.error('Error saving channel:', err);
        Dashboard.alert('Error saving channel: ' + err.message);
        Dashboard.hideLoadingMsg();
    }
});

// Edit channel
async function editChannel(index) {
    try {
        const config = await ApiClient.getPluginConfiguration(pluginId);
        editingChannelIndex = index;
        showChannelModal(config.Channels[index]);
    } catch (err) {
        console.error('Error loading channel:', err);
        Dashboard.alert('Error loading channel: ' + err.message);
    }
}

// Delete channel
async function deleteChannel(index) {
    if (!confirm('Are you sure you want to delete this channel?')) {
        return;
    }
    
    Dashboard.showLoadingMsg();
    
    try {
        const config = await ApiClient.getPluginConfiguration(pluginId);
        config.Channels.splice(index, 1);
        
        await ApiClient.updatePluginConfiguration(pluginId, config);
        currentConfig = config;
        renderChannelList(config.Channels);
        
        Dashboard.alert('Channel deleted successfully!');
        Dashboard.hideLoadingMsg();
    } catch (err) {
        console.error('Error deleting channel:', err);
        Dashboard.alert('Error deleting channel: ' + err.message);
        Dashboard.hideLoadingMsg();
    }
}

// Generate auto channels
document.querySelector('#generateAutoChannels').addEventListener('click', async function() {
    if (!confirm('This will regenerate all auto-generated channels based on your current settings. Continue?')) {
        return;
    }
    
    Dashboard.showLoadingMsg();
    
    try {
        const response = await fetch(ApiClient.getUrl('/api/virtualchannels/auto-generate'), {
            method: 'POST',
            headers: {
                'X-Emby-Token': ApiClient.accessToken()
            }
        });
        
        if (!response.ok) {
            throw new Error('Failed to generate auto channels');
        }
        
        const config = await ApiClient.getPluginConfiguration(pluginId);
        currentConfig = config;
        renderChannelList(config.Channels);
        
        Dashboard.alert('Auto channels generated successfully!');
        Dashboard.hideLoadingMsg();
    } catch (err) {
        console.error('Error generating auto channels:', err);
        Dashboard.alert('Error generating auto channels: ' + err.message);
        Dashboard.hideLoadingMsg();
    }
});

// Refresh channels
document.querySelector('#refreshChannels').addEventListener('click', async function() {
    Dashboard.showLoadingMsg();
    
    try {
        const response = await fetch(ApiClient.getUrl('/api/virtualchannels/refresh'), {
            method: 'POST',
            headers: {
                'X-Emby-Token': ApiClient.accessToken()
            }
        });
        
        if (!response.ok) {
            throw new Error('Failed to refresh channels');
        }
        
        Dashboard.alert('All channels refreshed successfully!');
        Dashboard.hideLoadingMsg();
    } catch (err) {
        console.error('Error refreshing channels:', err);
        Dashboard.alert('Error refreshing channels: ' + err.message);
        Dashboard.hideLoadingMsg();
    }
});

// Utility functions
function generateGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        var r = Math.random() * 16 | 0;
        var v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, m => map[m]);
}

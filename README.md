# Jellyfin Virtual Channels Plugin

Create virtual cable TV channels from your Jellyfin media library with commercial insertion, custom scheduling, and automatic channel generation!

## Features

- **Virtual Cable Channels**: Create live TV channels that play content from your existing Jellyfin library
- **Commercial Insertion**: Automatically insert commercials at configurable intervals (pre-roll, mid-roll support)
- **Automatic Channel Generation**: Auto-generate channels based on:
  - Genres (Action, Comedy, Drama, etc.)
  - Release years/decades (80s Movies, 90s Movies, etc.)
  - Custom tags and collections
- **Episode Ordering**: Respect episode order for TV series or shuffle content
- **Time-Based Programming**: Full EPG (Electronic Program Guide) support with XMLTV format
- **Live TV Integration**: Seamlessly integrates with Jellyfin's native Live TV feature
- **Hardware Acceleration**: Optional FFmpeg hardware acceleration support
- **HLS Streaming**: Modern HTTP Live Streaming for reliable playback

## Requirements

- Jellyfin Server 10.10.7 or later
- .NET 8.0 Runtime
- FFmpeg (for transcoding)
- Optional: Commercial/bumper videos for ad insertion

## Installation

### Method 1: Plugin Catalog (Recommended)

1. Open Jellyfin Dashboard
2. Navigate to **Plugins** → **Repositories**
3. Add custom repository: 
   ```
   https://raw.githubusercontent.com/yourusername/jellyfin-plugin-virtualchannels/main/manifest.json
   ```
4. Go to **Catalog** tab
5. Find and install "Virtual Channels" plugin
6. Restart Jellyfin server

### Method 2: Manual Installation

1. Download the latest release from [Releases](https://github.com/yourusername/jellyfin-plugin-virtualchannels/releases)
2. Extract the ZIP file
3. Copy the contents to your Jellyfin plugins directory:
   - **Linux**: `/var/lib/jellyfin/plugins/VirtualChannels/`
   - **Windows**: `%AppData%\Jellyfin\plugins\VirtualChannels\`
   - **Docker**: `/config/plugins/VirtualChannels/`
4. Restart Jellyfin
5. Navigate to **Dashboard** → **Plugins** → **Virtual Channels** to configure

## Quick Start Guide

### 1. Basic Setup

1. Go to **Dashboard** → **Plugins** → **Virtual Channels**
2. Set your commercial folder path (optional): `/path/to/commercials`
3. Configure default commercial interval (default: 900 seconds / 15 minutes)
4. Set base channel number (default: 1000)
5. Click **Save Configuration**

### 2. Create Your First Channel

#### Option A: Manual Channel Creation
1. Click **Add Custom Channel**
2. Enter channel name (e.g., "Action Movies")
3. Choose channel number (e.g., 1001)
4. Select channel type:
   - **Custom**: Use tags or manual selection
   - **Genre**: Auto-populate from a genre
   - **Year**: Movies from a specific year/decade
   - **Series**: Episodes from a specific TV series
5. Configure options:
   - Shuffle mode (randomize content)
   - Episode order (for TV series)
   - Commercial interval
6. Click **Save**

#### Option B: Automatic Channel Generation
1. Enable **Automatic Channel Generation**
2. Check **Auto-Generate Genre-Based Channels**
3. Check **Auto-Generate Year/Decade-Based Channels** (optional)
4. Click **Generate Auto Channels Now**
5. The plugin will scan your library and create channels automatically!

### 3. Access Your Channels

Your virtual channels are now available in Jellyfin's Live TV section:
1. Go to **Live TV** in Jellyfin
2. Browse your virtual channels (starting from channel 1000 by default)
3. Click to watch!

## Configuration Options

### Global Settings

| Setting | Description | Default |
|---------|-------------|---------|
| Commercial Folder Path | Path to folder containing commercial videos | (empty) |
| Default Commercial Interval | Seconds between commercial breaks | 900 (15 min) |
| Base Channel Number | Starting channel number | 1000 |
| Streaming Port | Port for HLS streaming | 8097 |
| EPG Days Ahead | Days of EPG data to generate | 3 |
| Hardware Acceleration | Enable FFmpeg hardware acceleration | Enabled |
| Transcoding Preset | FFmpeg preset (ultrafast to medium) | veryfast |

### Channel Options

| Option | Description |
|--------|-------------|
| Channel Name | Display name for the channel |
| Channel Number | Unique channel number |
| Type | Custom, Genre, Year, or Series |
| Content Filters | Genres, years, tags, or series IDs |
| Shuffle Mode | Randomize content playback |
| Respect Episode Order | Play episodes in order (for series) |
| Commercial Interval | Override global commercial interval |
| Enable Pre-rolls | Show commercials before content |
| Logo Path | URL or path to channel logo |
| Enabled | Enable/disable the channel |

## Advanced Features

### EPG/XMLTV Support

Access EPG data for external applications:
```
http://your-jellyfin-server:8096/api/virtualchannels/epg/xmltv
```

### M3U Playlists

Get M3U playlists for external players:
- Single channel: `http://your-server:8096/api/virtualchannels/[channel-number]/playlist.m3u`
- All channels: `http://your-server:8096/api/virtualchannels/playlist.m3u`

### API Endpoints

- `GET /api/virtualchannels/epg/xmltv` - XMLTV EPG data
- `GET /api/virtualchannels/playlist.m3u` - Master M3U playlist
- `GET /api/virtualchannels/{channelNumber}/playlist.m3u` - Single channel M3U
- `POST /api/virtualchannels/refresh` - Refresh channel queues
- `POST /api/virtualchannels/auto-generate` - Generate auto channels
- `GET /api/virtualchannels/stats` - Channel statistics
- `GET /api/virtualchannels/genres` - Available genres

## Use Cases

### Family Movie Night Channel
- Create a channel with family-friendly movies
- Enable shuffle mode for variety
- Add fun commercial bumpers between movies

### Saturday Morning Cartoons
- Create a channel with animated series
- Respect episode order for story continuity
- Run 24/7 for nostalgic viewing

### Decade Throwback Channels
- Auto-generate channels for 80s, 90s, 2000s content
- Perfect for themed viewing experiences

### Genre-Specific Channels
- Horror channel for Halloween
- Romance channel for date nights
- Action channel for weekend entertainment

## Troubleshooting

### Channels Not Appearing
1. Verify Jellyfin has been restarted after plugin installation
2. Check that channels are enabled in the configuration
3. Ensure content filters match library items

### Playback Issues
1. Verify FFmpeg is installed and accessible
2. Check transcoding logs in Jellyfin dashboard
3. Try disabling hardware acceleration
4. Adjust transcoding preset to "faster" or "fast"

### No Commercials Playing
1. Verify commercial folder path is correct
2. Ensure commercial videos are in a supported format
3. Check that commercials have valid runtime metadata

### Performance Issues
1. Enable hardware acceleration if available
2. Use "veryfast" or "ultrafast" transcoding preset
3. Reduce number of concurrent streaming channels
4. Consider pre-transcoding popular content

## Development

### Building from Source

```bash
git clone https://github.com/yourusername/jellyfin-plugin-virtualchannels.git
cd jellyfin-plugin-virtualchannels
dotnet build -c Release
```

### Testing

```bash
dotnet test
```

### Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## Roadmap

- [ ] Web UI for advanced channel management
- [ ] Import/export channel configurations
- [ ] Time-of-day scheduling (different content at different times)
- [ ] Multi-audio track support
- [ ] Subtitle support
- [ ] Channel groups/categories
- [ ] DVR functionality
- [ ] Remote commercial services integration

## Credits

Created for the Jellyfin community. Special thanks to:
- Jellyfin development team
- ErsatzTV and Tunarr projects for inspiration
- All contributors and testers

## License

This project is licensed under the GPL-3.0 License - see the [LICENSE](LICENSE) file for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/jellyfin-plugin-virtualchannels/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/jellyfin-plugin-virtualchannels/discussions)
- **Jellyfin Forum**: [Jellyfin Community](https://forum.jellyfin.org/)

## Disclaimer

This plugin is not officially supported by the Jellyfin project. Use at your own risk. Always backup your Jellyfin configuration before installing plugins.

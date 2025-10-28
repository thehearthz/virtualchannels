# Quick Start Guide

Get your virtual channels up and running in 5 minutes!

## Step 1: Install the Plugin (2 minutes)

### Manual Installation (Fastest)

1. Download the latest release ZIP from [Releases](https://github.com/yourusername/jellyfin-plugin-virtualchannels/releases)
2. Extract to your plugins directory:
   - **Linux**: `/var/lib/jellyfin/plugins/VirtualChannels/`
   - **Windows**: `%AppData%\Jellyfin\plugins\VirtualChannels\`
   - **Docker**: `/config/plugins/VirtualChannels/`
3. Restart Jellyfin

## Step 2: Basic Configuration (1 minute)

1. Open Jellyfin Dashboard
2. Go to **Plugins** → **Virtual Channels**
3. Set these minimum settings:
   - **Base Channel Number**: `1000` (or your preferred starting number)
   - **Streaming Port**: `8097` (default is fine)
4. Click **Save Configuration**

## Step 3: Create Channels (1 minute)

### Option A: Auto-Generate (Easiest!)

1. In the plugin configuration:
   - ✓ Enable **Automatic Channel Generation**
   - ✓ Enable **Auto-Generate Genre-Based Channels**
2. Click **Generate Auto Channels Now**
3. Done! Channels will be created automatically for each genre in your library

### Option B: Create One Manual Channel

1. Click **Add Custom Channel**
2. Enter details:
   ```
   Name: My First Channel
   Channel Number: 1001
   Type: Genre
   ```
3. When prompted for filters, enter a genre like: `Action` or `Comedy`
4. Click **Save**

## Step 4: Restart Jellyfin (30 seconds)

Restart Jellyfin to activate the channels:

**Linux/systemd:**
```bash
sudo systemctl restart jellyfin
```

**Docker:**
```bash
docker restart jellyfin
```

**Windows:**
- Restart the Jellyfin service from Services

## Step 5: Watch! (30 seconds)

1. Open Jellyfin
2. Go to **Live TV**
3. Browse your new virtual channels
4. Click one and enjoy!

---

## What's Next?

### Add Commercials (Optional)

1. Create a folder: `/media/commercials`
2. Add short video files (30-60 seconds recommended)
3. In plugin config, set **Commercial Folder Path**: `/media/commercials`
4. Set **Commercial Interval**: `900` (15 minutes)
5. Save and restart

### Customize Channels

- Edit channels in the plugin configuration
- Change shuffle mode on/off
- Adjust commercial intervals
- Enable/disable channels

### Explore Advanced Features

Read the full documentation:
- [Setup Guide](docs/SETUP.md) - Detailed installation
- [Examples](docs/EXAMPLES.md) - Channel configuration ideas
- [README](README.md) - Complete feature list

---

## Troubleshooting Quick Fixes

**Channels not appearing?**
- Restart Jellyfin completely
- Check channels are enabled in config
- Verify FFmpeg is installed: `ffmpeg -version`

**No content in channels?**
- Check content filters match your library genres
- Verify library has content with metadata
- Try auto-generation to test

**Playback issues?**
- Verify FFmpeg is working
- Disable hardware acceleration in config
- Check Jellyfin logs for errors

---

## Get Help

- **Documentation**: [docs/](docs/)
- **Issues**: [GitHub Issues](https://github.com/yourusername/jellyfin-plugin-virtualchannels/issues)
- **Community**: [Jellyfin Forum](https://forum.jellyfin.org/)

## That's It!

You're now running your own virtual cable TV channels! 📺

Enjoy your personalized TV experience!

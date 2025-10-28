# Setup Guide

This guide will help you set up the Virtual Channels plugin for Jellyfin.

## Prerequisites

### 1. FFmpeg Installation

The plugin requires FFmpeg for transcoding streams.

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install ffmpeg
```

**CentOS/RHEL:**
```bash
sudo yum install epel-release
sudo yum install ffmpeg
```

**Docker:**
FFmpeg is typically included in Jellyfin Docker images. Verify with:
```bash
docker exec jellyfin ffmpeg -version
```

### 2. Verify Jellyfin Version

Ensure you're running Jellyfin 10.10.7 or later:
1. Go to Dashboard → About
2. Check the version number

## Installation Steps

### Step 1: Install the Plugin

Choose one of the installation methods from the main README.

### Step 2: Initial Configuration

1. Navigate to **Dashboard** → **Plugins** → **Virtual Channels**
2. Configure the following settings:

#### Essential Settings:
- **Base Channel Number**: Starting channel number (default: 1000)
- **Streaming Port**: Port for HLS streaming (default: 8097)

#### Optional Settings:
- **Commercial Folder Path**: Path to your commercial/bumper videos
  - Example: `/media/commercials` or `C:\Media\Commercials`
- **Default Commercial Interval**: How often to insert commercials (in seconds)
  - 900 seconds = 15 minutes
  - 1800 seconds = 30 minutes

### Step 3: Prepare Commercial Content (Optional)

If you want to use commercials:

1. Create a folder for commercial videos
2. Add short video files (30-60 seconds recommended)
3. Supported formats: MP4, MKV, AVI, MOV
4. Set the folder path in plugin configuration

Example structure:
```
/media/commercials/
  ├── commercial1.mp4
  ├── commercial2.mp4
  ├── bumper1.mp4
  └── bumper2.mp4
```

### Step 4: Create Your First Channel

#### Option A: Manual Channel

1. Click **Add Custom Channel**
2. Fill in the details:
   ```
   Name: Action Movies
   Channel Number: 1001
   Type: Genre
   ```
3. Set **Content Filters**:
   - For Genre channels: Enter genre name (e.g., "Action")
   - For Year channels: Enter year (e.g., "1990")
   - For Series channels: Enter series GUID
4. Configure playback:
   - **Shuffle Mode**: Randomize content order
   - **Respect Episode Order**: For TV series
   - **Commercial Interval**: Override global setting
5. Click **Save**

#### Option B: Auto-Generate Channels

1. Enable **Automatic Channel Generation**
2. Select generation options:
   - **Auto-Generate Genre-Based Channels**: Creates a channel for each genre
   - **Auto-Generate Year/Decade-Based Channels**: Creates decade-based channels
3. Click **Generate Auto Channels Now**
4. Wait for the process to complete

### Step 5: Restart Jellyfin

After configuration, restart Jellyfin:

**Systemd (Linux):**
```bash
sudo systemctl restart jellyfin
```

**Docker:**
```bash
docker restart jellyfin
```

**Windows Service:**
```powershell
Restart-Service JellyfinServer
```

### Step 6: Access Live TV

1. Go to **Live TV** in Jellyfin
2. You should see your virtual channels
3. Click on a channel to start watching!

## Troubleshooting Setup

### Plugin Not Appearing

**Problem**: Plugin doesn't show in plugin list after installation

**Solutions**:
1. Verify plugin files are in correct directory
2. Check file permissions (should be readable by Jellyfin user)
3. Check Jellyfin logs for errors: `/var/log/jellyfin/`
4. Restart Jellyfin completely

### Channels Not Showing in Live TV

**Problem**: Virtual channels don't appear in Live TV section

**Solutions**:
1. Verify channels are enabled in configuration
2. Check that channel numbers don't conflict with existing channels
3. Restart Jellyfin after creating/modifying channels
4. Check that content filters match library items

### FFmpeg Not Found

**Problem**: Error messages about FFmpeg not being available

**Solutions**:
1. Verify FFmpeg installation: `ffmpeg -version`
2. Check Jellyfin can access FFmpeg:
   - Dashboard → Playback → FFmpeg path
3. Install FFmpeg if missing (see Prerequisites)

### No Content in Channels

**Problem**: Channels exist but show no content

**Solutions**:
1. Verify content filters match your library:
   - For Genre channels: Check genre names match exactly
   - For Year channels: Ensure movies have production year metadata
2. Check library scan is complete
3. Verify content has valid metadata
4. Check plugin logs for queue fill errors

### Permission Denied Errors

**Problem**: Cannot access commercial folder or media files

**Solutions**:
1. Set correct permissions on commercial folder:
   ```bash
   sudo chown -R jellyfin:jellyfin /media/commercials
   sudo chmod -R 755 /media/commercials
   ```
2. Ensure Jellyfin user can read media files
3. Check SELinux/AppArmor policies if applicable

## Advanced Setup

### Hardware Acceleration

For better performance, enable hardware acceleration:

1. Install appropriate drivers:
   - **Intel**: Install intel-media-driver
   - **NVIDIA**: Install CUDA/NVENC drivers
   - **AMD**: Install VA-API drivers

2. In plugin configuration:
   - Enable **Hardware Acceleration**
   - Jellyfin will automatically detect available hardware

3. Monitor CPU usage - should be significantly lower

### Custom Port Configuration

If port 8097 is already in use:

1. Choose a different port (e.g., 8098)
2. Update **Streaming Port** in configuration
3. If using a firewall, open the new port:
   ```bash
   sudo ufw allow 8098/tcp
   ```

### Multiple Commercial Folders

To use different commercials for different channels:

1. Create separate folders per channel type:
   ```
   /media/commercials/
     ├── action/
     ├── comedy/
     └── drama/
   ```

2. In channel configuration, specify the subfolder path

### External Access

To access channels from outside your network:

1. Set up reverse proxy (nginx, Apache, Caddy)
2. Configure SSL/TLS certificates
3. Update streaming URLs in configuration
4. Forward necessary ports through router

## Next Steps

- Read [EXAMPLES.md](EXAMPLES.md) for channel configuration examples
- Check the main [README.md](../README.md) for advanced features
- Join the community forum for tips and support

## Getting Help

If you encounter issues:

1. Check Jellyfin logs: `/var/log/jellyfin/`
2. Enable debug logging: Dashboard → Logs → Set log level to Debug
3. Search existing issues: GitHub Issues
4. Create a new issue with:
   - Plugin version
   - Jellyfin version
   - Operating system
   - Relevant log excerpts
   - Configuration (sanitize sensitive info)

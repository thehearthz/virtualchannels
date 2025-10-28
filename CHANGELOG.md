# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned Features
- Web UI for advanced channel management
- Import/export channel configurations
- Time-of-day scheduling
- Multi-audio track support
- Subtitle support
- Channel groups/categories
- DVR functionality

## [1.0.0] - 2025-10-28

### Added
- Initial release
- Virtual channel creation and management
- Commercial insertion with configurable intervals
- Pre-roll commercial support
- Genre-based automatic channel generation
- Year/decade-based automatic channel generation
- Custom channel creation with filters
- Series-based channels with episode order support
- Shuffle mode for random content playback
- Time-based programming with full EPG support
- XMLTV format EPG generation
- M3U playlist generation (single channel and master)
- HLS streaming with FFmpeg transcoding
- Hardware acceleration support
- Live TV integration
- REST API for channel management
- Configuration web UI
- Channel state management
- Automatic content queue management
- Background service for maintenance tasks
- Comprehensive documentation and examples

### Technical Details
- Built for Jellyfin 10.10.7
- Target Framework: .NET 8.0
- FFmpeg integration for transcoding
- HLS adaptive streaming
- MPEG-TS segment generation

### Documentation
- README.md with comprehensive overview
- SETUP.md with detailed installation guide
- EXAMPLES.md with channel configuration examples
- CONTRIBUTING.md with contribution guidelines
- API documentation in source code

### Known Issues
- None currently identified

## Version History

### Legend
- `Added` - New features
- `Changed` - Changes in existing functionality
- `Deprecated` - Soon-to-be removed features
- `Removed` - Removed features
- `Fixed` - Bug fixes
- `Security` - Security vulnerability fixes

---

## Future Roadmap

### v1.1.0 (Planned)
- Enhanced web UI with channel preview
- Import/export configurations
- Channel templates
- Better commercial scheduling options
- Performance improvements

### v1.2.0 (Planned)
- Time-of-day scheduling
- Weekend/weekday programming
- Special event scheduling
- Multi-audio track support

### v2.0.0 (Planned)
- DVR functionality
- Recording management
- Pause/resume live TV
- Advanced EPG features

---

## Support

For issues, feature requests, or questions:
- GitHub Issues: https://github.com/yourusername/jellyfin-plugin-virtualchannels/issues
- GitHub Discussions: https://github.com/yourusername/jellyfin-plugin-virtualchannels/discussions
- Jellyfin Forum: https://forum.jellyfin.org/

## Credits

- Jellyfin Team - For the excellent media server platform
- ErsatzTV & Tunarr Projects - For inspiration
- Contributors - See CONTRIBUTORS.md
- Community - For feedback and support

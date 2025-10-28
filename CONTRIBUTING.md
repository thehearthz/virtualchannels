# Contributing to Jellyfin Virtual Channels Plugin

Thank you for your interest in contributing! This document provides guidelines for contributing to the project.

## Code of Conduct

- Be respectful and inclusive
- Welcome newcomers and help them learn
- Focus on constructive feedback
- Respect differing viewpoints and experiences

## How to Contribute

### Reporting Bugs

Before creating a bug report:
1. Check existing issues to avoid duplicates
2. Gather relevant information (version, OS, logs)
3. Create a minimal reproducible example if possible

**Bug Report Template:**
```markdown
**Description**: Clear description of the bug

**To Reproduce**:
1. Step one
2. Step two
3. ...

**Expected Behavior**: What should happen

**Actual Behavior**: What actually happens

**Environment**:
- Plugin Version: [e.g., 1.0.0]
- Jellyfin Version: [e.g., 10.10.7]
- OS: [e.g., Ubuntu 22.04]
- Browser: [e.g., Chrome 120]

**Logs**:
```
Paste relevant log excerpts here
```

**Additional Context**: Any other relevant information
```

### Suggesting Features

Feature requests are welcome! Please:
1. Check if the feature already exists or is planned
2. Explain the use case and benefit
3. Be open to discussion and alternatives

**Feature Request Template:**
```markdown
**Feature Description**: Clear description of the feature

**Use Case**: Why this feature would be useful

**Proposed Implementation**: (Optional) Ideas for implementation

**Alternatives Considered**: Other solutions you've thought about
```

### Contributing Code

#### Getting Started

1. **Fork the repository**
   ```bash
   # Click "Fork" on GitHub, then:
   git clone https://github.com/YOUR-USERNAME/jellyfin-plugin-virtualchannels.git
   cd jellyfin-plugin-virtualchannels
   ```

2. **Create a branch**
   ```bash
   git checkout -b feature/my-new-feature
   # or
   git checkout -b fix/bug-description
   ```

3. **Set up development environment**
   ```bash
   dotnet restore
   dotnet build
   ```

#### Development Guidelines

**Code Style:**
- Follow C# naming conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and reasonably sized

**Example:**
```csharp
/// <summary>
/// Gets the next item to play for a channel.
/// </summary>
/// <param name="channelId">The unique channel identifier.</param>
/// <param name="config">The channel configuration.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The next item, or null if queue is empty.</returns>
public async Task<BaseItem?> GetNextItem(
    string channelId,
    VirtualChannelConfig config,
    CancellationToken cancellationToken)
{
    // Implementation
}
```

**Testing:**
- Write unit tests for new features
- Ensure existing tests pass
- Test manually with different configurations

**Logging:**
- Use appropriate log levels:
  - `LogDebug`: Detailed diagnostic information
  - `LogInformation`: General informational messages
  - `LogWarning`: Warning messages for unexpected but handled situations
  - `LogError`: Error messages for failures

```csharp
_logger.LogInformation("Channel {ChannelId} started streaming", channelId);
_logger.LogWarning("Queue for channel {ChannelId} is running low", channelId);
_logger.LogError(ex, "Failed to transcode content for channel {ChannelId}", channelId);
```

#### Commit Messages

Write clear, descriptive commit messages:

**Format:**
```
<type>: <short description>

<optional detailed description>

<optional footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples:**
```
feat: add support for custom commercial intervals per channel

Allows users to override the global commercial interval setting
for individual channels, providing more flexibility in channel
configuration.

Closes #123
```

```
fix: resolve FFmpeg process not terminating on stream stop

The FFmpeg process was not being properly terminated when a
stream was stopped, causing resource leaks. Now explicitly
kills the process and waits for termination.

Fixes #456
```

#### Pull Request Process

1. **Update documentation** if needed
2. **Add tests** for new functionality
3. **Update CHANGELOG.md** (if applicable)
4. **Ensure all tests pass**:
   ```bash
   dotnet test
   ```
5. **Create pull request** with clear description

**Pull Request Template:**
```markdown
## Description
Brief description of changes

## Related Issue
Closes #[issue number]

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
Describe how you tested these changes

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Comments added for complex code
- [ ] Documentation updated
- [ ] Tests added/updated
- [ ] All tests pass
- [ ] No new warnings introduced
```

### Contributing Documentation

Documentation improvements are always welcome:
- Fix typos and grammar
- Clarify confusing sections
- Add examples and use cases
- Improve setup instructions

### Contributing Examples

Share your channel configurations:
1. Add to `docs/EXAMPLES.md`
2. Include configuration details
3. Explain the use case
4. Add any special tips or tricks

## Development Setup

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022, VS Code, or Rider
- Git
- FFmpeg (for testing transcoding)

### Building
```bash
dotnet restore
dotnet build -c Release
```

### Running Tests
```bash
dotnet test
```

### Local Testing

1. Build the plugin
2. Copy DLL to Jellyfin plugins directory:
   ```bash
   # Linux
   cp bin/Release/net8.0/Jellyfin.Plugin.VirtualChannels.dll \
      /var/lib/jellyfin/plugins/VirtualChannels/
   
   # Windows
   copy bin\Release\net8.0\Jellyfin.Plugin.VirtualChannels.dll ^
        %AppData%\Jellyfin\plugins\VirtualChannels\
   ```
3. Restart Jellyfin
4. Test your changes

## Project Structure

```
Jellyfin.Plugin.VirtualChannels/
├── Api/                    # REST API controllers
├── Configuration/          # Plugin configuration
│   └── Web/               # Web UI (HTML/JS)
├── LiveTV/                # Live TV provider implementation
├── Models/                # Data models
├── Services/              # Core services
│   ├── ChannelScheduler.cs
│   ├── StreamGenerator.cs
│   ├── CommercialInserter.cs
│   ├── EpgGenerator.cs
│   └── ...
└── Plugin.cs              # Main plugin class
```

## Getting Help

- **Documentation**: Check docs/ folder
- **GitHub Issues**: For bug reports and feature requests
- **GitHub Discussions**: For questions and community chat
- **Jellyfin Forum**: For general Jellyfin help

## Recognition

Contributors will be:
- Listed in CONTRIBUTORS.md (if substantial contributions)
- Mentioned in release notes
- Credited in commit history

Thank you for contributing to make this plugin better!

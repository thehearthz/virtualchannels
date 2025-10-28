# Channel Configuration Examples

This document provides practical examples of virtual channel configurations for various use cases.

## Basic Examples

### Example 1: Action Movies Channel

A 24/7 action movie channel with commercials every 20 minutes.

**Configuration:**
```
Name: Action Theater
Channel Number: 1001
Type: Genre
Content Filters: ["Action"]
Shuffle Mode: Yes
Respect Episode Order: No
Commercial Interval: 1200 seconds (20 minutes)
Enable Pre-rolls: Yes
```

**Best For**: Movie marathons, background entertainment

---

### Example 2: Comedy TV Series Channel

A channel playing comedy TV shows in episode order.

**Configuration:**
```
Name: Comedy Central
Channel Number: 1002
Type: Genre
Content Filters: ["Comedy"]
Shuffle Mode: No
Respect Episode Order: Yes
Commercial Interval: 1800 seconds (30 minutes)
Enable Pre-rolls: Yes
```

**Best For**: Binge-watching sitcoms, evening entertainment

---

### Example 3: 90s Nostalgia Channel

A decade-specific channel featuring 90s content.

**Configuration:**
```
Name: 90s Flashback
Channel Number: 1003
Type: Year
Content Filters: ["1990"] (represents 1990-1999 decade)
Shuffle Mode: Yes
Respect Episode Order: No
Commercial Interval: 900 seconds (15 minutes)
Enable Pre-rolls: Yes
```

**Best For**: Themed viewing, nostalgia trips

---

### Example 4: Kids' Cartoons Channel

A safe, family-friendly animation channel.

**Configuration:**
```
Name: Kids Cartoons
Channel Number: 1004
Type: Genre
Content Filters: ["Animation", "Family"]
Shuffle Mode: Yes
Respect Episode Order: No
Commercial Interval: 600 seconds (10 minutes)
Enable Pre-rolls: Yes
```

**Best For**: Children's entertainment, Saturday mornings

---

## Advanced Examples

### Example 5: Specific TV Series Marathon

A channel dedicated to a single TV series.

**Configuration:**
```
Name: The Office Marathon
Channel Number: 1005
Type: Series
Content Filters: ["<series-guid-here>"]
Shuffle Mode: No
Respect Episode Order: Yes
Commercial Interval: 1200 seconds (20 minutes)
Enable Pre-rolls: No
```

**How to get Series GUID:**
1. Browse to the series in Jellyfin web interface
2. Look at the URL: `jellyfin.example.com/web/index.html#!/details?id=<GUID>`
3. Copy the GUID and use it in Content Filters

**Best For**: Complete series watching, fan channels

---

### Example 6: Horror Movie Night

A horror-themed channel for October or late-night viewing.

**Configuration:**
```
Name: Midnight Horror
Channel Number: 1006
Type: Genre
Content Filters: ["Horror", "Thriller"]
Shuffle Mode: Yes
Respect Episode Order: No
Commercial Interval: 1500 seconds (25 minutes)
Enable Pre-rolls: Yes
```

**Tip**: Use spooky bumpers in your commercial folder!

---

### Example 7: Documentary Channel

An educational documentary channel.

**Configuration:**
```
Name: Documentary Network
Channel Number: 1007
Type: Genre
Content Filters: ["Documentary"]
Shuffle Mode: No
Respect Episode Order: Yes
Commercial Interval: 1800 seconds (30 minutes)
Enable Pre-rolls: No
```

**Best For**: Educational content, background learning

---

### Example 8: Classic Film Channel

Old movies channel (pre-1970).

**Configuration:**
```
Name: Classic Cinema
Channel Number: 1008
Type: Custom
Content Filters: ["Classic", "Golden Age"]
Shuffle Mode: No
Respect Episode Order: No
Commercial Interval: 1200 seconds (20 minutes)
Enable Pre-rolls: Yes
```

**Note**: Use tags "Classic" or "Golden Age" on movies in your library

---

## Themed Channel Collections

### Weekend Movie Marathon Setup

Create a set of channels for different moods:

**Friday Night:**
```
Channel 2001: Action Blockbusters (Action genre, shuffle)
Channel 2002: Horror Scares (Horror genre, shuffle)
Channel 2003: Laugh Track (Comedy genre, shuffle)
```

**Saturday Family Time:**
```
Channel 2004: Family Movies (Family genre, shuffle)
Channel 2005: Kids Cartoons (Animation genre, episode order)
Channel 2006: Disney Classics (Tag: Disney, shuffle)
```

**Sunday Chill:**
```
Channel 2007: Drama Channel (Drama genre, shuffle)
Channel 2008: Romance Movies (Romance genre, shuffle)
Channel 2009: Documentaries (Documentary genre, episode order)
```

---

### Season-Specific Channels

**Holiday Season (November-December):**
```
Channel 3001: Holiday Movies
Type: Custom
Content Filters: ["Holiday", "Christmas"]
Shuffle: Yes
```

**Halloween (October):**
```
Channel 3002: Spooky Season
Type: Genre
Content Filters: ["Horror"]
Shuffle: Yes
Commercial Interval: 666 seconds (for fun!)
```

**Summer Blockbusters:**
```
Channel 3003: Summer Action
Type: Genre
Content Filters: ["Action", "Adventure"]
Shuffle: Yes
```

---

## Auto-Generation Examples

### Automatic Genre Channels

Enable auto-generation and the plugin will create channels like:

```
Channel 1100: Action Channel
Channel 1101: Comedy Channel
Channel 1102: Drama Channel
Channel 1103: Horror Channel
Channel 1104: Romance Channel
Channel 1105: Sci-Fi Channel
... (one for each genre in your library)
```

**Configuration:**
- Enable Automatic Channel Generation: Yes
- Auto-Generate Genre-Based Channels: Yes
- Base Channel Number: 1100

---

### Automatic Decade Channels

For movie libraries organized by era:

```
Channel 1200: 1950s Movies
Channel 1201: 1960s Movies
Channel 1202: 1970s Movies
Channel 1203: 1980s Movies
Channel 1204: 1990s Movies
Channel 1205: 2000s Movies
Channel 1206: 2010s Movies
Channel 1207: 2020s Movies
```

**Configuration:**
- Enable Automatic Channel Generation: Yes
- Auto-Generate Year/Decade-Based Channels: Yes
- Base Channel Number: 1200

---

## Commercial Configuration Examples

### Light Commercial Load

For channels where you want minimal interruption:

```
Commercial Interval: 2400 seconds (40 minutes)
Enable Pre-rolls: No
```

**Good for**: Documentaries, dramas, art films

---

### Heavy Commercial Load (Classic TV Experience)

For authentic broadcast TV feel:

```
Commercial Interval: 600 seconds (10 minutes)
Enable Pre-rolls: Yes
```

**Good for**: Sitcoms, variety shows, kids content

---

### No Commercials

For premium, uninterrupted viewing:

```
Commercial Interval: 999999 seconds (effectively never)
Enable Pre-rolls: No
Commercial Folder Path: (leave empty)
```

**Good for**: Movies, prestige TV series

---

## Custom Tag-Based Channels

### Channel Based on IMDb Ratings

1. Tag high-rated content in Jellyfin:
   - Add tag "Top Rated" to movies with 8+ IMDb rating
   
2. Create channel:
```
Name: Top Picks
Type: Custom
Content Filters: ["Top Rated"]
Shuffle: Yes
```

---

### Curated Collection Channel

1. Create a collection in Jellyfin (e.g., "Best of 2023")
2. Tag all items in collection
3. Create channel:
```
Name: 2023 Highlights
Type: Custom
Content Filters: ["Best of 2023"]
Shuffle: No
```

---

## Tips for Great Channels

### 1. Content Variety
- Ensure at least 20+ items per channel for good variety
- Mix content lengths (short episodes + long movies)

### 2. Commercial Timing
- Match interval to content type:
  - Short episodes (20 min): 10-minute intervals
  - Standard shows (40 min): 15-minute intervals
  - Movies (2 hrs): 20-30 minute intervals

### 3. Episode Ordering
- Enable for: Story-driven series, documentaries
- Disable for: Sitcoms, variety content, movies

### 4. Shuffle Mode
- Enable for: Movie channels, mixed content
- Disable for: Series marathons, chronological content

### 5. Pre-rolls
- Use for: Branding, channel identification
- Skip for: Serious content like documentaries

### 6. Logo/Branding
- Create channel logos for better identification
- Use transparent PNGs (200x200 pixels recommended)
- Store in accessible location, provide path in config

---

## Troubleshooting Common Issues

### Channel Shows No Content

**Problem**: Content filters don't match any library items

**Solutions**:
1. Verify genre names match exactly (case-sensitive)
2. Check that content has metadata
3. Try broader filters first, then narrow down

### Content Repeats Too Often

**Problem**: Not enough items in queue

**Solutions**:
1. Add more items to library matching filters
2. Use broader filters (multiple genres)
3. Enable auto-generation for more variety

### Commercials Don't Fit

**Problem**: Commercials are too long or cause awkward breaks

**Solutions**:
1. Use short commercials (30-60 seconds)
2. Adjust commercial interval to match content rhythm
3. Create multiple commercial types for different channels

---

## Community Examples

Share your best channel configurations on GitHub Discussions!

### Template for Sharing

```markdown
## My Awesome Channel

**Name**: [Channel Name]
**Type**: [Custom/Genre/Year/Series]
**Purpose**: [What makes this channel great]

**Configuration**:
- Channel Number: [####]
- Content Filters: [list]
- Shuffle: [Yes/No]
- Episode Order: [Yes/No]
- Commercial Interval: [####] seconds

**Pro Tips**:
- [Your advice here]
```

---

## Next Steps

- Experiment with different configurations
- Monitor viewer engagement
- Adjust based on feedback
- Share your best setups with the community!

Need more help? Check out:
- [Setup Guide](SETUP.md)
- [Main README](../README.md)
- [GitHub Issues](https://github.com/yourusername/jellyfin-plugin-virtualchannels/issues)

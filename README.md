# PDA Subtitle Library

A developer library for Subnautica modders to add synchronized subtitles to PDA audio logs using standard `.srt` files. Built for BepInEx and Nautilus, this library enables accessibility and localization for custom PDA content.


## Features
- **.srt Subtitle Support:** Use industry-standard SubRip files for subtitles
- **Automatic Timing:** Subtitles display in sync with audio playback
- **Easy Integration:** Simple API for registering subtitles and audio
- **Harmony Patches:** Non-intrusive, works with vanilla and modded content


## Getting Started

### 1. Installation
- Add the compiled `PDASubtitleLib.dll` to your mod project or as a dependency
- Place the DLL in your mod's folder or reference it in your project file

### 2. Prepare Your Assets
- Place your `.srt` subtitle files in a folder (e.g., `Assets/Audio`)
- The only requirement is that the `key` you use when adding PDA log entries matches the `.srt` filename (without extension). Audio files do not need to be named the same or placed in the same folder as the `.srt` files; you can load audio however you wish.

### 3. Register Subtitles in Your Mod
```csharp
// At mod startup (e.g., in your Plugin.Start method):
SubtitlesHandler.Register(Path.Combine(Paths.PluginPath, "YourMod", "Assets", "Audio"));
// This will register all of your .srt files for later lookup
```

### 4. Register PDA Log Entries
```csharp
PDAHandler.AddLogEntry(
    key, // e.g., "log1" (must match .srt filename without extension)
    SubtitlesHandler.GetLanguageKey(key), // IMPORTANT: Always use GetLanguageKey(key) here!
    audioPath // this is just one way of adding audio using PDAHandler.AddLogEntry, there's many more and PDASubtitleLib will work regardless of how you load the audio into the log entry
);
// NOTE: If you do not use SubtitlesHandler.GetLanguageKey(key), the PDA log entry will not show the actual text of the .srt file.
```

### 5. Trigger PDA Log Playback
```csharp
SubtitlesHandler.PDALogAdd(key); // Adds the log and starts audio + subtitle playback
```


## Subtitle File Format
- Standard `.srt` format:
  ```
  1
  00:00:00,099 --> 00:00:03,019
  This is a subtitle synchronization test.
  ```
- Each `.srt` file's name (without extension) is the `key` for that log entry


## API Reference

### `SubtitlesHandler.Register(string folderPath)`
Scans the folder for `.srt` files and registers subtitles for each key.

### `SubtitlesHandler.GetLanguageKey(string key)`
Returns the language key for use with PDA log entries. **Always use this instead of just `key`!**

### `SubtitlesHandler.PDALogAdd(string key)`
Adds a PDA log entry and starts subtitle playback for the given key.


## Requirements
- BepInEx
- Nautilus


## License

See [LICENSE](LICENSE) file for details.
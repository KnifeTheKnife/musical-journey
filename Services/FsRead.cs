using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using musical_journey.Services.Interfaces;

namespace musical_journey.Services;

public class FsRead : IFsRead
{
    private static readonly HashSet<string> MusicExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".flac", ".wav", ".ogg", ".m4a", ".aac", ".wma", ".alac", ".ape", ".opus"
    };

    public List<string> GetMusicFiles(string musicPath)
    {
        if (string.IsNullOrWhiteSpace(musicPath))
        {
            throw new ArgumentException("Music path cannot be null or empty", nameof(musicPath));
        }

        if (!Directory.Exists(musicPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {musicPath}");
        }

        return ScanDirectory(musicPath);
    }

    private List<string> ScanDirectory(string path)
    {
        var musicFiles = new List<string>();

        try
        {
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file);
                if (MusicExtensions.Contains(ext))
                {
                    musicFiles.Add(file);
                }
            }

            var directories = Directory.GetDirectories(path);
            foreach (var dir in directories)
            {
                musicFiles.AddRange(ScanDirectory(dir));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Access denied to: {path} - {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scanning {path}: {ex.Message}");
        }

        return musicFiles;
    }
}

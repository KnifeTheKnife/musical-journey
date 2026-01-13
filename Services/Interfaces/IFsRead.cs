using System.Collections.Generic;

namespace musical_journey.Services.Interfaces;

public interface IFsRead
{
    /// <summary>
    /// Scans a directory and returns all music files found recursively
    /// </summary>
    /// <param name="musicPath">Root directory to scan</param>
    /// <returns>List of full file paths to music files, or empty list if none found</returns>
    List<string> GetMusicFiles(string musicPath);
}
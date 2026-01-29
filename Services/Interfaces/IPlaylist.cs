using System;
using System.Collections.ObjectModel;
using musical_journey.Services.Interfaces;

namespace musical_journey.Services.Interfaces;

/// <summary>
/// Interface for a playlist containing a collection of songs
/// </summary>
public interface IPlaylist
{
    string Id { get; set; }
    string Name { get; set; }
    DateTime CreatedDate { get; set; }
    DateTime ModifiedDate { get; set; }
    ObservableCollection<Song> Songs { get; }

    /// <summary>
    /// Adds a song to the playlist
    /// </summary>
    void AddSong(Song song);
    /// <summary>
    /// Removes a song from the playlist
    /// </summary>
    bool RemoveSong(string songPath);
    /// <summary>
    /// Removes a song at the specified index
    /// </summary>
    bool RemoveSongAt(int index);
    /// <summary>
    /// Gets the number of songs in the playlist
    /// </summary>
    int GetSongCount();
    /// <summary>
    /// Clears all songs from the playlist
    /// </summary>
    void Clear();
}

using System.Collections.Generic;

namespace musical_journey.Services.Interfaces;

/// <summary>
/// Interface for managing playlists
/// </summary>
public interface IPlaylistService
{
    /// <summary>
    /// Creates a new playlist with the specified name
    /// </summary>
    Playlist CreatePlaylist(string name);

    /// <summary>
    /// Deletes a playlist by its ID
    /// </summary>
    bool DeletePlaylist(string playlistId);

    /// <summary>
    /// Gets a playlist by its ID
    /// </summary>
    Playlist? GetPlaylistById(string playlistId);

    /// <summary>
    /// Gets all playlists
    /// </summary>
    List<Playlist> GetAllPlaylists();

    /// <summary>
    /// Renames a playlist
    /// </summary>
    bool RenamePlaylist(string playlistId, string newName);

    /// <summary>
    /// Adds a song to a playlist
    /// </summary>
    bool AddSongToPlaylist(string playlistId, Song song);

    /// <summary>
    /// Removes a song from a playlist
    /// </summary>
    bool RemoveSongFromPlaylist(string playlistId, string songPath);

    /// <summary>
    /// Removes a song at a specific index from a playlist
    /// </summary>
    bool RemoveSongFromPlaylistAt(string playlistId, int index);

    /// <summary>
    /// Clears all songs from a playlist
    /// </summary>
    bool ClearPlaylist(string playlistId);

    /// <summary>
    /// Saves playlists to persistent storage
    /// </summary>
    void SavePlaylists();

    /// <summary>
    /// Loads playlists from persistent storage
    /// </summary>
    void LoadPlaylists();
}

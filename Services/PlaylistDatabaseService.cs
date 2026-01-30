using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using musical_journey.Services.Interfaces;

namespace musical_journey.Services;

/// <summary>
/// Service for managing playlists using SQLite database
/// </summary>
public class PlaylistDatabaseService : IPlaylistService
{
    private const string ConnectionString = "Data Source=cache.db;";
    private readonly Dictionary<string, Playlist> _playlistsCache = new();

    public PlaylistDatabaseService()
    {
        DoTablesExist();
        LoadPlaylistsFromDatabase();
    }

    private void DoTablesExist()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand()
        {
            Connection = connection
        };
        
        // Create Playlists table
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Playlists (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                CreatedDate TEXT NOT NULL,
                ModifiedDate TEXT NOT NULL
            )";
        command.ExecuteNonQuery();

        // Create PlaylistSongs table (junction table)
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS PlaylistSongs (
                PlaylistId TEXT NOT NULL,
                SongTitle TEXT,
                SongArtist TEXT,
                SongAlbum TEXT,
                SongTrackNo TEXT,
                SongGenre TEXT,
                SongDate TEXT,
                SongDiscNo TEXT,
                SongPath TEXT NOT NULL,
                FOREIGN KEY(PlaylistId) REFERENCES Playlists(Id),
                PRIMARY KEY(PlaylistId, SongPath)
            )";
        command.ExecuteNonQuery();
    }

    public Playlist CreatePlaylist(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Playlist name cannot be empty");

        var playlist = new Playlist(name);
        
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand(
            "INSERT INTO Playlists (Id, Name, CreatedDate, ModifiedDate) VALUES (@Id, @Name, @CreatedDate, @ModifiedDate)",
            connection);
        command.Parameters.AddWithValue("@Id", playlist.Id);
        command.Parameters.AddWithValue("@Name", playlist.Name);
        command.Parameters.AddWithValue("@CreatedDate", playlist.CreatedDate.ToString("o"));
        command.Parameters.AddWithValue("@ModifiedDate", playlist.ModifiedDate.ToString("o"));
        command.ExecuteNonQuery();

        _playlistsCache[playlist.Id] = playlist;
        return playlist;
    }

    public bool DeletePlaylist(string playlistId)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand("DELETE FROM Playlists WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", playlistId);
        var result = command.ExecuteNonQuery() > 0;

        if (result)
        {
            _playlistsCache.Remove(playlistId);
        }
        return result;
    }

    public Playlist? GetPlaylistById(string playlistId)
    {
        _playlistsCache.TryGetValue(playlistId, out var playlist);
        return playlist;
    }

    public List<Playlist> GetAllPlaylists()
    {
        return _playlistsCache.Values.OrderByDescending(p => p.ModifiedDate).ToList();
    }

    public bool RenamePlaylist(string playlistId, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return false;

        if (!_playlistsCache.TryGetValue(playlistId, out var playlist))
            return false;

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand(
            "UPDATE Playlists SET Name = @Name, ModifiedDate = @ModifiedDate WHERE Id = @Id",
            connection);
        command.Parameters.AddWithValue("@Name", newName);
        command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now.ToString("o"));
        command.Parameters.AddWithValue("@Id", playlistId);
        var result = command.ExecuteNonQuery() > 0;

        if (result)
        {
            playlist.Name = newName;
            playlist.ModifiedDate = DateTime.Now;
        }
        return result;
    }

    public bool AddSongToPlaylist(string playlistId, Song song)
    {
        if (!_playlistsCache.TryGetValue(playlistId, out var playlist))
            return false;

        // Check if song already exists in playlist
        if (playlist.Songs.Any(s => s.path == song.path))
            return false;

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand(
            @"INSERT INTO PlaylistSongs (PlaylistId, SongTitle, SongArtist, SongAlbum, SongTrackNo, SongGenre, SongDate, SongDiscNo, SongPath)
              VALUES (@PlaylistId, @Title, @Artist, @Album, @TrackNo, @Genre, @Date, @DiscNo, @Path)",
            connection);
        command.Parameters.AddWithValue("@PlaylistId", playlistId);
        command.Parameters.AddWithValue("@Title", song.title ?? "");
        command.Parameters.AddWithValue("@Artist", song.artist ?? "");
        command.Parameters.AddWithValue("@Album", song.album ?? "");
        command.Parameters.AddWithValue("@TrackNo", song.trackNo ?? "");
        command.Parameters.AddWithValue("@Genre", song.genre ?? "");
        command.Parameters.AddWithValue("@Date", song.date ?? "");
        command.Parameters.AddWithValue("@DiscNo", song.discNo ?? "");
        command.Parameters.AddWithValue("@Path", song.path ?? "");
        
        var result = command.ExecuteNonQuery() > 0;
        if (result)
        {
            playlist.Songs.Add(song);
            playlist.ModifiedDate = DateTime.Now;
            UpdatePlaylistModifiedDate(playlistId);
        }
        return result;
    }

    public bool RemoveSongFromPlaylist(string playlistId, string songPath)
    {
        if (!_playlistsCache.TryGetValue(playlistId, out var playlist))
            return false;

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand(
            "DELETE FROM PlaylistSongs WHERE PlaylistId = @PlaylistId AND SongPath = @SongPath",
            connection);
        command.Parameters.AddWithValue("@PlaylistId", playlistId);
        command.Parameters.AddWithValue("@SongPath", songPath);
        var result = command.ExecuteNonQuery() > 0;

        if (result)
        {
            var song = playlist.Songs.FirstOrDefault(s => s.path == songPath);
            if (song.path != null)
            {
                playlist.Songs.Remove(song);
                playlist.ModifiedDate = DateTime.Now;
                UpdatePlaylistModifiedDate(playlistId);
            }
        }
        return result;
    }

    public bool RemoveSongFromPlaylistAt(string playlistId, int index)
    {
        if (!_playlistsCache.TryGetValue(playlistId, out var playlist))
            return false;

        if (index < 0 || index >= playlist.Songs.Count)
            return false;

        var song = playlist.Songs[index];
        return RemoveSongFromPlaylist(playlistId, song.path);
    }

    public bool ClearPlaylist(string playlistId)
    {
        if (!_playlistsCache.TryGetValue(playlistId, out var playlist))
            return false;

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand(
            "DELETE FROM PlaylistSongs WHERE PlaylistId = @PlaylistId",
            connection);
        command.Parameters.AddWithValue("@PlaylistId", playlistId);
        var result = command.ExecuteNonQuery() > 0;

        if (result)
        {
            playlist.Clear();
            playlist.ModifiedDate = DateTime.Now;
            UpdatePlaylistModifiedDate(playlistId);
        }
        return result;
    }

    public void SavePlaylists()
    {
        // Data is already saved to database on each operation
    }

    public void LoadPlaylists()
    {
        LoadPlaylistsFromDatabase();
    }

    private void LoadPlaylistsFromDatabase()
    {
        _playlistsCache.Clear();

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        // Load all playlists
        using var command = new SqliteCommand("SELECT Id, Name, CreatedDate, ModifiedDate FROM Playlists", connection);
        using var reader = command.ExecuteReader();
        
        var playlistIds = new List<string>();
        while (reader.Read())
        {
            var id = reader.GetString(0);
            var name = reader.GetString(1);
            var createdDate = DateTime.Parse(reader.GetString(2));
            var modifiedDate = DateTime.Parse(reader.GetString(3));

            var playlist = new Playlist(name)
            {
                Id = id,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            };

            _playlistsCache[id] = playlist;
            playlistIds.Add(id);
        }

        // Load songs for each playlist
        foreach (var playlistId in playlistIds)
        {
            using var songCommand = new SqliteCommand(
                "SELECT SongTitle, SongArtist, SongAlbum, SongTrackNo, SongGenre, SongDate, SongDiscNo, SongPath FROM PlaylistSongs WHERE PlaylistId = @PlaylistId",
                connection);
            songCommand.Parameters.AddWithValue("@PlaylistId", playlistId);
            
            using var songReader = songCommand.ExecuteReader();
            while (songReader.Read())
            {
                var song = new Song(
                    songReader.GetString(0),
                    songReader.GetString(1),
                    songReader.GetString(2),
                    songReader.GetString(3),
                    songReader.GetString(4),
                    songReader.GetString(5),
                    songReader.GetString(6),
                    songReader.GetString(7)
                );
                _playlistsCache[playlistId].Songs.Add(song);
            }
        }
    }

    private void UpdatePlaylistModifiedDate(string playlistId)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand(
            "UPDATE Playlists SET ModifiedDate = @ModifiedDate WHERE Id = @Id",
            connection);
        command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now.ToString("o"));
        command.Parameters.AddWithValue("@Id", playlistId);
        command.ExecuteNonQuery();
    }
}

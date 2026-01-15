using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using musical_journey.Services.Interfaces;

namespace musical_journey.Services;

public class Database : IDb{

    public Song SelectSongByTitle(string SongTitle){
        string connectionString = "Data Source=cache.db;Version=3;";
        string query = "SELECT * FROM Songs WHERE Title = @SongTitle";
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@SongTitle", SongTitle);
        using var reader = command.ExecuteReader();
        if (reader.Read()){
            return new Song(
             reader.GetString(0),
             reader.GetString(1),
             reader.GetString(2),
             reader.GetString(3),
             reader.GetString(4),
             reader.GetString(5),
             reader.GetString(6),
             reader.GetString(7)
            );
        } else{
            return new Song("", "", "", "", "", "", "","");        
        }
    }
        public List<Song> SelectSongsByAlbum(string AlbumName){
        string connectionString = "Data Source=cache.db;Version=3;";
        string query = "SELECT * FROM Songs WHERE Album = @AlbumName";
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@AlbumName", AlbumName);
        using var reader = command.ExecuteReader();
        List<Song> songs = new List<Song>();
        while (reader.Read()){
            songs.Add(new Song(
             reader.GetString(0),
             reader.GetString(1), 
             reader.GetString(2),
             reader.GetString(3),
             reader.GetString(4),
             reader.GetString(5),
             reader.GetString(6),
             reader.GetString(7)
             ));
            }
            return songs;
        }
    public int InsertSong(Song song){
        string connectionString = "Data Source=cache.db;Version=3;";
        using var connection = new SqliteConnection(connectionString);
        string checkQ = "SELECT EXISTS(SELECT 1 FROM Songs WHERE Path=@Path)";
        using var checkCmd = new SqliteCommand(checkQ, connection);
        checkCmd.Parameters.AddWithValue("@Path", song.path);
        using var reader = checkCmd.ExecuteReader();
        if (reader.Read())
        {
            return 0;
        }

        string query = "INSERT INTO Songs (Path, Title, Artist, Album, TrackNo, Date, Genre, DiscNo) VALUES (@Path, @Title, @Artist, @Album, @TrackNo, @Date, @Genre, @DiscNo)";
        connection.Open();
        using var command = new SqliteCommand(query, connection);
        if (song.path == "")
        {
            return -1;
        }

        
        if (song.title != "")
        {
        command.Parameters.AddWithValue("@Path", song.path);
        command.Parameters.AddWithValue("@Title", song.title);
        command.Parameters.AddWithValue("@Artist", song.artist);
        command.Parameters.AddWithValue("@Album", song.album);
        command.Parameters.AddWithValue("@TrackNo", song.trackNo);
        command.Parameters.AddWithValue("@Date", song.date);
        command.Parameters.AddWithValue("@Genre", song.genre);
        command.Parameters.AddWithValue("@DiscNo", song.discNo);
        command.ExecuteNonQuery();
        return 0;
        }
        else
        {
            return -2;
        }        
    }
    public void InsertSongList(List<Song> songs)
    {
        foreach (var song in songs)
        {
            InsertSong(song);
        }
    }
    public void InsertSongWrapper(string SongPath)
    {
        IGetTags tagServ= new GetTag();
        Song tmpSong = tagServ.GetTagsByPath(SongPath);
        InsertSong(tmpSong);
    }
}
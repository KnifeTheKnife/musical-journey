using System.Collections.Generic;

namespace musical_journey.Services.Interfaces;
    public readonly struct Song
    {
        public readonly string title;
        public readonly string artist;
        public readonly string album;
        public readonly string trackNo;
        public readonly string genre;
        public readonly string date;
        public readonly string discNo;
        public readonly string path;

        public Song(string Title, string Artist, string Album, string TrackNo, string Genre, string Date, string DiscNo, string Path)
        {
            title = Title;
            artist = Artist;
            album = Album;
            trackNo = TrackNo;
            genre = Genre;
            date = Date;
            discNo = DiscNo;
            path = Path;
        }
    }
public interface IDb
{
     /// <summary>
    /// Inserts a song from table
    /// </summary>
    /// <param name="song">Song struct to insert</param>
    /// <returns>0 if successful; -1 if failed because no path; -2 if failed because no metadata (at least song title)</returns>
    int InsertSong(Song song);

    /// <summary>
    /// Select a song from table
    /// </summary>
    /// <param name="SongTitle">Song title string</param>
    /// <returns>Song struct containing all song data</returns>
    Song SelectSongByTitle(string SongTitle);

    /// <summary>
    /// Select songs by album name
    /// </summary>
    /// <param name="AlbumName">Album name string</param>
    /// <returns>List of Song structs</returns>
    List<Song> SelectSongsByAlbum(string AlbumName);
    /// <summary>
    /// Scrapes metadata and then adds it to the database
    /// </summary>
    /// <param name="SongPath">Path for song to be added to the database</param>
    void InsertSongWrapper(string SongPath);

    public void InsertSongList(List<Song> songs);
}
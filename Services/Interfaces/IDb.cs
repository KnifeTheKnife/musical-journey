using System.Collections.Generic;

namespace musical_journey.Services.Interfaces;
    public struct Song
    {
        public string title { get; set; }
        public string artist { get; set; }
        public string album { get; set; }
        public string trackNo { get; set; }
        public string genre { get; set; }
        public string date { get; set; }
        public string discNo { get; set; }
        public string path { get; set; }

        // Properties for Avalonia binding (using PascalCase)
        public string Title => title ?? "";
        public string Artist => artist ?? "";
        public string Album => album ?? "";
        public string TrackNo => trackNo ?? "";
        public string Genre => genre ?? "";
        public string Date => date ?? "";
        public string DiscNo => discNo ?? "";
        public string Path => path ?? "";

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
using System.Collections.ObjectModel;
using System.Windows.Input;
using musical_journey.Services.Interfaces;

namespace musical_journey.Models;

/// <summary>
/// Wrapper class to expose Song fields as properties for binding
/// </summary>
public class SongWrapper
{
    private readonly Song _song;
    
    public SongWrapper(Song song)
    {
        _song = song;
    }
    
    public string Title => _song.title;
    public string Artist => _song.artist;
    public string Album => _song.album;
    public string TrackNo => _song.trackNo;
    public string Genre => _song.genre;
    public string Date => _song.date;
    public string DiscNo => _song.discNo;
    public string Path => _song.path;
    
    public Song GetSong() => _song;
}

/// <summary>
/// Wrapper for album folders with command binding
/// </summary>
public class AlbumFolder
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public ICommand? ClickCommand { get; set; }
}

/// <summary>
/// Wrapper for grouping songs by album
/// </summary>
public class AlbumGroup
{
    public string AlbumName { get; set; } = "";
    public ObservableCollection<SongWrapper> Songs { get; set; } = new ObservableCollection<SongWrapper>();
}
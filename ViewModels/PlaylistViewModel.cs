using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using ReactiveUI;
using musical_journey.Commands;
using musical_journey.Models;
using musical_journey.Services.Interfaces;

namespace musical_journey.ViewModels;

/// <summary>
/// ViewModel responsible for playlist management
/// </summary>
public class PlaylistViewModel : ViewModelBase
{
    private readonly IPlaylistService _playlistService;
    private Playlist? _selectedPlaylist;
    private SongWrapper? _selectedPlaylistSong;
    private Song _selectedPlaylistSongDirect = default;

    public PlaylistViewModel(IPlaylistService playlistService)
    {
        _playlistService = playlistService ?? throw new ArgumentNullException(nameof(playlistService));

        // Initialize commands
        CreatePlaylistCommand = new AsyncCommand<string>(CreatePlaylist);
        DeletePlaylistCommand = new AsyncCommand(DeletePlaylist);
        AddSongToPlaylistCommand = new AsyncCommand(AddSongToPlaylist);
        RemoveSongFromPlaylistCommand = new AsyncCommand(RemoveSongFromPlaylist);
        AddAlbumToPlaylistCommand = new AsyncCommand(AddAlbumToPlaylist);
        ClearPlaylistCommand = new AsyncCommand(ClearPlaylist);

        // Load existing playlists
        LoadPlaylists();
    }

    #region Properties

    public ObservableCollection<Playlist> Playlists { get; } = new ObservableCollection<Playlist>();

    public Playlist? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedPlaylist, value);
            if (value != null)
            {
                System.Diagnostics.Debug.WriteLine($"Selected playlist: {value.Name}, Songs in collection: {value.Songs.Count}");
                foreach (var song in value.Songs)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {song.Title} by {song.Artist}");
                }
            }
        }
    }

    public SongWrapper? SelectedPlaylistSong
    {
        get => _selectedPlaylistSong;
        set => this.RaiseAndSetIfChanged(ref _selectedPlaylistSong, value);
    }

    public Song SelectedPlaylistSongDirect
    {
        get => _selectedPlaylistSongDirect;
        set => this.RaiseAndSetIfChanged(ref _selectedPlaylistSongDirect, value);
    }

    // Reference to selected song from library (for adding to playlist)
    public SongWrapper? SelectedLibrarySong { get; set; }

    // Reference to selected album group (for adding entire album to playlist)
    public AlbumGroup? SelectedAlbumGroup { get; set; }

    #endregion

    #region Commands

    public ICommand CreatePlaylistCommand { get; }
    public ICommand DeletePlaylistCommand { get; }
    public ICommand AddSongToPlaylistCommand { get; }
    public ICommand RemoveSongFromPlaylistCommand { get; }
    public ICommand AddAlbumToPlaylistCommand { get; }
    public ICommand ClearPlaylistCommand { get; }

    #endregion

    #region Private Methods

    private async Task CreatePlaylist(string? playlistName)
    {
        if (string.IsNullOrWhiteSpace(playlistName))
            return;

        var playlist = await Task.Run(() => _playlistService.CreatePlaylist(playlistName));
        
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Playlists.Add(playlist);
            SelectedPlaylist = playlist;
        });
    }

    private async Task DeletePlaylist()
    {
        if (SelectedPlaylist == null)
            return;

        var playlistIdToDelete = SelectedPlaylist.Id;

        // Execute delete on background thread
        await Task.Run(() =>
        {
            _playlistService.DeletePlaylist(playlistIdToDelete);
        });

        // Update UI on UI thread
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var playlistToRemove = Playlists.FirstOrDefault(p => p.Id == playlistIdToDelete);
            if (playlistToRemove != null)
            {
                Playlists.Remove(playlistToRemove);
                SelectedPlaylist = Playlists.FirstOrDefault();
            }
        });
    }

    private async Task AddSongToPlaylist()
    {
        if (SelectedPlaylist == null || SelectedLibrarySong == null)
            return;

        var song = SelectedLibrarySong.GetSong();
        await Task.Run(() =>
        {
            _playlistService.AddSongToPlaylist(SelectedPlaylist.Id, song);
        });
    }

    private async Task RemoveSongFromPlaylist()
    {
        if (SelectedPlaylist == null || string.IsNullOrEmpty(SelectedPlaylistSongDirect.Path))
            return;

        var songPath = SelectedPlaylistSongDirect.Path;
        
        await Task.Run(() =>
        {
            _playlistService.RemoveSongFromPlaylist(SelectedPlaylist.Id, songPath);
        });

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var songToRemove = SelectedPlaylist.Songs.FirstOrDefault(s => s.Path == songPath);
            if (songToRemove.Path != null)
            {
                SelectedPlaylist.Songs.Remove(songToRemove);
                SelectedPlaylistSongDirect = default;
            }
        });
    }

    private async Task AddAlbumToPlaylist()
    {
        if (SelectedPlaylist == null || SelectedAlbumGroup == null)
            return;

        var songs = SelectedAlbumGroup.Songs.Select(s => s.GetSong()).ToList();
        
        await Task.Run(() =>
        {
            foreach (var song in songs)
            {
                _playlistService.AddSongToPlaylist(SelectedPlaylist.Id, song);
            }
        });
    }

    private async Task ClearPlaylist()
    {
        if (SelectedPlaylist == null)
            return;

        var playlistId = SelectedPlaylist.Id;
        
        await Task.Run(() =>
        {
            _playlistService.ClearPlaylist(playlistId);
        });

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            SelectedPlaylist.Songs.Clear();
            SelectedPlaylistSongDirect = default;
        });
    }

    private void LoadPlaylists()
    {
        Playlists.Clear();
        var allPlaylists = _playlistService.GetAllPlaylists();
        
        System.Diagnostics.Debug.WriteLine($"Loading {allPlaylists.Count} playlists");
        
        foreach (var playlist in allPlaylists)
        {
            System.Diagnostics.Debug.WriteLine($"Loading playlist: {playlist.Name}, Songs count: {playlist.Songs.Count}");
            Playlists.Add(playlist);
        }
    }

    #endregion
}
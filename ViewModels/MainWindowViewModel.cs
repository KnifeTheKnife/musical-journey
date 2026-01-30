using System;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive.Concurrency;
using musical_journey.Services;
using musical_journey.Services.Interfaces;
using musical_journey.ViewModels;
namespace musical_journey.ViewModels;

/// <summary>
/// Main window view model that coordinates all child view models
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    public string Greeting => "Musical Journey";

    public MainWindowViewModel()
    {
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

        var fsRead = new FsRead();
        var getTags = new GetTag();
        var audioService = new AudioService();
        var playlistService = new PlaylistDatabaseService();


        PlaybackViewModel = new PlaybackViewModel(audioService);
        LibraryViewModel = new LibraryViewModel(fsRead, getTags);
        PlaylistViewModel = new PlaylistViewModel(playlistService);


        WireViewModels();
    }

    #region Child ViewModels

    public PlaybackViewModel PlaybackViewModel { get; }
    public LibraryViewModel LibraryViewModel { get; }
    public PlaylistViewModel PlaylistViewModel { get; }

    #endregion

    #region Convenience Properties

    /// <summary>
    /// Direct access to AudioService for backward compatibility.
    /// Prefer using PlaybackViewModel.AudioService in new code.
    /// </summary>
    public IAudioService AudioService => PlaybackViewModel.AudioService;

    /// <summary>
    /// Direct access to CreatePlaylistCommand for backward compatibility.
    /// Prefer using PlaylistViewModel.CreatePlaylistCommand in new code.
    /// </summary>
    public System.Windows.Input.ICommand CreatePlaylistCommand => PlaylistViewModel.CreatePlaylistCommand;

    /// <summary>
    /// Direct access to BrowseMusicFilesCommand for backward compatibility.
    /// Prefer using LibraryViewModel.BrowseMusicFilesCommand in new code.
    /// </summary>
    public System.Windows.Input.ICommand BrowseMusicFilesCommand => LibraryViewModel.BrowseMusicFilesCommand;

    #endregion

    #region Public Methods

    /// <summary>
    /// Attach the main window reference to ViewModels that need it
    /// </summary>
    public void AttachWindow(Window window)
    {
        LibraryViewModel.AttachWindow(window);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Wire up connections and data flow between child ViewModels
    /// </summary>    
    private void WireViewModels()
    {
        LibraryViewModel.WhenAnyValue(x => x.SelectedSong)
            .Subscribe(song =>
            {
                if (song != null)
                {
                    PlaybackViewModel.CurrentSong = song;
                }
            });

        LibraryViewModel.Songs.CollectionChanged += (s, e) =>
        {
            PlaybackViewModel.CurrentPlaylist.Clear();
            foreach (var song in LibraryViewModel.Songs)
            {
                PlaybackViewModel.CurrentPlaylist.Add(song);
            }
        };

        LibraryViewModel.WhenAnyValue(x => x.SelectedSong)
            .Subscribe(song =>
            {
                PlaylistViewModel.SelectedLibrarySong = song;
            });

        LibraryViewModel.WhenAnyValue(x => x.SelectedAlbumGroup)
            .Subscribe(albumGroup =>
            {
                PlaylistViewModel.SelectedAlbumGroup = albumGroup;
            });
    }

    #endregion
}
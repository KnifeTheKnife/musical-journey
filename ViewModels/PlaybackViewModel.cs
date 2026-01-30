using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using ReactiveUI;
using musical_journey.Commands;
using musical_journey.Models;
using musical_journey.Services;
using musical_journey.Services.Interfaces;

namespace musical_journey.ViewModels;

/// <summary>
/// ViewModel responsible for audio playback control
/// </summary>
public class PlaybackViewModel : ViewModelBase
{
    private readonly IAudioService _audioService;
    private float _playbackPosition;
    private int _volume = 100;
    private string _selectedSongTitle = "";
    private string _selectedSongArtist = "";
    private string _selectedSongAlbum = "";
    private string _selectedSongTrackNo = "";
    private string _selectedSongGenre = "";
    private string _selectedSongDate = "";
    private string _selectedSongDiscNo = "";
    private string _selectedSongPath = "";

    /// <summary>
    /// Gets the audio service for direct access when needed
    /// </summary>
    public IAudioService AudioService => _audioService;

    public PlaybackViewModel(IAudioService audioService)
    {
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        
        // Initialize commands
        PlayPauseCommand = ReactiveCommand.Create(PlayPause);
        PlaySongCommand = ReactiveCommand.Create<string>(PlaySong);
        StopCommand = new AsyncCommand(Stop);
        NextCommand = new AsyncCommand(PlayNext);
        PreviousCommand = new AsyncCommand(PlayPrevious);

        // Subscribe to media player events
        _audioService.MediaPlayer.EndReached += OnEndReached;
        _audioService.MediaPlayer.PositionChanged += OnPositionChanged;

        // Subscribe to volume changes
        this.WhenAnyValue(x => x.Volume).Subscribe(vol =>
        {
            if (_audioService?.MediaPlayer != null)
            {
                _audioService.MediaPlayer.Volume = vol;
            }
        });
    }

    #region Properties

    public ObservableCollection<SongWrapper> CurrentPlaylist { get; set; } = new ObservableCollection<SongWrapper>();

    public int Volume
    {
        get => _volume;
        set
        {
            this.RaiseAndSetIfChanged(ref _volume, value);
            if (_audioService?.MediaPlayer != null)
            {
                _audioService.MediaPlayer.Volume = value;
            }
        }
    }

    public float PlaybackPosition
    {
        get => _playbackPosition;
        set
        {
            this.RaiseAndSetIfChanged(ref _playbackPosition, value);
            if (_audioService?.MediaPlayer != null)
            {
                if (_audioService.MediaPlayer.IsPlaying || _audioService.MediaPlayer.WillPlay)
                {
                    // Check if difference is large to avoid loop when updating from VLC
                    if (Math.Abs(_audioService.MediaPlayer.Position - value) > 0.01)
                    {
                        _audioService.MediaPlayer.Position = value;
                    }
                }
            }
        }
    }

    public string SelectedSongTitle
    {
        get => _selectedSongTitle;
        set => this.RaiseAndSetIfChanged(ref _selectedSongTitle, value);
    }

    public string SelectedSongArtist
    {
        get => _selectedSongArtist;
        set => this.RaiseAndSetIfChanged(ref _selectedSongArtist, value);
    }

    public string SelectedSongAlbum
    {
        get => _selectedSongAlbum;
        set => this.RaiseAndSetIfChanged(ref _selectedSongAlbum, value);
    }

    public string SelectedSongTrackNo
    {
        get => _selectedSongTrackNo;
        set => this.RaiseAndSetIfChanged(ref _selectedSongTrackNo, value);
    }

    public string SelectedSongGenre
    {
        get => _selectedSongGenre;
        set => this.RaiseAndSetIfChanged(ref _selectedSongGenre, value);
    }

    public string SelectedSongDate
    {
        get => _selectedSongDate;
        set => this.RaiseAndSetIfChanged(ref _selectedSongDate, value);
    }

    public string SelectedSongDiscNo
    {
        get => _selectedSongDiscNo;
        set => this.RaiseAndSetIfChanged(ref _selectedSongDiscNo, value);
    }

    public string SelectedSongPath
    {
        get => _selectedSongPath;
        set => this.RaiseAndSetIfChanged(ref _selectedSongPath, value);
    }

    private SongWrapper? _currentSong;
    public SongWrapper? CurrentSong
    {
        get => _currentSong;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentSong, value);
            if (value != null)
            {
                UpdateSongInfo(value.GetSong());
                if (!string.IsNullOrEmpty(value.Path))
                {
                    _audioService.Play(value.Path);
                }
            }
        }
    }

    #endregion

    #region Commands

    public ICommand PlayPauseCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand PlaySongCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }

    #endregion

    #region Private Methods

    private void PlayPause()
    {
        if (_audioService.MediaPlayer.IsPlaying)
            _audioService.MediaPlayer.Pause();
        else
            _audioService.MediaPlayer.Play();
    }

    private void PlaySong(string path)
    {
        _audioService.Play(path);
    }

    private async Task Stop()
    {
        _audioService.MediaPlayer.Stop();
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            PlaybackPosition = 0;
        });
    }

    private async Task PlayNext()
    {
        System.Diagnostics.Debug.WriteLine($"Attempting Next. Songs in playlist: {CurrentPlaylist.Count}");

        if (CurrentPlaylist.Count == 0) return;

        int currentIndex = -1;
        if (CurrentSong != null)
        {
            currentIndex = CurrentPlaylist.ToList().FindIndex(s => s.Path == CurrentSong.Path);
        }

        System.Diagnostics.Debug.WriteLine($"Current index: {currentIndex}");

        int nextIndex = (currentIndex + 1) % CurrentPlaylist.Count;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            CurrentSong = CurrentPlaylist[nextIndex];
            System.Diagnostics.Debug.WriteLine($"Changed to: {CurrentSong?.Title}");
        });
    }

    private async Task PlayPrevious()
    {
        if (CurrentSong == null || CurrentPlaylist.Count == 0)
            return;

        int currentIndex = CurrentPlaylist.IndexOf(CurrentSong);
        
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (currentIndex > 0)
            {
                CurrentSong = CurrentPlaylist[currentIndex - 1];
            }
            else
            {
                CurrentSong = CurrentPlaylist[CurrentPlaylist.Count - 1]; // Loop back to the last song
            }
        });
    }

    private void UpdateSongInfo(Song song)
    {
        SelectedSongTitle = song.title;
        SelectedSongArtist = song.artist;
        SelectedSongAlbum = song.album;
        SelectedSongTrackNo = song.trackNo;
        SelectedSongGenre = song.genre;
        SelectedSongDate = song.date;
        SelectedSongDiscNo = song.discNo;
        SelectedSongPath = song.path;
    }

    private void OnEndReached(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            System.Diagnostics.Debug.WriteLine("[VLC] End of track. Switching...");
            await PlayNext();
        });
    }

    private void OnPositionChanged(object? sender, LibVLCSharp.Shared.MediaPlayerPositionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _playbackPosition = e.Position;
            this.RaisePropertyChanged(nameof(PlaybackPosition));
        });
    }

    #endregion
}
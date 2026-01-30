using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Platform.Storage;
using musical_journey.Services;
using musical_journey.Services.Interfaces;


namespace musical_journey.ViewModels;

// Wrapper class to expose Song fields as properties for binding
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

// Wrapper for album folders with command binding
public class AlbumFolder
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public ICommand? ClickCommand { get; set; }
}

// Wrapper for grouping songs by album
public class AlbumGroup
{
    public string AlbumName { get; set; } = "";
    public ObservableCollection<SongWrapper> Songs { get; set; } = new ObservableCollection<SongWrapper>();
}

public class MainWindowViewModel : ViewModelBase
{
    private readonly IFsRead fsRead;
    private readonly IGetTags getTags;
    private readonly IPlaylistService playlistService;
    private Window? mainWindow;
    private string _selectedSongTitle = "";
    private string _selectedSongArtist = "";
    private string _selectedSongAlbum = "";
    private string _selectedSongTrackNo = "";
    private string _selectedSongGenre = "";
    private string _selectedSongDate = "";
    private string _selectedSongDiscNo = "";
    private string _selectedSongPath = "";
    
    public ICommand BrowseMusicFilesCommand { get; }
    public IAudioService AudioService { get; }
    public ICommand CreatePlaylistCommand { get; }
    public ICommand DeletePlaylistCommand { get; }
    public ICommand AddSongToPlaylistCommand { get; }
    public ICommand RemoveSongFromPlaylistCommand { get; }
    public ICommand AddAlbumToPlaylistCommand { get; }
    public ICommand ClearPlaylistCommand { get; } 

    public string Greeting => "Musical Journey";
    
    private int _volume = 100;
    public int Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            if (AudioService?.MediaPlayer != null)
            {
                AudioService.MediaPlayer.Volume = _volume;
            }
        }  
 
    }
    private float _playbackPosition;
    public float PlaybackPosition
    {
        get => _playbackPosition;
        set 
        { this.RaiseAndSetIfChanged(ref _playbackPosition, value);
        if (AudioService?.MediaPlayer != null)
        {
        if (AudioService.MediaPlayer.IsPlaying || AudioService.MediaPlayer.WillPlay)
        {
            // Sprawdzamy czy różnica jest duża, żeby uniknąć zapętlenia przy aktualizacji z VLC
            if (Math.Abs(AudioService.MediaPlayer.Position - value) > 0.01)
            {
                AudioService.MediaPlayer.Position = value;
            }
        }
        }
        }
    }

    public ICommand PlayPauseCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand PlaySongCommand { get; }

    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }
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

    public MainWindowViewModel()
    {
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        fsRead = new FsRead();
        getTags = new GetTag();
        AudioService = new AudioService();
        playlistService = new PlaylistDatabaseService();
        AudioService.MediaPlayer.EndReached += (s, e) => 
        {
            Dispatcher.UIThread.Post(async () => 
            {
                Console.WriteLine("[VLC] Koniec utworu. Przełączam...");
                await PlayNext(); 
            });
        };

        AudioService.MediaPlayer.PositionChanged += (s, e) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                _playbackPosition = e.Position;
                this.RaisePropertyChanged(nameof(PlaybackPosition));
            });
        };
        
        BrowseMusicFilesCommand = new AsyncCommand(BrowseAndGetMusicFiles);
        //AudioService.Play("/home/marcy/Documents/mjuzik/taud.mp3");
        
        PlayPauseCommand = ReactiveCommand.Create(() =>
        {
            if (AudioService.MediaPlayer.IsPlaying)
                AudioService.MediaPlayer.Pause();
            else
                AudioService.MediaPlayer.Play();
        });
        
        PlaySongCommand = ReactiveCommand.Create<string>(path =>
        {
            AudioService.Play(path);
        }); 

        StopCommand = new AsyncCommand(async () => 
        {
    
            AudioService.MediaPlayer.Stop();
            await Dispatcher.UIThread.InvokeAsync(() => 
            {
                PlaybackPosition = 0; // To dotyka UI, więc musi być tutaj
            });
        });

        NextCommand = new AsyncCommand(async () => await PlayNext());
        PreviousCommand = new AsyncCommand(async () => await PlayPrevious());

        // Playlist commands
        CreatePlaylistCommand = new AsyncCommand<string>(async playlistName =>
        {
            if (!string.IsNullOrWhiteSpace(playlistName))
            {
                var playlist = await Task.Run(() => playlistService.CreatePlaylist(playlistName));
                Dispatcher.UIThread.Post(() =>
                {
                    Playlists.Add(playlist);
                    SelectedPlaylist = playlist;
                });
            }
        });

        DeletePlaylistCommand = new AsyncCommand(async () =>
        {
            if (SelectedPlaylist != null)
            {
                var playlistIdToDelete = SelectedPlaylist.Id;
                
                // Execute delete on background thread
                await Task.Run(() =>
                {
                    playlistService.DeletePlaylist(playlistIdToDelete);
                });
                
                // Update UI on UI thread
                Dispatcher.UIThread.Post(() =>
                {
                    var playlistToRemove = Playlists.FirstOrDefault(p => p.Id == playlistIdToDelete);
                    if (playlistToRemove != null)
                    {
                        Playlists.Remove(playlistToRemove);
                        SelectedPlaylist = Playlists.FirstOrDefault();
                    }
                });
            }
        });

        AddSongToPlaylistCommand = new AsyncCommand(async () =>
        {
            if (SelectedPlaylist != null && SelectedSong != null)
            {
                var song = SelectedSong.GetSong();
                await Task.Run(() =>
                {
                    playlistService.AddSongToPlaylist(SelectedPlaylist.Id, song);
                });
            }
        });

        RemoveSongFromPlaylistCommand = new AsyncCommand(async () =>
        {
            if (SelectedPlaylist != null && !string.IsNullOrEmpty(SelectedPlaylistSongDirect.Path))
            {
                var songPath = SelectedPlaylistSongDirect.Path;
                await Task.Run(() =>
                {
                    playlistService.RemoveSongFromPlaylist(SelectedPlaylist.Id, songPath);
                });
                Dispatcher.UIThread.Post(() =>
                {
                    var songToRemove = SelectedPlaylist.Songs.FirstOrDefault(s => s.Path == songPath);
                    if (songToRemove.Path != null)
                    {
                        SelectedPlaylist.Songs.Remove(songToRemove);
                        SelectedPlaylistSongDirect = default;
                    }
                });
            }
        });

        AddAlbumToPlaylistCommand = new AsyncCommand(async () =>
        {
            if (SelectedPlaylist != null && SelectedAlbumGroup != null)
            {
                var songs = SelectedAlbumGroup.Songs.Select(s => s.GetSong()).ToList();
                await Task.Run(() =>
                {
                    foreach (var song in songs)
                    {
                        playlistService.AddSongToPlaylist(SelectedPlaylist.Id, song);
                    }
                });
            }
        });

        ClearPlaylistCommand = new AsyncCommand(async () =>
        {
            if (SelectedPlaylist != null)
            {
                var playlistId = SelectedPlaylist.Id;
                await Task.Run(() =>
                {
                    playlistService.ClearPlaylist(playlistId);
                });
                Dispatcher.UIThread.Post(() =>
                {
                    SelectedPlaylist.Songs.Clear();
                    SelectedPlaylistSongDirect = default;
                });
            }
        });

        this.WhenAnyValue(x => x.Volume).Subscribe(vol =>
        {
            AudioService.MediaPlayer.Volume = vol;
        });

        // Load playlists on initialization
        LoadPlaylists();
    }
    
    private async Task PlayNext()
    {
    Console.WriteLine($"Próba Next. Ilość utworów w Songs: {Songs.Count}");
    
    if (Songs.Count == 0) return;

    // Szukamy po ścieżce, bo to najpewniejsza metoda porównania
    int currentIndex = -1;
    if (SelectedSong != null)
    {
        currentIndex = Songs.ToList().FindIndex(s => s.Path == SelectedSong.Path);
    }

    Console.WriteLine($"Obecny indeks: {currentIndex}");

    int nextIndex = (currentIndex + 1) % Songs.Count;
    
    // Zmiana SelectedSong odpali muzykę przez Twój setter
    Dispatcher.UIThread.Post(() => {
        SelectedSong = Songs[nextIndex];
        Console.WriteLine($"Zmieniono na: {SelectedSong?.Title}");
    });
    }

    private async Task PlayPrevious()
    {
        // Logic to play the previous song in the list
        if (SelectedSong == null || Songs.Count == 0)
            return;
        int currentIndex = Songs.IndexOf(SelectedSong);
        if (currentIndex > 0)
        {
            SelectedSong = Songs[currentIndex - 1];
        }
        else
        {
            SelectedSong = Songs[Songs.Count - 1]; // Loop back to the last song
        }
    }
    public void AttachWindow(Window window)
    {
        mainWindow = window;
    }

    public ObservableCollection<AlbumFolder> AlbumFolders { get; } = new ObservableCollection<AlbumFolder>();
    public ObservableCollection<AlbumGroup> AlbumGroups { get; } = new ObservableCollection<AlbumGroup>();
    
    // Legacy Songs collection kept for backward compatibility, if needed
    public ObservableCollection<SongWrapper> Songs { get; } = new ObservableCollection<SongWrapper>();
    
    // Playlist collections
    public ObservableCollection<Playlist> Playlists { get; } = new ObservableCollection<Playlist>();
    
    private Playlist? _selectedPlaylist;
    public Playlist? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _selectedPlaylist, value);
            if (value != null)
            {
                Console.WriteLine($"[DEBUG] Selected playlist: {value.Name}, Songs in collection: {value.Songs.Count}");
                foreach (var song in value.Songs)
                {
                    Console.WriteLine($"[DEBUG]   - {song.Title} by {song.Artist}");
                }
            }
        }
    }
    
    private SongWrapper? _selectedPlaylistSong;
    public SongWrapper? SelectedPlaylistSong
    {
        get => _selectedPlaylistSong;
        set => this.RaiseAndSetIfChanged(ref _selectedPlaylistSong, value);
    }

    private Song _selectedPlaylistSongDirect = default;
    public Song SelectedPlaylistSongDirect
    {
        get => _selectedPlaylistSongDirect;
        set => this.RaiseAndSetIfChanged(ref _selectedPlaylistSongDirect, value);
    }

    private AlbumGroup? _selectedAlbumGroup;
    public AlbumGroup? SelectedAlbumGroup
    {
        get => _selectedAlbumGroup;
        set => this.RaiseAndSetIfChanged(ref _selectedAlbumGroup, value);
    }
    
    private SongWrapper? _selectedSong;
    public SongWrapper? SelectedSong
    {
        get => _selectedSong;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSong, value);
            if (value != null)
            {
                UpdateSongInfo(value.GetSong());

                if (!string.IsNullOrEmpty(value.Path))
                {
                    AudioService.Play(value.Path);
                }
            }
        }
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

    private async Task BrowseAndGetMusicFiles()
    {
        AlbumFolders.Clear();
        Songs.Clear();

        if (mainWindow?.StorageProvider == null)
            return;

        var folders = await mainWindow.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Select Music Folder",
                AllowMultiple = false
            });

        if (folders.Count == 0)
            return;

        var selectedPath = folders[0].Path.LocalPath;
        
        // Get directories
        List<string> directories = await Task.Run(() => fsRead.ScanForDirectory(selectedPath));
        List<string> names = await Task.Run(() => fsRead.DirectoryNames(selectedPath));
        
        if (directories.Count != names.Count)
        {
            int count = Math.Min(directories.Count, names.Count);

            for (int i = 0; i < count; i++)
            {
                var dir = directories[i];
                var nm = names[i];
                var folder = new AlbumFolder
                {
                    Name = nm,
                    Path = dir,
                    ClickCommand = new AsyncCommand(async () => await LoadSongsFromFolder(dir))
                };
                AlbumFolders.Add(folder);
            }
        }
        else
        {
            for (int i = 0; i < directories.Count; i++)
            {
                var dir = directories[i];
                var nm = names[i];
                var folder = new AlbumFolder
                {
                    Name = nm,
                    Path = dir,
                    ClickCommand = new AsyncCommand(async () => await LoadSongsFromFolder(dir))
                };
                AlbumFolders.Add(folder);
            }
        }
    }

    private async Task LoadSongsFromFolder(string folderPath)
    {
        Songs.Clear();
        AlbumGroups.Clear();
        
        var musicFiles = await Task.Run(() => fsRead.GetMusicFiles(folderPath));
        
        var songsList = new List<SongWrapper>();
        foreach (var file in musicFiles)
        {
            var song = await Task.Run(() => getTags.GetTags(file));
            var wrapper = new SongWrapper(song);
            songsList.Add(wrapper);
        }
        
        var sortedSongs = songsList
            .Where(s => int.TryParse(s.TrackNo, out _))
            .OrderBy(s => int.Parse(s.TrackNo))
            .Concat(songsList
                .Where(s => !int.TryParse(s.TrackNo, out _))
                .OrderBy(s => s.Title))
            .ToList();
        
        foreach (var wrapper in sortedSongs)
        {
            Songs.Add(wrapper);
        }
        
        // Group songs by album
        var groupedByAlbum = songsList
            .GroupBy(s => string.IsNullOrWhiteSpace(s.Album) ? "[Unknown Album]" : s.Album)
            .OrderBy(g => g.Key);
        
        foreach (var albumGroup in groupedByAlbum)
        {
            var sortedGroupSongs = albumGroup
                .Where(s => int.TryParse(s.TrackNo, out _))
                .OrderBy(s => int.Parse(s.TrackNo))
                .Concat(albumGroup
                    .Where(s => !int.TryParse(s.TrackNo, out _))
                    .OrderBy(s => s.Title))
                .ToList();
            
            var group = new AlbumGroup
            {
                AlbumName = albumGroup.Key,
                Songs = new ObservableCollection<SongWrapper>(sortedGroupSongs)
            };
            AlbumGroups.Add(group);
        }
        
        System.Diagnostics.Debug.WriteLine($"Loaded {musicFiles.Count} songs from {folderPath}");
    }
    private void LoadPlaylists()
    {
        Playlists.Clear();
        var allPlaylists = playlistService.GetAllPlaylists();
        Console.WriteLine($"[DEBUG] Loading {allPlaylists.Count} playlists");
        foreach (var playlist in allPlaylists)
        {
            Console.WriteLine($"[DEBUG] Loading playlist: {playlist.Name}, Songs count: {playlist.Songs.Count}");
            Playlists.Add(playlist);
        }
    }

public class AsyncCommand : ICommand
{
    private readonly Func<Task> execute;
    private bool isExecuting;

    public AsyncCommand(Func<Task> execute)
    {
        this.execute = execute;
    }

    public bool CanExecute(object? parameter) => !isExecuting;

    public async void Execute(object? parameter)
    {
        if (isExecuting)
            return;

        isExecuting = true;
        OnCanExecuteChanged();

        try
        {
            await execute();
        }
        finally
        {
            isExecuting = false;
            OnCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged;

    protected virtual void OnCanExecuteChanged()
    {
        if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => OnCanExecuteChanged());
        }
    }
}}

public class AsyncCommand<T> : ICommand
{
    private readonly Func<T?, Task> execute;
    private bool isExecuting;

    public AsyncCommand(Func<T?, Task> execute)
    {
        this.execute = execute;
    }

    public bool CanExecute(object? parameter) => !isExecuting;

    public async void Execute(object? parameter)
    {
        if (isExecuting)
            return;

        isExecuting = true;
        OnCanExecuteChanged();

        try
        {
            await execute((T?)parameter);
        }
        finally
        {
            isExecuting = false;
            OnCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged;

    protected virtual void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
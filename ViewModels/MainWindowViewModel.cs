using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
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

public class MainWindowViewModel : ViewModelBase
{
    private readonly IFsRead fsRead;
    private readonly IGetTags getTags;
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

    public string SelectedSongTitle
    {
        get => _selectedSongTitle;
        set => SetProperty(ref _selectedSongTitle, value);
    }

    public string SelectedSongArtist
    {
        get => _selectedSongArtist;
        set => SetProperty(ref _selectedSongArtist, value);
    }

    public string SelectedSongAlbum
    {
        get => _selectedSongAlbum;
        set => SetProperty(ref _selectedSongAlbum, value);
    }

    public string SelectedSongTrackNo
    {
        get => _selectedSongTrackNo;
        set => SetProperty(ref _selectedSongTrackNo, value);
    }

    public string SelectedSongGenre
    {
        get => _selectedSongGenre;
        set => SetProperty(ref _selectedSongGenre, value);
    }

    public string SelectedSongDate
    {
        get => _selectedSongDate;
        set => SetProperty(ref _selectedSongDate, value);
    }

    public string SelectedSongDiscNo
    {
        get => _selectedSongDiscNo;
        set => SetProperty(ref _selectedSongDiscNo, value);
    }

    public string SelectedSongPath
    {
        get => _selectedSongPath;
        set => SetProperty(ref _selectedSongPath, value);
    }

    public MainWindowViewModel()
    {
        fsRead = new FsRead();
        getTags = new GetTag();
        BrowseMusicFilesCommand = new AsyncCommand(BrowseAndGetMusicFiles);
    }
    
    public void AttachWindow(Window window)
    {
        mainWindow = window;
    }

    public ObservableCollection<AlbumFolder> AlbumFolders { get; } = new ObservableCollection<AlbumFolder>();
    public ObservableCollection<SongWrapper> Songs { get; } = new ObservableCollection<SongWrapper>();
    
    private SongWrapper? _selectedSong;
    public SongWrapper? SelectedSong
    {
        get => _selectedSong;
        set
        {
            if (SetProperty(ref _selectedSong, value) && value != null)
            {
                UpdateSongInfo(value.GetSong());
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
        
        var musicFiles = await Task.Run(() => fsRead.GetMusicFiles(folderPath));
        
        foreach (var file in musicFiles)
        {
            var song = await Task.Run(() => getTags.GetTags(file));
            Songs.Add(new SongWrapper(song));
        }
        
        System.Diagnostics.Debug.WriteLine($"Loaded {musicFiles.Count} songs from {folderPath}");
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
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
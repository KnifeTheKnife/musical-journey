using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ReactiveUI;
using musical_journey.Commands;
using musical_journey.Models;
using musical_journey.Services.Interfaces;

namespace musical_journey.ViewModels;

/// <summary>
/// ViewModel responsible for music library browsing and album management
/// </summary>
public class LibraryViewModel : ViewModelBase
{
    private readonly IFsRead _fsRead;
    private readonly IGetTags _getTags;
    private Window? _mainWindow;
    private AlbumGroup? _selectedAlbumGroup;
    private SongWrapper? _selectedSong;

    public LibraryViewModel(IFsRead fsRead, IGetTags getTags)
    {
        _fsRead = fsRead ?? throw new ArgumentNullException(nameof(fsRead));
        _getTags = getTags ?? throw new ArgumentNullException(nameof(getTags));

        BrowseMusicFilesCommand = new AsyncCommand(BrowseAndGetMusicFiles);
    }

    #region Properties

    public ObservableCollection<AlbumFolder> AlbumFolders { get; } = new ObservableCollection<AlbumFolder>();
    public ObservableCollection<AlbumGroup> AlbumGroups { get; } = new ObservableCollection<AlbumGroup>();
    public ObservableCollection<SongWrapper> Songs { get; } = new ObservableCollection<SongWrapper>();

    public AlbumGroup? SelectedAlbumGroup
    {
        get => _selectedAlbumGroup;
        set => this.RaiseAndSetIfChanged(ref _selectedAlbumGroup, value);
    }

    public SongWrapper? SelectedSong
    {
        get => _selectedSong;
        set => this.RaiseAndSetIfChanged(ref _selectedSong, value);
    }

    #endregion

    #region Commands

    public ICommand BrowseMusicFilesCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Attach the main window for file picker dialogs
    /// </summary>
    public void AttachWindow(Window window)
    {
        _mainWindow = window;
    }

    #endregion

    #region Private Methods

    private async Task BrowseAndGetMusicFiles()
    {
        AlbumFolders.Clear();
        Songs.Clear();

        if (_mainWindow?.StorageProvider == null)
            return;

        var folders = await _mainWindow.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Select Music Folder",
                AllowMultiple = false
            });

        if (folders.Count == 0)
            return;

        var selectedPath = folders[0].Path.LocalPath;

        // Get directories
        List<string> directories = await Task.Run(() => _fsRead.ScanForDirectory(selectedPath));
        List<string> names = await Task.Run(() => _fsRead.DirectoryNames(selectedPath));

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

    private async Task LoadSongsFromFolder(string folderPath)
    {
        Songs.Clear();
        AlbumGroups.Clear();

        var musicFiles = await Task.Run(() => _fsRead.GetMusicFiles(folderPath));

        var songsList = new List<SongWrapper>();
        foreach (var file in musicFiles)
        {
            var song = await Task.Run(() => _getTags.GetTags(file));
            var wrapper = new SongWrapper(song);
            songsList.Add(wrapper);
        }

        // Sort songs by track number, then by title
        var sortedSongs = SortSongsByTrackNumber(songsList);

        foreach (var wrapper in sortedSongs)
        {
            Songs.Add(wrapper);
        }

        // Group songs by album
        GroupSongsByAlbum(songsList);

        System.Diagnostics.Debug.WriteLine($"Loaded {musicFiles.Count} songs from {folderPath}");
    }

    private List<SongWrapper> SortSongsByTrackNumber(List<SongWrapper> songs)
    {
        return songs
            .Where(s => int.TryParse(s.TrackNo, out _))
            .OrderBy(s => int.Parse(s.TrackNo))
            .Concat(songs
                .Where(s => !int.TryParse(s.TrackNo, out _))
                .OrderBy(s => s.Title))
            .ToList();
    }

    private void GroupSongsByAlbum(List<SongWrapper> songsList)
    {
        var groupedByAlbum = songsList
            .GroupBy(s => string.IsNullOrWhiteSpace(s.Album) ? "[Unknown Album]" : s.Album)
            .OrderBy(g => g.Key);

        foreach (var albumGroup in groupedByAlbum)
        {
            var sortedGroupSongs = SortSongsByTrackNumber(albumGroup.ToList());

            var group = new AlbumGroup
            {
                AlbumName = albumGroup.Key,
                Songs = new ObservableCollection<SongWrapper>(sortedGroupSongs)
            };
            AlbumGroups.Add(group);
        }
    }

    #endregion
}
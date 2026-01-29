using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using musical_journey.Services;
using musical_journey.Services.Interfaces;


namespace musical_journey.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IFsRead fsRead;
    private Window? mainWindow;
    
    public ICommand BrowseMusicFilesCommand { get; }
    public IAudioService AudioService { get; } 

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

    public ICommand PlayPauseCommand { get; }
    public ICommand StopCommand { get;}
    public ICommand PlaySongCommand { get; }

    public MainWindowViewModel()
    {
        fsRead = new FsRead();
        AudioService = new AudioService();
        BrowseMusicFilesCommand = new AsyncCommand(BrowseAndGetMusicFiles);
        //AudioService.Play("/home/marcy/Documents/musical-journey/taud.mp3");
        PlayPauseCommand = ReactiveCommand.Create(() =>
        {
            if (AudioService.MediaPlayer.IsPlaying)
            {
                AudioService.MediaPlayer.Pause();
            }
            else
            {
                AudioService.MediaPlayer.Play();
            }
        });

        StopCommand = ReactiveCommand.Create(() =>
        {
            AudioService.Stop();
        });
        PlaySongCommand = ReactiveCommand.Create<string>(path =>
        {
            AudioService.Play(path);
        });

        this.WhenAnyValue(x => x.Volume).Subscribe(vol =>
        {
            AudioService.MediaPlayer.Volume = vol;
        });
    }
    
    public void AttachWindow(Window window)
    {
        mainWindow = window;
    }

    //button counter
    public ObservableCollection<string> Albums { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> AlbumName { get; } = new ObservableCollection<string>();
    private async Task BrowseAndGetMusicFiles()
    {
        Albums.Clear();
        AlbumName.Clear();

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
        
        var musicFiles = await Task.Run(() => fsRead.GetMusicFiles(selectedPath));
        //stuff to attach to buttons
        List<string> directories = await Task.Run(() => fsRead.ScanForDirectory(selectedPath));
        List<string> name = await Task.Run(() => fsRead.DirectoryNames(selectedPath));
        for(int i=0;  i<directories.Count; i++)
        {
            AlbumName.Add(name[i]);
            Albums.Add(directories[i]);
            
             }


        // TODO: Do something with musicFiles
        System.Diagnostics.Debug.WriteLine($"Found {musicFiles.Count} music files");
    }

    //soon
    public async Task AlbumFolderFunc()
    {
        return;
    }
   
  }

// Simple async command implementation
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
using System;
using System.Collections.Generic;
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

    public MainWindowViewModel()
    {
        fsRead = new FsRead();
        BrowseMusicFilesCommand = new AsyncCommand(BrowseAndGetMusicFiles);
    }
    
    public void AttachWindow(Window window)
    {
        mainWindow = window;
    }
    
    private async Task BrowseAndGetMusicFiles()
    {
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
        
        // TODO: Do something with musicFiles
        System.Diagnostics.Debug.WriteLine($"Found {musicFiles.Count} music files");
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
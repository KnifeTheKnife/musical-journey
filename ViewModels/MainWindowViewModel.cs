
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using musical_journey.Services;
using musical_journey.Services.Interfaces;

namespace musical_journey.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFsRead fsRead;
    public string Greeting { get; } = "Welcome to Avalonia!";
    public ReactiveCommand<Unit, List<string>> BrowseMusicFilesCommand { get; }

    public MainWindowViewModel()
    {
        fsRead = new FsRead();
        BrowseMusicFilesCommand = ReactiveCommand.CreateFromTask(_ => BrowseAndGetMusicFiles());
    }    
    
    private async Task<List<string>> BrowseAndGetMusicFiles()
    {
        var topLevel = TopLevel.GetTopLevel(new Control());
        if (topLevel?.StorageProvider == null)
            return new List<string>();

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Select Music Folder",
                AllowMultiple = false
            });

        if (folders.Count == 0)
            return new List<string>();

        var selectedPath = folders[0].Path.LocalPath;
        return fsRead.GetMusicFiles(selectedPath);
    }
}
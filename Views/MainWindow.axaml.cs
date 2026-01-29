using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using musical_journey.ViewModels;
using musical_journey.Services;

namespace musical_journey.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Opened += (s, e) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.AttachWindow(this);
                var vlcView = this.FindControl<VideoView>("VideoPlayer");
                if (vlcView != null)
                {
                    // Łączymy widok z MediaPlayerem, który siedzi w serwisie w ViewModelu
                    vlcView.MediaPlayer = vm.AudioService.MediaPlayer;
                }
            }
        };
    }
}
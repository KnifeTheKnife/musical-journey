using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using musical_journey.ViewModels;
using musical_journey.Services;
using System.Threading.Tasks;

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
                    vlcView.MediaPlayer = vm.AudioService.MediaPlayer;
                }
                
                var createPlaylistBtn = this.FindControl<Button>("CreatePlaylistBtn");
                if (createPlaylistBtn != null)
                {
                    createPlaylistBtn.Click += async (sender, args) =>
                    {
                        var playlistName = await PromptForPlaylistName();
                        if (!string.IsNullOrWhiteSpace(playlistName))
                        {
                            vm.CreatePlaylistCommand.Execute(playlistName);
                        }
                    };
                }
            }
        };
    }

    private async Task<string?> PromptForPlaylistName()
    {
        var dialog = new Window
        {
            Title = "Create Playlist",
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var textBox = new TextBox
        {
            Margin = new Thickness(10),
            Watermark = "Enter playlist name"
        };

        var okButton = new Button
        {
            Content = "OK",
            Padding = new Thickness(20, 5),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Margin = new Thickness(10)
        };

        var cancelButton = new Button
        {
            Content = "Cancel",
            Padding = new Thickness(20, 5),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Margin = new Thickness(10)
        };

        string? result = null;

        okButton.Click += (s, e) =>
        {
            result = textBox.Text;
            dialog.Close();
        };

        cancelButton.Click += (s, e) =>
        {
            dialog.Close();
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 5,
            Margin = new Thickness(10)
        };
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        var mainPanel = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(10)
        };
        mainPanel.Children.Add(textBox);
        mainPanel.Children.Add(buttonPanel);

        dialog.Content = mainPanel;
        await dialog.ShowDialog(this);

        return result;
    }
}

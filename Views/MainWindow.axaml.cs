using Avalonia.Controls;
using musical_journey.ViewModels;

namespace musical_journey.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        Loaded += (s, e) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.AttachWindow(this);
            }
        };
    }
}
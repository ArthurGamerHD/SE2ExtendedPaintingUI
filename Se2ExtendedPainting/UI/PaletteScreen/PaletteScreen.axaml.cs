using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Keen.VRage.UI.AvaloniaInterface.Services;
using Keen.VRage.UI.Screens;
using Se2ExtendedPainting.Model;

namespace Se2ExtendedPainting.UI.PaletteScreen;

[NeedsWindowStyles]
public partial class PaletteScreen : ScreenView
{
    public PaletteScreen()
    {
        InitializeComponent();
    }

    private void LoadPalette(object? sender, TappedEventArgs e)
    {
        if (sender is Control control && control.DataContext is Palette palette)
            ((PaletteScreenViewModel)DataContext!).SelectedPalette = palette;
    }
}
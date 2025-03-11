using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Keen.VRage.Library.Mathematics;
using Keen.VRage.UI.AvaloniaInterface.Services;
using Keen.VRage.UI.Screens;

namespace Se2ExtendedPainting.UI.PaintScreen;

[NeedsWindowStyles]
public partial class ExtendedPaintUIScreen : ScreenView
{
    private readonly ExtendedPaintUIViewModel _viewModel = null!;

    public ExtendedPaintUIScreen()
    {
        InitializeComponent();
        if (Design.IsDesignMode) return;
        _viewModel = (ExtendedPaintUIViewModel)DataContext!;
        _viewModel.PropertyChanged += OnViewModelOnPropertyChanged;
        PART_ItemsRepeater.Loaded += OnPart_ItemsRepeaterOnLoaded;
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        PART_ItemsRepeater.Loaded -= OnPart_ItemsRepeaterOnLoaded;
        _viewModel.PropertyChanged -= OnViewModelOnPropertyChanged;
    }

    private void OnViewModelOnPropertyChanged(object? _, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(ExtendedPaintUIViewModel.SelectedIndex)) UpdatePalette();
    }

    private void OnPart_ItemsRepeaterOnLoaded(object? o, RoutedEventArgs routedEventArgs)
    {
        UpdatePalette();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is ColorHSV color)
            if (_viewModel.Palette.IndexOf(color) is var index && index is not -1)
                _viewModel.SelectedIndex = index;
    }

    private void Button_Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Button { DataContext: ColorHSV color } && e.GetCurrentPoint(null).Properties.IsRightButtonPressed)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                _viewModel.ResetPalette();
            else if (_viewModel.Palette.IndexOf(color) is var index and not -1) _viewModel.ResetIndex(index);
        }
    }

    private void UpdatePalette()
    {
        var palette = PART_ItemsRepeater.GetLogicalChildren().ToArray();

        for (var index = 0; index < palette.Length; index++)
        {
            var tile = palette[index];
            if (tile is Button button) button.Classes.Set("Active", index == _viewModel.SelectedIndex);
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Keen.Game2.Client.WorldObjects.Tools;
using Keen.Game2.Simulation.WorldObjects.Tools;
using Keen.VRage.Library.Definitions;
using Keen.VRage.Library.Mathematics;
using Keen.VRage.UI.Screens;

namespace Se2ExtendedPainting.UI.PaintScreen;

public partial class ExtendedPaintUIViewModel : ScreenViewModel
{
    public ColorHSV Color
    {
        get => _paintToolControllable.PaintData.Color;
        set => _paintToolControllable.PaintData.Color = value;
    }

    private PaintToolControllableComponent _paintToolControllable;
    public ObservableCollection<ColorHSV> Palette => _paintToolControllable.PaintData.Palette;

    public int SelectedIndex
    {
        get => _paintToolControllable.PaintData.PaletteIndex;
        set
        {
            _paintToolControllable.PaintData.PaletteIndex = value;
            Color = _paintToolControllable.PaintData.Palette[value];
        }
    }

    private Action _openPaletteSettings;

    public ExtendedPaintUIViewModel(PaintToolControllableComponent paintToolControllable, Action openPaletteSettings)
    {
        _paintToolControllable = paintToolControllable;
        _paintToolControllable.PaintData.PropertyChanged += PaintDataOnPropertyChanged;
        _openPaletteSettings = openPaletteSettings;
        InitializeInputContext();
    }

    private void PaintDataOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvokePropertyChanged(nameof(Palette));
        InvokePropertyChanged(nameof(Color));
        InvokePropertyChanged(nameof(SelectedIndex));
    }

    public void ResetIndex(int index)
    {
        _paintToolControllable.PaintData.Palette[index] = DefinitionManager.Instance
            .GetConfiguration<PaintToolConfiguration>().DefaultColors[index];
    }

    public void ResetPalette()
    {
        _paintToolControllable.PaintData.ResetPalette();
    }

    public void OpenPaletteSettings()
    {
        _openPaletteSettings();
    }
}
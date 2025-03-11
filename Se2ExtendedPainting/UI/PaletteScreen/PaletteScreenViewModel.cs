using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Collections;
using Keen.Game2.Client.WorldObjects.Tools;
using Keen.VRage.Library.Diagnostics;
using Keen.VRage.Library.Filesystem;
using Keen.VRage.Library.Threading;
using Keen.VRage.UI.Screens;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization.Json;
using Se2ExtendedPainting.Model;

namespace Se2ExtendedPainting.UI.PaletteScreen;

public class PaletteScreenViewModel : ScreenViewModel
{
    public string DisplayName { get; set; } = "Untitled Palette";
    public AvaloniaDictionary<string, Palette> PalettesCollection => PaletteCollection.Instance.Palettes;

    public AvaloniaList<Palette> Palettes { get; } = new();

    private object _lock = new();

    private Palette? _selectedPalette;

    private JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public Palette? SelectedPalette
    {
        get => _selectedPalette ?? new Palette(_paintToolControllable.PaintData.Palette.ToList())
        {
            DisplayName = "Current Palette",
            IsInternal = true
        };
        set
        {
            if (Equals(value, _selectedPalette)) return;

            if (value != null)
                for (var index = 0; index < value.Value.Colors.Count; index++)
                    _paintToolControllable.PaintData.Palette[index] = value.Value.Colors[index];

            _selectedPalette = value;
            OnPropertyChanged();
        }
    }

    public Palette ActivePalette => SelectedPalette!.Value;

    private PaintToolControllableComponent _paintToolControllable;


    public PaletteScreenViewModel(PaintToolControllableComponent controllable)
    {
        _paintToolControllable = controllable;

        InitializeInputContext();

        Init();

        PalettesCollection.PropertyChanged += PalettesCollectionOnPropertyChanged;
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        PalettesCollection.PropertyChanged -= PalettesCollectionOnPropertyChanged;
    }

    private void PalettesCollectionOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        RefreshPalettes();
    }

    private void RefreshPalettes()
    {
        Palettes.Clear();
        Palettes.AddRange(PalettesCollection.Values);
    }

    public void SaveCurrent()
    {
        SaveCurrentPalette();
    }

    public void DeleteCurrent()
    {
        DeleteCurrentPalette();
    }


    public async void Init()
    {
        try
        {
            _selectedPalette = null;

            await LoadPaletteCollection();

            foreach (var palette in PaletteCollection.Instance.Palettes)
                PalettesCollection[palette.Key] = palette.Value;

            RefreshPalettes();
        }
        catch (Exception e)
        {
            Log.Default.Error($"[{PaintToolPlugin.PluginId} ]" + e.Message);
        }
    }

    private void SaveCurrentPalette()
    {
        if (string.IsNullOrEmpty(DisplayName))
            DisplayName = "Untitled Palette";

        var palette = new Palette(_paintToolControllable.PaintData.Palette.ToList())
        {
            DisplayName = DisplayName
        };
        PaletteCollection.Instance.Palettes[DisplayName] = palette;
        SavePaletteCollection();
    }

    private void DeleteCurrentPalette()
    {
        if (_selectedPalette != null && !_selectedPalette.Value.IsInternal)
        {
            PaletteCollection.Instance.Palettes.Remove(_selectedPalette.Value.DisplayName);
            SavePaletteCollection();
            _selectedPalette = PaletteCollection.Instance.Palettes.FirstOrDefault().Value;
        }
    }

    private async Task LoadPaletteCollection()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (PaletteCollection.Instance == null)
            await ForceLoadPalettes();
    }

    private async Task ForceLoadPalettes()
    {
        var appData = FileSystem.AppData;
        if (!appData.DirectoryExists("ExtendedPainting\\"))
            FileSystem.Instance.GetAppDataFiles().CreateDirectory("ExtendedPainting");

        await using var config = FileSystem.Instance.GetAppDataFiles().Open("ExtendedPainting\\palettes.json",
            FileMode.OpenOrCreate, FileAccess.ReadWrite);

        try
        {
            PaletteCollection.Instance =
                JsonSerializer.Deserialize<PaletteCollection>(config) ?? new PaletteCollection();
        }
        catch (Exception e)
        {
            PaletteCollection.Instance = new PaletteCollection();
            Log.Default.Error($"[{PaintToolPlugin.PluginId}] Fail to load settings {e}");
        }
        finally
        {
            config.Close();
        }
    }

    private void SavePaletteCollection()
    {
        lock (_lock)
        {
            var appData = FileSystem.AppData;
            if (!appData.DirectoryExists("ExtendedPainting\\"))
                FileSystem.Instance.GetAppDataFiles().CreateDirectory("ExtendedPainting");

            using var config = FileSystem.Instance.GetAppDataFiles()
                .Open("ExtendedPainting\\palettes_temp.json", FileMode.CreateNew, FileAccess.Write);

            var error = false;

            try
            {
                JsonSerializer.Serialize(config, PaletteCollection.Instance, _options);
            }
            catch (Exception e)
            {
                error = true;
                Log.Default.Error($"[{PaintToolPlugin.PluginId}] Fail to load settings {e}");
            }
            finally
            {
                config.Close();
            }

            if (!error)
                FileSystem.Instance.GetAppDataFiles().MoveFile("ExtendedPainting\\palettes_temp.json",
                    "ExtendedPainting\\palettes.json", true);
        }
    }
}
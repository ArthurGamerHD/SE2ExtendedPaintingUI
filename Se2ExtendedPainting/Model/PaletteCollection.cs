using System;
using Avalonia.Collections;
using System.Collections.Generic;

namespace Se2ExtendedPainting.Model;

[Serializable]
public class PaletteCollection
{
    public static PaletteCollection Instance { get; internal set; } = null!;

    public AvaloniaDictionary<string, Palette> Palettes { get; set; } = new();
}
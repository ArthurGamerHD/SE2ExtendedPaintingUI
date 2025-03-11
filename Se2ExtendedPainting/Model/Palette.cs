using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Keen.VRage.Library.Filesystem.StorageManagers;
using Keen.VRage.Library.Mathematics;
using Keen.VRage.Library.Reflection;
using Keen.VRage.Library.Serialization;
using Keen.VRage.Library.Serialization.Binary;
using Keen.VRage.Library.Serialization.Generic;
using Keen.VRage.Library.Serialization.Json;
using Keen.VRage.Library.Serialization.Migrations;
using Keen.VRage.Library.Utils.Cloning;

namespace Se2ExtendedPainting.Model;

[Serialize]
public partial struct Palette
{
    public string DisplayName { get; set; } = "Untitled Palette";

    public List<ColorHSV> Colors { get; set; } =
    [
        new(0.0f, 0.0f, 0.45f, 1f), new(0.0f, 0.0f, 0.65f, 1f),
        new(0.0f, 0.8f, 0.5f, 1f),
        new(0.0f, 0.95f, 0.7f, 1f), new(0.33333334f, 0.32f, 0.2f, 1f),
        new(0.33333334f, 0.47f, 0.4f, 1f), new(0.575f, 0.8f, 0.45f, 1f),
        new(0.575f, 0.95f, 0.65f, 1f),
        new(0.12222222f, 0.7f, 0.71f, 1f),
        new(0.12222222f, 0.85f, 0.91f, 1f),
        new(0.0f, 0.0f, 0.85f, 1f),
        new(0.0f, 0.0f, 1f, 1f),
        new(0.0f, 0.0f, 0.0f, 1f),
        new(0.0f, 0.0f, 0.15f, 1f)
    ];

    public bool IsInternal { get; init; }

    public Palette()
    {
    }

    public Palette(List<ColorHSV> palette) : this()
    {
        Colors = palette;
    }
}
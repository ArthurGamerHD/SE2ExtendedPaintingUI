using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Keen.Game2.Client.GameSystems.PlayerControl.PlayerInput.InputHandlers;
using Keen.Game2.Client.UI.Library;
using Keen.Game2.Client.WorldObjects.Tools;
using Keen.Game2.Game.Plugins;
using Keen.Game2.Simulation.Utils;
using Keen.VRage.Input;
using Keen.VRage.Input.Extensions;
using Keen.VRage.Library.Definitions;
using Keen.VRage.Library.Diagnostics;
using Keen.VRage.Library.Filesystem;
using Se2ExtendedPainting.Model;
using Se2ExtendedPainting.UI;
using Se2ExtendedPainting.UI.PaintScreen;
using Se2ExtendedPainting.UI.PaletteScreen;

namespace Se2ExtendedPainting;

public class PaintToolPlugin : IPlugin
{
    public const string PluginId = "SeExtendedPainting";
    private Harmony _harmony = new(PluginId);

    public PaintToolPlugin()
    {
#if InjectorDebug
        var @try = 0;
        Log.Default.WriteLine($"[{PluginId}] Initializing in DEBUG mode.");
        while (!Debugger.IsAttached && @try < 30)
        {
            Log.Default.WriteLine($"[{PluginId}] Waiting for debugger attachment...");
            System.Threading.Thread.Sleep(1000);
            @try++;
        }

        if (!Debugger.IsAttached)
            Log.Default.WriteLine(
                $"[{PluginId}] Debugger not attached, consider building in Release to avoid 30 seconds wait time");
#else
        Log.Default.WriteLine($"[{PluginId}] Initializing.");
#endif

        try
        {
            var preparePaintHandlers = typeof(PaintToolInputHandlerComponent)?.GetMethod("PrepareInputHandlers",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var postFixPaintHandlers = typeof(PaintToolPlugin)?.GetMethod(nameof(PrepareInputHandlers_PostFix),
                BindingFlags.Static | BindingFlags.Public);

            _harmony.Patch(preparePaintHandlers, postfix: postFixPaintHandlers);


            var uiInitialized =
                typeof(SharedUIComponent)?.GetMethod("PostInit", BindingFlags.Public | BindingFlags.Instance);
            var uiPostFix =
                typeof(PaintToolPlugin)?.GetMethod(nameof(PostInit_PostFix), BindingFlags.Static | BindingFlags.Public);

            _harmony.Patch(uiInitialized, postfix: uiPostFix);
        }
        catch (Exception e)
        {
            Log.Default.WriteLine($"[{PluginId}] Fail to initialize paint tool plugin: {e}");
        }

        Log.Default.WriteLine($"[{PluginId}] Initialized");
    }

    private static SharedUIComponent? SharedUI;
    private static PaintToolInputHandlerComponent? _paintHandler;

    public static void PostInit_PostFix(SharedUIComponent __instance)
    {
        SharedUI = __instance;
        Log.Default.WriteLine($"[{PluginId}] SharedUI captured");
    }

    public static void PrepareInputHandlers_PostFix(InputContext context, PaintToolInputHandlerComponent __instance)
    {
        Log.Default.WriteLine($"[{PluginId}] Paint Tool Equipped");

        _paintHandler = __instance;

        //we don't have reassignable keys yet, so P for *P*arking could be use as P for *P*aint as well
        var brakes = DefinitionManager.Instance.GetDefinitionsOfType<CockpitInputHandlerDefinition>().First()
            .ToggleParkingBrakes;

        var definition = new HashSet<InputActionDefinition>();
        foreach (var action in context.Definition.Actions)
            definition.Add(action);
        definition.Add(brakes);

        typeof(InputContextDefinition).GetField("_actions", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(context.Definition, definition);

        context.SetTrigger(brakes, OpenExtendedPaintTool);
    }

    public static void OpenExtendedPaintTool()
    {
        if (SharedUI != null &&
            typeof(PaintToolInputHandlerComponent)
                ?.GetField("_paintToolControllable", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(_paintHandler) is PaintToolControllableComponent controllable)
        {
            var vm = new ExtendedPaintUIViewModel(controllable, OpenPaletteSettings);
            SharedUI?.CreateScreen<ExtendedPaintUIScreen>(vm, true);
            Log.Default.WriteLine($"[{PluginId}] Opened extended paint tool UI");
        }
        else
        {
            Log.Default.WriteLine($"[{PluginId}] Fail to open extended paint tool UI");
        }
    }

    public static void OpenPaletteSettings()
    {
        if (SharedUI != null &&
            typeof(PaintToolInputHandlerComponent)
                ?.GetField("_paintToolControllable", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(_paintHandler) is PaintToolControllableComponent controllable)
        {
            var vm = new PaletteScreenViewModel(controllable);
            SharedUI?.CreateScreen<PaletteScreen>(vm, true);
            Log.Default.WriteLine($"[{PluginId}] Opened palette settings UI");
        }
        else
        {
            Log.Default.WriteLine($"[{PluginId}] Fail to open palette settings UI");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Components;
using FontChanger.Configuration;
using HarmonyLib;
using HMUI;
using TMPro;
using UnityEngine;

namespace FontChanger.Patches;

internal abstract class PatcherFunctions
{
    private static PluginConfig Config => PluginConfig.Instance;

    private static readonly TMP_FontAsset TekoFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
        .LastOrDefault(f2 => f2.name == "Teko-Medium SDF")!;

    public static void Unpatch(TMP_Text instance)
    {
        if (instance.font.name.Contains(Config.FontName))
        {
            instance.font = TekoFont;
        }
    }

    public static void Patch(TMP_Text instance, List<TMP_FontAsset> fontAssets, bool force = false)
    {
        if (!Config.Enabled)
        {
            return;
        }

        if (!instance.font.name.Contains("Teko") && !force)
        {
            return;
        }
        
        instance.font = fontAssets.FirstOrDefault(font => font.name.Contains(Config.FontName));
        instance.SetAllDirty();
    }
}

[HarmonyPatch(typeof(TMP_Text), "text", MethodType.Setter)]
[HarmonyPriority(int.MinValue)]
internal class FontPatch
{
    private static readonly Type[] CurvedTypes =
    {
        typeof(CurvedTextMeshPro),
        typeof(FormattableText),
        typeof(ClickableText)
    };
    private static readonly Type[] StandardTypes =
    {
        typeof(TextMeshPro)
    };
    private static PluginConfig Config => PluginConfig.Instance;

    internal static void Finalizer(TMP_Text __instance)
    {
        if (!Config.Enabled || __instance == null)
        {
            return;
        }

        if (!__instance.isActiveAndEnabled)
        {
            return;
        }
        
        Type type = __instance.GetType();
            
        if (CurvedTypes.Contains(type))
        {
            PatcherFunctions.Patch(__instance, Managers.FontManager.Fonts);
        }
        else if (StandardTypes.Contains(type))
        {
            PatcherFunctions.Patch(__instance, Managers.FontManager.StandardFonts);
        }
        else
        {
            Plugin.Log.Debug($"{__instance.name} ({__instance.GetInstanceID()}) -- {__instance.GetType()}");
        }
    }
}

[HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable")]
[HarmonyPriority(int.MinValue)]
internal class FontPatchUGUIOnEnable
{
    internal static void Finalizer(TextMeshProUGUI __instance)
    {
        PatcherFunctions.Patch(__instance, Managers.FontManager.Fonts);
    }
}
    
[HarmonyPatch(typeof(TextMeshPro), "OnEnable")]
[HarmonyPriority(int.MinValue)]
internal class FontPatchOnEnable
{
    internal static void Finalizer(TextMeshPro __instance)
    {
        PatcherFunctions.Patch(__instance, Managers.FontManager.StandardFonts);
    }
}
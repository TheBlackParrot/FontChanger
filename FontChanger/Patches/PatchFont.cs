﻿using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Components;
using FontChanger.Configuration;
using FontChanger.Managers;
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

        if (FontLists.Fonts.FindIndex(x => x.name == $"{Config.FontName}-Curved(Clone)") == -1)
        {
            return;
        }
        
        instance.font = fontAssets.FirstOrDefault(font => font.name.Contains(Config.FontName));
        if (Config.ForceDisableItalic && (instance.fontStyle & FontStyles.Italic) == FontStyles.Italic)
        {
            instance.fontStyle ^= FontStyles.Italic;
        }

        instance.SetAllDirty();
    }
}

[HarmonyPatch(typeof(TMP_Text), "text", MethodType.Setter)]
[HarmonyPriority(int.MinValue)]
internal class FontPatch
{
    private static readonly Type[] CurvedTypes =
    [
        typeof(CurvedTextMeshPro),
        typeof(FormattableText),
        typeof(ClickableText)
    ];
    private static readonly Type[] StandardTypes =
    [
        typeof(TextMeshPro)
    ];
    private static PluginConfig Config => PluginConfig.Instance;

    // ReSharper disable once InconsistentNaming
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
            PatcherFunctions.Patch(__instance, FontManager.Fonts);
        }
        else if (StandardTypes.Contains(type))
        {
            PatcherFunctions.Patch(__instance, FontManager.StandardFonts);
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
    // ReSharper disable once InconsistentNaming
    internal static void Finalizer(TextMeshProUGUI __instance)
    {
        PatcherFunctions.Patch(__instance, FontManager.Fonts);
    }
}
    
[HarmonyPatch(typeof(TextMeshPro), "OnEnable")]
[HarmonyPriority(int.MinValue)]
internal class FontPatchOnEnable
{
    // ReSharper disable once InconsistentNaming
    internal static void Finalizer(TextMeshPro __instance)
    {
        PatcherFunctions.Patch(__instance, FontManager.StandardFonts);
    }
}

[HarmonyPatch]
internal class FixItalicOffsets
{
    private static PluginConfig Config => PluginConfig.Instance;
    
    [HarmonyPatch(typeof(LevelListTableCell), "SetDataFromLevelAsync")]
    [HarmonyPriority(int.MinValue)]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    internal static void TableViewSetupFix(LevelListTableCell __instance)
    {
        if (!Config.ForceDisableItalic)
        {
            return;
        }
        
        Transform songAuthor = __instance.transform.Find("SongAuthor");
        RectTransform songAuthorRect = songAuthor.GetComponent<RectTransform>();
        songAuthor.GetComponent<RectTransform>().offsetMin = songAuthorRect.offsetMin with { x = 9.7f };
        
        Transform songTime = __instance.transform.Find("SongTime");
        RectTransform songTimeRect = songTime.GetComponent<RectTransform>();
        songTime.GetComponent<RectTransform>().offsetMax = songTimeRect.offsetMax with { x = -2.33f };
    }

    [HarmonyPatch(typeof(LevelBar), "SetupData")]
    [HarmonyPriority(int.MinValue)]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    internal static void LevelBarSetupFix(LevelBar __instance)
    {
        if (!Config.ForceDisableItalic)
        {
            return;
        }
        
        Transform songAuthor = __instance.transform.Find("SingleLineTextContainer").Find("AuthorNameText");
        RectTransform songAuthorRectSingleLine = songAuthor.GetComponent<RectTransform>();
        songAuthor.GetComponent<RectTransform>().offsetMin = songAuthorRectSingleLine.offsetMin with { x = 0.81f };
        
        Transform songAuthorMultipleLine = __instance.transform.Find("MultipleLineTextContainer").Find("AuthorNameText");
        RectTransform songAuthorRectMultipleLine = songAuthorMultipleLine.GetComponent<RectTransform>();
        songAuthorMultipleLine.GetComponent<RectTransform>().offsetMin = songAuthorRectMultipleLine.offsetMin with { x = 0.81f };
        
        Transform songTitleMultipleLine = __instance.transform.Find("MultipleLineTextContainer").Find("SongNameText");
        CurvedTextMeshPro songTitleTMPComponent = songTitleMultipleLine.GetComponent<CurvedTextMeshPro>();
        songTitleMultipleLine.GetComponent<CurvedTextMeshPro>().text = songTitleTMPComponent.text[1..];
    }
}
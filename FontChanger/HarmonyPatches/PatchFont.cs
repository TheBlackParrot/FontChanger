using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BeatSaberMarkupLanguage.Components;
using FontChanger.Configuration;
using HarmonyLib;
using HMUI;
using TMPro;
using UnityEngine;

namespace FontChanger.HarmonyPatches
{
    /*
    [HarmonyPatch(typeof(TMP_Text), "fontSizeMin", MethodType.Setter)]
    [HarmonyPatch(typeof(TMP_Text), "fontSizeMax", MethodType.Setter)]
    [HarmonyPatch(typeof(TMP_Text), "fontSize", MethodType.Setter)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class FontSizePatch
    {
        private static readonly PluginConfig Config = PluginConfig.Instance;
        
        internal static bool Prefix(ref float value)
        { 
            value *= Config.FontSizeMultiplier;
            return true;
        }
    }
    */
    
    /*
    [HarmonyPatch(typeof(TMP_Settings), "defaultFontSize", MethodType.Getter)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class FontSizePatch
    {
        private static readonly PluginConfig Config = PluginConfig.Instance;
        
        static bool Prefix(ref float __result)
        {
            Plugin.Log.Info($"{__result}");
            __result *= Config.FontSizeMultiplier;
            return false;
        }
    }
    */
    
    /*
    [HarmonyPatch(typeof(TMP_Text), "fontSize", MethodType.Setter)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class FontSizePatch
    {
        private static readonly PluginConfig Config = PluginConfig.Instance;
        
        internal static bool Prefix(ref float value)
        { 
            value *= Config.FontSizeMultiplier;
            return true;
        }
    }
    */

    [HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class FontPatch
    {
        private static readonly PluginConfig Config = PluginConfig.Instance;
        
        internal static void Postfix(TextMeshProUGUI __instance)
        {
            if (!__instance.font.name.Contains("Teko-Medium") || !Config.Enabled)
            {
                return;
            }
            
            List<TMP_FontAsset> fontAssets = Managers.FontManager.Fonts;

            bool previouslyUppercase = __instance.fontStyle.HasFlag(FontStyles.UpperCase);
            bool previouslyItalic = __instance.fontStyle.HasFlag(FontStyles.Italic);

            int styleFlag = (Config.FontItalic && previouslyItalic ? (int)FontStyles.Italic : (int)FontStyles.Normal);
            int caseFlag = (Config.FontUppercase && previouslyUppercase ? (int)FontStyles.UpperCase : 0);
            
            __instance.font = fontAssets.FirstOrDefault(font => font.name.Contains(Config.FontName));
            __instance.fontStyle = (FontStyles)(styleFlag | caseFlag);
            __instance.fontSize *= Config.FontSizeMultiplier;
            __instance.fontSizeMin *= Config.FontSizeMultiplier;
            __instance.fontSizeMax *= Config.FontSizeMultiplier;
            __instance.characterSpacing = Config.CharSpacing;
            __instance.wordSpacing = Config.WordSpacingAdjustment;
        }
    }
}
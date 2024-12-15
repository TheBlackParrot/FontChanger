using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FontChanger.Configuration;
using HarmonyLib;
using HMUI;
using TMPro;

namespace FontChanger.HarmonyPatches
{
    [HarmonyPatch(typeof(CurvedTextMeshPro), "OnEnable")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class FontPatch
    {
        private static readonly PluginConfig Config = PluginConfig.Instance;
        
        internal static void Prefix(CurvedTextMeshPro __instance)
        {
            if (!__instance.font.name.Contains("Teko-Medium") || !Config.Enabled)
            {
                return;
            }
            
            List<TMP_FontAsset> fontAssets = Managers.FontManager.Fonts;

            bool previouslyUppercase = __instance.fontStyle.HasFlag(FontStyles.UpperCase);
            bool previouslyItalic = __instance.fontStyle.HasFlag(FontStyles.Italic);
            int italicFlag = (int)FontStyles.Italic;
            int normalFlag = (int)FontStyles.Normal;
            int uppercaseFlag = (int)FontStyles.UpperCase;
            
            __instance.font = fontAssets.FirstOrDefault(font => font.name.Contains(Config.FontName));
            __instance.fontStyle = (FontStyles)((Config.FontItalic && previouslyItalic ? italicFlag : normalFlag) | (previouslyUppercase ? uppercaseFlag : 0));
            __instance.fontSize *= Config.FontSizeMultiplier;
            __instance.characterSpacing = Config.CharSpacing;
            __instance.wordSpacing = Config.WordSpacingAdjustment;
        }
    }
    
    [HarmonyPatch(typeof(TextMeshPro), "OnEnable")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class FontPatchTMP
    {
        private static readonly PluginConfig Config = PluginConfig.Instance;
        
        internal static void Prefix(TextMeshPro __instance)
        {
            if (!__instance.font.name.Contains("Teko-Medium") || !Config.Enabled)
            {
                return;
            }
            
            List<TMP_FontAsset> fontAssets = Managers.FontManager.StandardFonts;

            bool previouslyUppercase = __instance.fontStyle.HasFlag(FontStyles.UpperCase);
            bool previouslyItalic = __instance.fontStyle.HasFlag(FontStyles.Italic);
            int italicFlag = (int)FontStyles.Italic;
            int normalFlag = (int)FontStyles.Normal;
            int uppercaseFlag = (int)FontStyles.UpperCase;
            
            __instance.font = fontAssets.FirstOrDefault(font => font.name.Contains(Config.FontName));
            __instance.fontStyle = (FontStyles)((Config.FontItalic && previouslyItalic ? italicFlag : normalFlag) | (previouslyUppercase ? uppercaseFlag : 0));
            __instance.fontSize *= Config.FontSizeMultiplier;
            __instance.characterSpacing = Config.CharSpacing;
            __instance.wordSpacing = Config.WordSpacingAdjustment;
        }
    }
}
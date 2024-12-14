using System.Collections.Generic;
using System.Linq;
using FontChanger.Configuration;
using HarmonyLib;
using HMUI;
using TMPro;

namespace FontChanger.HarmonyPatches
{
    [HarmonyPatch(typeof(CurvedTextMeshPro), "OnEnable")]
    internal class FontPatch
    {
        internal static readonly PluginConfig _config = PluginConfig.Instance;
        
        internal static void Prefix(CurvedTextMeshPro __instance)
        {
            if (!__instance.font.name.Contains("Teko-Medium"))
            {
                return;
            }
            
            List<TMP_FontAsset> fontAssets = Managers.FontManager.Fonts;

            bool previouslyUppercase = __instance.fontStyle.HasFlag(FontStyles.UpperCase);
            bool previouslyItalic = __instance.fontStyle.HasFlag(FontStyles.Italic);
            int italicFlag = (int)FontStyles.Italic;
            int normalFlag = (int)FontStyles.Normal;
            int uppercaseFlag = (int)FontStyles.UpperCase;
            
            __instance.font = fontAssets.FirstOrDefault(font => font.name.Contains(_config.FontName));
            __instance.fontStyle = (FontStyles)((_config.FontItalic && previouslyItalic ? italicFlag : normalFlag) | (previouslyUppercase ? uppercaseFlag : 0));
            __instance.fontSize *= _config.FontSizeMultiplier;
            __instance.characterSpacing = _config.CharSpacing;
            __instance.wordSpacing = _config.WordSpacingAdjustment;
        }
    }
    
    [HarmonyPatch(typeof(TextMeshPro), "OnEnable")]
    internal class FontPatchTMP
    {
        internal static readonly PluginConfig _config = PluginConfig.Instance;
        
        internal static void Prefix(TextMeshPro __instance)
        {
            if (!__instance.font.name.Contains("Teko-Medium"))
            {
                return;
            }
            
            List<TMP_FontAsset> fontAssets = Managers.FontManager.StandardFonts;

            bool previouslyUppercase = __instance.fontStyle.HasFlag(FontStyles.UpperCase);
            bool previouslyItalic = __instance.fontStyle.HasFlag(FontStyles.Italic);
            int italicFlag = (int)FontStyles.Italic;
            int normalFlag = (int)FontStyles.Normal;
            int uppercaseFlag = (int)FontStyles.UpperCase;
            
            __instance.font = fontAssets.FirstOrDefault(font => font.name.Contains(_config.FontName));
            __instance.fontStyle = (FontStyles)((_config.FontItalic && previouslyItalic ? italicFlag : normalFlag) | (previouslyUppercase ? uppercaseFlag : 0));
            __instance.fontSize *= _config.FontSizeMultiplier;
            __instance.characterSpacing = _config.CharSpacing;
            __instance.wordSpacing = _config.WordSpacingAdjustment;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using FontChanger.Configuration;
using HarmonyLib;
using TMPro;
// ReSharper disable InconsistentNaming

namespace FontChanger.HarmonyPatches
{
    internal abstract class PatcherFunctions
    {
        private static readonly PluginConfig Config = PluginConfig.Instance;
        public static void Patch(TMP_Text instance, List<TMP_FontAsset> fontAssets)
        {
            Plugin.Log.Info(instance.font.name);
            if (!instance.font.name.Contains("Teko-Medium") || !Config.Enabled)
            {
                return;
            }

            bool previouslyUppercase = instance.fontStyle.HasFlag(FontStyles.UpperCase);
            bool previouslyItalic = instance.fontStyle.HasFlag(FontStyles.Italic);

            int styleFlag = (Config.FontItalic && previouslyItalic ? (int)FontStyles.Italic : (int)FontStyles.Normal);
            int caseFlag = (Config.FontUppercase && previouslyUppercase ? (int)FontStyles.UpperCase : 0);
            
            instance.font = fontAssets.FirstOrDefault(font => font.name.Contains(Config.FontName));
            instance.fontStyle = (FontStyles)(styleFlag | caseFlag);
            instance.fontSize *= Config.FontSizeMultiplier;
            instance.fontSizeMin *= Config.FontSizeMultiplier;
            instance.fontSizeMax *= Config.FontSizeMultiplier;
            instance.characterSpacing = Config.CharSpacing;
            instance.wordSpacing = Config.WordSpacingAdjustment;
        }
    }
    
    [HarmonyPatch(typeof(TextMeshPro), "OnEnable")]
    internal class FontPatch
    {
        internal static void Finalizer(TextMeshPro __instance)
        {
            PatcherFunctions.Patch(__instance, Managers.FontManager.StandardFonts);
        }
    }

    [HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable")]
    internal class FontPatchUGUI
    {
        internal static void Finalizer(TextMeshProUGUI __instance)
        {
            PatcherFunctions.Patch(__instance, Managers.FontManager.Fonts);
        }
    }
}
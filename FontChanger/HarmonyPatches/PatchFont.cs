using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeatSaberMarkupLanguage.Components;
using FontChanger.Configuration;
using HarmonyLib;
using IPA.Utilities;
using TMPro;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace FontChanger.HarmonyPatches
{
    internal abstract class PatcherFunctions
    {
        private static readonly PluginConfig Config = PluginConfig.Instance;
        internal static readonly List<KeyValuePair<int, float>> OriginalFontSizes = new List<KeyValuePair<int, float>>();
        internal static readonly List<KeyValuePair<int, float>> OriginalFontSizeMins = new List<KeyValuePair<int, float>>();
        internal static readonly List<KeyValuePair<int, float>> OriginalFontSizeMaxs = new List<KeyValuePair<int, float>>();
        
        public static void Patch(TMP_Text instance, List<TMP_FontAsset> fontAssets)
        {
            if (!Config.Enabled)
            {
                return;
            }
            
            int instanceID = instance.GetInstanceID();
            KeyValuePair<int, float> originalSize = OriginalFontSizes.Find(x => x.Key == instanceID);
            KeyValuePair<int, float> originalSizeMin = OriginalFontSizeMins.Find(x => x.Key == instanceID);
            KeyValuePair<int, float> originalSizeMax = OriginalFontSizeMaxs.Find(x => x.Key == instanceID);

            if (originalSize.Key == 0 && originalSize.Value == 0)
            {
                originalSize = new KeyValuePair<int, float>(instanceID, instance.fontSize);
                OriginalFontSizes.Add(originalSize);

                originalSizeMin = new KeyValuePair<int, float>(instanceID, instance.fontSizeMin);
                OriginalFontSizeMins.Add(originalSizeMin);

                originalSizeMax = new KeyValuePair<int, float>(instanceID, instance.fontSizeMax);
                OriginalFontSizeMaxs.Add(originalSizeMax);
            }

            if (!instance.font.name.Contains("Teko-Medium"))
            {
                // has been patched. stop here
                return;
            }
            
            instance.fontSizeMin = originalSizeMin.Value * Config.FontSizeMultiplier;
            instance.fontSizeMax = originalSizeMax.Value * Config.FontSizeMultiplier;
            instance.fontSize = originalSize.Value * Config.FontSizeMultiplier;

            bool previouslyUppercase = instance.fontStyle.HasFlag(FontStyles.UpperCase);
            bool previouslyItalic = instance.fontStyle.HasFlag(FontStyles.Italic);

            int styleFlag = (Config.FontItalic && previouslyItalic ? (int)FontStyles.Italic : (int)FontStyles.Normal);
            int caseFlag = (Config.FontUppercase && previouslyUppercase ? (int)FontStyles.UpperCase : 0);
            
            instance.font = fontAssets.FirstOrDefault(font => font.name.Contains(Config.FontName));
            instance.fontStyle &= (FontStyles)(styleFlag | caseFlag);
            instance.characterSpacing = Config.CharSpacing;
            instance.wordSpacing = Config.WordSpacingAdjustment;

            if (instance.lineSpacing < 0)
            {
                instance.lineSpacing += (instance.lineSpacing * Config.LineSpacingMultiplier * -1);
            }
            else
            {
                instance.lineSpacing *= Config.LineSpacingMultiplier;
            }
        }
    }

    [HarmonyPatch]
    internal class FontSizePatch
    {
        private static readonly PluginConfig Config = PluginConfig.Instance;
        
        [HarmonyPatch(typeof(TMP_Text), "fontSize", MethodType.Setter)]
        [HarmonyPrefix]
        internal static bool setFontSize(TMP_Text __instance, ref float value)
        {
            int instanceID = __instance.GetInstanceID();
            KeyValuePair<int, float> originalSize = PatcherFunctions.OriginalFontSizes.Find(x => x.Key == instanceID);
            if (originalSize.Key != 0 && originalSize.Value != 0)
            {
                if (!Mathf.Approximately(value, originalSize.Value * Config.FontSizeMultiplier))
                {
                    value *= Config.FontSizeMultiplier;
                }
            }
            return true;
        }
        
        [HarmonyPatch(typeof(TMP_Text), "fontSizeMin", MethodType.Setter)]
        [HarmonyPrefix]
        internal static bool setFontSizeMin(TMP_Text __instance, ref float value)
        {
            int instanceID = __instance.GetInstanceID();
            KeyValuePair<int, float> originalSize = PatcherFunctions.OriginalFontSizeMins.Find(x => x.Key == instanceID);
            if (originalSize.Key != 0 && originalSize.Value != 0 && __instance.autoSizeTextContainer)
            {
                if (!Mathf.Approximately(value, originalSize.Value * Config.FontSizeMultiplier))
                {
                    value *= Config.FontSizeMultiplier;
                }
            }
            return true;
        }
        
        [HarmonyPatch(typeof(TMP_Text), "fontSizeMax", MethodType.Setter)]
        [HarmonyPrefix]
        internal static bool setFontSizeMax(TMP_Text __instance, ref float value)
        {
            int instanceID = __instance.GetInstanceID();
            KeyValuePair<int, float> originalSize = PatcherFunctions.OriginalFontSizeMaxs.Find(x => x.Key == instanceID);
            if (originalSize.Key != 0 && originalSize.Value != 0 && __instance.autoSizeTextContainer)
            {
                if (!Mathf.Approximately(value, originalSize.Value * Config.FontSizeMultiplier))
                {
                    value *= Config.FontSizeMultiplier;
                }
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(TextMeshPro), "OnEnable")]
    internal class FontPatch
    {
        internal static void Finalizer(TextMeshPro __instance)
        {
            int instanceID = __instance.GetInstanceID();
            KeyValuePair<int, float> originalSize = PatcherFunctions.OriginalFontSizes.Find(x => x.Key == instanceID);

            if (originalSize.Key == 0 && originalSize.Value == 0)
            {
                PatcherFunctions.Patch(__instance, Managers.FontManager.StandardFonts);
            }
        }
    }
    
    [HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable")]
    internal class FontPatchUGUI
    {
        internal static void Finalizer(TextMeshProUGUI __instance)
        {
            int instanceID = __instance.GetInstanceID();
            KeyValuePair<int, float> originalSize = PatcherFunctions.OriginalFontSizes.Find(x => x.Key == instanceID);

            if (originalSize.Key == 0 && originalSize.Value == 0)
            {
                PatcherFunctions.Patch(__instance, Managers.FontManager.Fonts);
            }
        }
    }
}
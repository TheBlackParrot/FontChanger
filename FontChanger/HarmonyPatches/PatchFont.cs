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
        internal static readonly List<KeyValuePair<int, float>> ScaledFontSizes = new List<KeyValuePair<int, float>>();
        internal static readonly List<KeyValuePair<int, float>> ScaledFontSizeMins = new List<KeyValuePair<int, float>>();
        internal static readonly List<KeyValuePair<int, float>> ScaledFontSizeMaxs = new List<KeyValuePair<int, float>>();
        
        public static void Patch(TMP_Text instance, List<TMP_FontAsset> fontAssets)
        {
            if (!Config.Enabled)
            {
                return;
            }
            
            int instanceID = instance.GetInstanceID();
            KeyValuePair<int, float> scaledSize = ScaledFontSizes.Find(x => x.Key == instanceID);
            KeyValuePair<int, float> scaledSizeMin = ScaledFontSizeMins.Find(x => x.Key == instanceID);
            KeyValuePair<int, float> scaledSizeMax = ScaledFontSizeMaxs.Find(x => x.Key == instanceID);

            if (scaledSize.Key != 0 && scaledSize.Value != 0)
            {
                // should be 0 when patching the first time, so these won't set
                instance.fontSizeMin = scaledSizeMin.Value;
                instance.fontSizeMax = scaledSizeMax.Value;
                instance.fontSize = scaledSize.Value;
            }

            if (!instance.font.name.Contains("Teko-Medium"))
            {
                // has been patched. stop here
                return;
            }
            
            if (scaledSize.Key == 0 && scaledSize.Value == 0)
            {
                // ok now set the values, since we haven't done that yet
                scaledSize = new KeyValuePair<int, float>(instanceID, instance.fontSize * Config.FontSizeMultiplier);
                ScaledFontSizes.Add(scaledSize);

                scaledSizeMin = new KeyValuePair<int, float>(instanceID, instance.fontSizeMin * Config.FontSizeMultiplier);
                ScaledFontSizeMins.Add(scaledSizeMin);

                scaledSizeMax = new KeyValuePair<int, float>(instanceID, instance.fontSizeMax * Config.FontSizeMultiplier);
                ScaledFontSizeMaxs.Add(scaledSizeMax);
                
                instance.fontSizeMin = scaledSizeMin.Value;
                instance.fontSizeMax = scaledSizeMax.Value;
                instance.fontSize = scaledSize.Value;
            }

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
            KeyValuePair<int, float> scaledSize = PatcherFunctions.ScaledFontSizes.Find(x => x.Key == instanceID);
            if (scaledSize.Key != 0 && scaledSize.Value != 0)
            {
                if (Mathf.Approximately(value, scaledSize.Value))
                {
                    value = scaledSize.Value;
                }
                else
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
            KeyValuePair<int, float> scaledSize = PatcherFunctions.ScaledFontSizeMins.Find(x => x.Key == instanceID);
            if (scaledSize.Key != 0 && scaledSize.Value != 0 && __instance.autoSizeTextContainer)
            {
                if (Mathf.Approximately(value, scaledSize.Value))
                {
                    value = scaledSize.Value;
                }
                else
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
            KeyValuePair<int, float> scaledSize = PatcherFunctions.ScaledFontSizeMaxs.Find(x => x.Key == instanceID);
            if (scaledSize.Key != 0 && scaledSize.Value != 0 && __instance.autoSizeTextContainer)
            {
                if (Mathf.Approximately(value, scaledSize.Value))
                {
                    value = scaledSize.Value;
                }
                else
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
            KeyValuePair<int, float> scaledSize = PatcherFunctions.ScaledFontSizes.Find(x => x.Key == instanceID);

            if (scaledSize.Key == 0 && scaledSize.Value == 0)
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
            KeyValuePair<int, float> scaledSize = PatcherFunctions.ScaledFontSizes.Find(x => x.Key == instanceID);

            if (scaledSize.Key == 0 && scaledSize.Value == 0)
            {
                PatcherFunctions.Patch(__instance, Managers.FontManager.Fonts);
            }
        }
    }
}
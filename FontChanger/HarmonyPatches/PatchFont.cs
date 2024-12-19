using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeatSaberMarkupLanguage.Components;
using FontChanger.Classes;
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
        internal static readonly List<KeyValuePair<int, OriginalValues>> valuesList = new List<KeyValuePair<int, OriginalValues>>();
        
        public static void Patch(TMP_Text instance, List<TMP_FontAsset> fontAssets)
        {
            if (!Config.Enabled || !instance.font.name.Contains("Teko-Medium"))
            {
                return;
            }
            
            int instanceID = instance.GetInstanceID();
            if (!valuesList.Exists(x => x.Key == instanceID))
            {
                valuesList.Add(new KeyValuePair<int, OriginalValues>(instanceID, new OriginalValues(instanceID, instance.fontSize, instance.fontSizeMin, instance.fontSizeMax, instance.fontStyle, instance.lineSpacing)));
            }
            OriginalValues values = valuesList.Find(x => x.Key == instanceID).Value;
            
            instance.fontSizeMin = values.FontSizeMin * Config.FontSizeMultiplier;
            instance.fontSizeMax = values.FontSizeMax * Config.FontSizeMultiplier;
            instance.fontSize = values.FontSize * Config.FontSizeMultiplier;

            bool previouslyUppercase = values.FontStyle.HasFlag(FontStyles.UpperCase);
            bool previouslyItalic = values.FontStyle.HasFlag(FontStyles.Italic);

            int styleFlag = (Config.FontItalic && previouslyItalic ? (int)FontStyles.Italic : (int)FontStyles.Normal);
            int caseFlag = (Config.FontUppercase && previouslyUppercase ? (int)FontStyles.UpperCase : 0);
            
            instance.font = fontAssets.FirstOrDefault(font => font.name.Contains(Config.FontName));
            instance.fontStyle = values.FontStyle & (FontStyles)(styleFlag | caseFlag);
            instance.characterSpacing = Config.CharSpacing;
            instance.wordSpacing = Config.WordSpacingAdjustment;

            if (instance.lineSpacing < 0)
            {
                instance.lineSpacing = (values.LineSpacing * Config.LineSpacingMultiplier * -1);
            }
            else
            {
                instance.lineSpacing = values.LineSpacing * Config.LineSpacingMultiplier;
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
            OriginalValues values = PatcherFunctions.valuesList.Find(x => x.Key == instanceID).Value;
            
            if (values != null)
            {
                if (!Mathf.Approximately(value, values.FontSize * Config.FontSizeMultiplier))
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
            OriginalValues values = PatcherFunctions.valuesList.Find(x => x.Key == instanceID).Value;
            
            if (values != null && __instance.autoSizeTextContainer)
            {
                if (!Mathf.Approximately(value, values.FontSizeMin * Config.FontSizeMultiplier))
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
            OriginalValues values = PatcherFunctions.valuesList.Find(x => x.Key == instanceID).Value;
            
            if (values != null && __instance.autoSizeTextContainer)
            {
                if (!Mathf.Approximately(value, values.FontSizeMax * Config.FontSizeMultiplier))
                {
                    value *= Config.FontSizeMultiplier;
                }
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable")]
    internal class FontPatchUGUI
    {
        internal static void Finalizer(TextMeshProUGUI __instance)
        {
            PatcherFunctions.Patch(__instance, Managers.FontManager.Fonts);
            /*
            int instanceID = __instance.GetInstanceID();
            KeyValuePair<int, float> originalSize = PatcherFunctions.OriginalFontSizes.Find(x => x.Key == instanceID);

            if ((originalSize.Key == 0 && originalSize.Value == 0) || !__instance.material.name.Contains("Curved"))
            {
                // for some reason that last part of the conditional fixes tooltips. idk either
                PatcherFunctions.Patch(__instance, Managers.FontManager.Fonts);
            }
            */
        }
    }
    
    [HarmonyPatch(typeof(TextMeshPro), "OnEnable")]
    internal class FontPatch
    {
        internal static void Finalizer(TextMeshPro __instance)
        {
            PatcherFunctions.Patch(__instance, Managers.FontManager.StandardFonts);
            /*
            int instanceID = __instance.GetInstanceID();
            KeyValuePair<int, float> originalSize = PatcherFunctions.OriginalFontSizes.Find(x => x.Key == instanceID);

            if (originalSize.Key == 0 && originalSize.Value == 0)
            {
                PatcherFunctions.Patch(__instance, Managers.FontManager.StandardFonts);
            }
            */
        }
    }
}
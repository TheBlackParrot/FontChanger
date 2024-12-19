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
using HMUI;
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
                valuesList.Add(new KeyValuePair<int, OriginalValues>(instanceID, new OriginalValues(instance)));
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
            
            instance.SetAllDirty();
        }
    }

    [HarmonyPatch]
    internal class FontSizePatch
    {
        private static readonly PluginConfig Config = PluginConfig.Instance;

        private static OriginalValues GetValueList(TMP_Text instance)
        {
            int instanceID = instance.GetInstanceID();
            KeyValuePair<int, OriginalValues> list = PatcherFunctions.valuesList.Find(x => x.Key == instanceID);
            if (list.Key != 0)
            {
                list = new KeyValuePair<int, OriginalValues>(instanceID, new OriginalValues(instance));
                PatcherFunctions.valuesList.Add(list);
            }

            return list.Value;
        }
        
        [HarmonyPatch(typeof(TMP_Text), "fontSize", MethodType.Setter)]
        [HarmonyPriority(int.MinValue)]
        [HarmonyPrefix]
        internal static bool setFontSize(TMP_Text __instance, ref float value)
        {
            OriginalValues values = GetValueList(__instance);
            
            if (values != null)
            {
                if (Mathf.Approximately(value, values.FontSize))
                {
                    value *= Config.FontSizeMultiplier;
                }
                else if (!Mathf.Approximately(value, values.FontSize * Config.FontSizeMultiplier) && value > 0)
                {
                    Plugin.Log.Info($"{__instance.name} ({values.InstanceID}) now wants a font size of {value}");
                    values.FontSize = value;
                    value *= Config.FontSizeMultiplier;
                }
            }
            return true;
        }
        
        [HarmonyPatch(typeof(TMP_Text), "fontSizeMin", MethodType.Setter)]
        [HarmonyPriority(int.MinValue)]
        [HarmonyPrefix]
        internal static bool setFontSizeMin(TMP_Text __instance, ref float value)
        {
            OriginalValues values = GetValueList(__instance);
            
            if (values != null && __instance.autoSizeTextContainer)
            {
                if (Mathf.Approximately(value, values.FontSizeMin))
                {
                    value *= Config.FontSizeMultiplier;
                }
                else if (!Mathf.Approximately(value, values.FontSizeMin * Config.FontSizeMultiplier) && value > 0)
                {
                    Plugin.Log.Info($"{__instance.name} ({values.InstanceID}) now wants a minimum font size of {value}");
                    values.FontSizeMin = value;
                    value *= Config.FontSizeMultiplier;
                }
            }
            return true;
        }
        
        [HarmonyPatch(typeof(TMP_Text), "fontSizeMax", MethodType.Setter)]
        [HarmonyPriority(int.MinValue)]
        [HarmonyPrefix]
        internal static bool setFontSizeMax(TMP_Text __instance, ref float value)
        {
            OriginalValues values = GetValueList(__instance);
            
            if (values != null && __instance.autoSizeTextContainer)
            {
                if (Mathf.Approximately(value, values.FontSizeMax))
                {
                    value *= Config.FontSizeMultiplier;
                }
                else if (!Mathf.Approximately(value, values.FontSizeMax * Config.FontSizeMultiplier) && value > 0)
                {
                    Plugin.Log.Info($"{__instance.name} ({values.InstanceID}) now wants a maximum font size of {value}");
                    values.FontSizeMax = value;
                    value *= Config.FontSizeMultiplier;
                }
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(TMP_Text), "text", MethodType.Setter)]
    [HarmonyPriority(int.MinValue)]
    internal class FontPatch
    {
        private static readonly Type[] CurvedTypes = {
            typeof(CurvedTextMeshPro),
            typeof(FormattableText),
            typeof(ClickableText)
        };
        private static readonly Type[] StandardTypes =
        {
            typeof(TextMeshPro)
        };
        
        internal static void Finalizer(TMP_Text __instance)
        {
            //Plugin.Log.Info($"{__instance.name} ({__instance.GetInstanceID()}) -- {__instance.GetType()}");
            if (__instance.fontSize <= 0)
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
                Plugin.Log.Info($"{__instance.name} ({__instance.GetInstanceID()}) -- {__instance.GetType()}");
            }
        }
    }
    
    /*
    [HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable")]
    [HarmonyPriority(int.MinValue)]
    internal class FontPatchUGUI
    {
        internal static void Finalizer(TextMeshProUGUI __instance)
        {
            PatcherFunctions.Patch(__instance, Managers.FontManager.Fonts);
        }
    }
    
    [HarmonyPatch(typeof(TextMeshPro), "OnEnable")]
    [HarmonyPriority(int.MinValue)]
    internal class FontPatch
    {
        internal static void Finalizer(TextMeshPro __instance)
        {
            PatcherFunctions.Patch(__instance, Managers.FontManager.StandardFonts);
        }
    }
    */
}
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
        private static readonly TMP_FontAsset TekoFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().LastOrDefault(f2 => f2.name == "Teko-Medium SDF");

        public static void Unpatch(TMP_Text instance)
        {
            int instanceID = instance.GetInstanceID();
            OriginalValues values = valuesList.Find(x => x.Key == instanceID).Value;
            if (values == null)
            {
                return;
            }
            
            if (instance.font.name.Contains(Config.FontName))
            {
                instance.font = TekoFont;
                instance.fontSizeMin = values.FontSizeMin;
                instance.fontSizeMax = values.FontSizeMax;
                instance.fontSize = values.FontSize;
                instance.fontStyle = values.FontStyle;
                instance.lineSpacing = values.LineSpacing;
                instance.wordSpacing = 0;
                instance.characterSpacing = 0;
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
            instance.lineSpacing = values.LineSpacing * Config.LineSpacingMultiplier;
            
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
                if (!Mathf.Approximately(value, values.FontSize * Config.FontSizeMultiplier) && !Mathf.Approximately(value, values.FontSize) && value > 0)
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
            
            if (values != null)
            {
                if (!Mathf.Approximately(value, values.FontSizeMin * Config.FontSizeMultiplier) && !Mathf.Approximately(value, values.FontSizeMin) && value > 0)
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
            
            if (values != null)
            {
                if (!Mathf.Approximately(value, values.FontSizeMax * Config.FontSizeMultiplier) && !Mathf.Approximately(value, values.FontSizeMax) && value > 0)
                {
                    Plugin.Log.Info($"{__instance.name} ({values.InstanceID}) now wants a maximum font size of {value}");
                    values.FontSizeMax = value;
                    value *= Config.FontSizeMultiplier;
                }
            }
            
            return true;
        }

        [HarmonyPatch(typeof(TMP_Text), "fontStyle", MethodType.Setter)]
        [HarmonyPriority(int.MinValue)]
        [HarmonyPrefix]
        internal static bool setFontStyle(TMP_Text __instance, ref FontStyles value)
        {
            OriginalValues values = GetValueList(__instance);

            if (values != null)
            {
                bool previouslyUppercase = value.HasFlag(FontStyles.UpperCase);
                bool previouslyItalic = value.HasFlag(FontStyles.Italic);

                int styleFlag = (Config.FontItalic && previouslyItalic ? (int)FontStyles.Italic : (int)FontStyles.Normal);
                int caseFlag = (Config.FontUppercase && previouslyUppercase ? (int)FontStyles.UpperCase : 0);
                
                FontStyles style = value & (FontStyles)(styleFlag | caseFlag);
                
                values.FontStyle = value;
                value = style;
            }

            return true;
        }

        [HarmonyPatch(typeof(TMP_Text), "lineSpacing", MethodType.Setter)]
        [HarmonyPriority(int.MinValue)]
        [HarmonyPrefix]
        internal static bool setLineSpacing(TMP_Text __instance, ref float value)
        {
            OriginalValues values = GetValueList(__instance);
            
            if (values != null)
            {
                if (!Mathf.Approximately(value, values.LineSpacing * Config.LineSpacingMultiplier) && !Mathf.Approximately(value, values.LineSpacing))
                {
                    Plugin.Log.Info($"{__instance.name} ({values.InstanceID}) now wants a line spacing value of {value}");
                    values.LineSpacing = value;
                    value *= Config.LineSpacingMultiplier;
                }
            }
            
            return true;
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
        private static readonly PluginConfig Config = PluginConfig.Instance;
        
        internal static void Finalizer(TMP_Text __instance)
        {
            if (!Config.Enabled || __instance == null)
            {
                return;
            }
            if (__instance.fontSize <= 0 || !__instance.isActiveAndEnabled)
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
}
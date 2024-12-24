using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage.Tags.Settings;
using BeatSaberMarkupLanguage.TypeHandlers;
using FontChanger.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace FontChanger.HarmonyPatches
{
    internal class BSMLPatches
    {
        [HarmonyPatch]
        internal class TextTagPatch
        {
            [HarmonyPatch(typeof(TextTag), "CreateObject")]
            [HarmonyPriority(int.MinValue)]
            [HarmonyFinalizer]
            internal static void Patch(ref GameObject __result)
            {
                PatcherFunctions.Patch(__result.GetComponent<FormattableText>(), Managers.FontManager.Fonts);
            }
            
            [HarmonyPatch(typeof(ClickableTextTag), "CreateObject")]
            [HarmonyPatch(typeof(SubmenuTag), "CreateObject")]
            [HarmonyPriority(int.MinValue)]
            [HarmonyFinalizer]
            internal static void ClickablePatch(ref GameObject __result)
            {
                PatcherFunctions.Patch(__result.GetComponent<ClickableText>(), Managers.FontManager.Fonts);
            }
            
            [HarmonyPatch(typeof(FormattableText), "RefreshText")]
            [HarmonyPriority(int.MinValue)]
            [HarmonyFinalizer]
            internal static void FormattableTextPatch(FormattableText __instance)
            {
                PatcherFunctions.Patch(__instance, Managers.FontManager.Fonts, true);
            }

            /*
            // this crashes
            [HarmonyPatch(typeof(TextMeshProUGUIHandler), "Setters", MethodType.Constructor)]
            [HarmonyPriority(int.MinValue)]
            [HarmonyAfter("com.monkeymanboy.BeatSaberMarkupLanguage")]
            [HarmonyPostfix]
            internal static void PatchValue(Dictionary<string, Action<TextMeshProUGUI, string>> __instance)
            {
                Plugin.Log.Info(__instance.Count.ToString());
                
                foreach (KeyValuePair<string,Action<TextMeshProUGUI,string>> keyValuePair in __instance)
                {
                    Plugin.Log.Info($"{keyValuePair.Key}");
                }

                Plugin.Log.Info("triggered");
            }
            */
        }
    }
}
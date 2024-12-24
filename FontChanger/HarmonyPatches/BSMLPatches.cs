using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage.Tags.Settings;
using BeatSaberMarkupLanguage.TypeHandlers;
using BeatSaberMarkupLanguage.ViewControllers;
using FontChanger.Configuration;
using HarmonyLib;
using HMUI;
using TMPro;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace FontChanger.HarmonyPatches
{
    internal abstract class BSMLPatches
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

        [HarmonyPatch]
        internal class TextTagPatch
        {
            /*[HarmonyPatch(typeof(TextTag), "CreateObject")]
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
            }*/

            [HarmonyPatch(typeof(FormattableText), MethodType.Constructor)]
            [HarmonyPriority(int.MinValue)]
            [HarmonyFinalizer]
            internal static void FormattableTextAddDestroyerEvent(FormattableText __instance)
            {
                __instance.Destroyed += (sender, args) => { PatcherFunctions.Unpatch(__instance); };
            }

            [HarmonyPatch(typeof(FormattableText), "RefreshText")]
            [HarmonyPriority(int.MinValue)]
            [HarmonyFinalizer]
            internal static void FormattableTextPatch(FormattableText __instance)
            {
                PatcherFunctions.Patch(__instance, Managers.FontManager.Fonts, true);
            }
        }
        
        [HarmonyPatch]
        internal class ViewControllerPatch
        {
            [HarmonyPatch(typeof(BSMLViewController), "ParseWithFallback")]
            [HarmonyPriority(int.MinValue)]
            [HarmonyFinalizer]
            internal static void Patch(BSMLViewController __instance)
            {
                var components = __instance.GetComponentsInChildren<TMP_Text>();
                foreach (var component in components)
                {
                    Type type = component.GetType();
            
                    if (CurvedTypes.Contains(type))
                    {
                        PatcherFunctions.Patch(component, Managers.FontManager.Fonts);
                    }
                    else if (StandardTypes.Contains(type))
                    {
                        PatcherFunctions.Patch(component, Managers.FontManager.StandardFonts);
                    }
                    else
                    {
                        Plugin.Log.Debug($"{component.gameObject.name} ({component.GetInstanceID()}) -- {component.GetType()}");
                    }
                }
            }
        }
    }
}
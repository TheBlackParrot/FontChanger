using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.Util;
using IPA;
using FontChanger.Configuration;
using FontChanger.HarmonyPatches;
using HarmonyLib;
using IPA.Config.Data;
using IPA.Config.Stores;
using TMPro;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using Config = IPA.Config.Config;

namespace FontChanger
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        internal static PluginConfig Config { get; private set; }
        private static Harmony harmony;
        
        [Init]
        public void Init(Config conf, IPALogger logger) {
            Instance = this;
            Log = logger;
            
            PluginConfig.Instance = Config = conf.Generated<PluginConfig>();
        }
        
        [OnStart]
        public void OnApplicationStart()
        {
            MainMenuAwaiter.MainMenuInitializing += DoPatching;
        }

        [OnEnable]
        public void OnEnable()
        {
            harmony = new Harmony("TheBlackParrot.FontChanger");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void DoPatching()
        {
            Managers.FontManager.FirstTimeFontLoad();
            BSMLSettings.Instance.AddSettingsMenu("FontChanger", "FontChanger.UI.BSML.Settings.bsml", Config);
        }

        [OnDisable]
        public void OnDisable()
        {
            harmony.UnpatchSelf();
            
            TMP_FontAsset TekoFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().LastOrDefault(f2 => f2.name == "Teko-Medium SDF");
            TMP_Text[] textObjs = Resources.FindObjectsOfTypeAll<TMP_Text>();
            
            PatcherFunctions.valuesList.ForEach(pair =>
            {
                if (Resources.InstanceIDIsValid(pair.Key))
                {
                    TMP_Text foundObj = textObjs.FirstOrDefault(obj => obj.GetInstanceID() == pair.Key);
                    if (foundObj != null)
                    {
                        if (foundObj.font.name.Contains(Config.FontName))
                        {
                            foundObj.font = TekoFont;
                            foundObj.fontSizeMin = pair.Value.FontSizeMin;
                            foundObj.fontSizeMax = pair.Value.FontSizeMax;
                            foundObj.fontSize = pair.Value.FontSize;
                            foundObj.fontStyle = pair.Value.FontStyle;
                            foundObj.lineSpacing = pair.Value.LineSpacing;
                            foundObj.wordSpacing = 0;
                            foundObj.characterSpacing = 0;
                        }
                    }
                }
            });
            
            PatcherFunctions.valuesList.Clear();
        }

        [OnExit]
        public void OnApplicationQuit() {
            harmony.UnpatchSelf();
        }
    }
}
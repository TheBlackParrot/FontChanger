using System.Reflection;
using BeatSaberMarkupLanguage.Util;
using IPA;
using FontChanger.Configuration;
using HarmonyLib;
using IPA.Config.Stores;
using IPALogger = IPA.Logging.Logger;
using Config = IPA.Config.Config;

namespace FontChanger
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        private static Harmony harmony;
        
        [Init]
        public void Init(Config conf, IPALogger logger) {
            Instance = this;
            Log = logger;
            
            PluginConfig.Instance = conf.Generated<PluginConfig>();
        }
        
        [OnStart]
        async public void OnApplicationStart()
        {
            MainMenuAwaiter.MainMenuInitializing += DoPatching;
            
            await MainMenuAwaiter.WaitForMainMenuAsync();
            DoPatching();
        }

        private static void DoPatching()
        {
            Managers.FontManager.FirstTimeFontLoad();
            
            harmony = new Harmony("TheBlackParrot.FontChanger");
            harmony.PatchAll(Assembly.GetExecutingAssembly()); 
        }

        [OnExit]
        public void OnApplicationQuit() {
            harmony.UnpatchSelf();
        }
    }
}
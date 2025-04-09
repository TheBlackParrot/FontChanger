using System.Reflection;
using FontChanger.Configuration;
using FontChanger.Installers;
using HarmonyLib;
using IPA;
using IPA.Config.Stores;
using IPAConfig = IPA.Config.Config;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace FontChanger;

[Plugin(RuntimeOptions.DynamicInit)]
internal class Plugin
{
    private static Harmony _harmony = null!;
    internal static IPALogger Log { get; set; } = null!;

    [Init]
    public Plugin(IPALogger logger, IPAConfig ipaConfig, Zenjector zenjector)
    {
        Log = logger;
        zenjector.UseLogger(Log);
        
        PluginConfig c = ipaConfig.Generated<PluginConfig>();
        PluginConfig.Instance = c;
        
        zenjector.Install<MenuInstaller>(Location.Menu);
        
        Log.Info("Plugin loaded");
    }

    [OnEnable]
    public void OnEnable()
    {
        _harmony = new Harmony("TheBlackParrot.FontChanger");
        _harmony.PatchAll(Assembly.GetExecutingAssembly());
        
        Log.Info("Patches applied");
    }
    
    [OnDisable]
    public void OnDisable()
    {
        _harmony.UnpatchSelf();
    }
}
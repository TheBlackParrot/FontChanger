using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using FontChanger.Configuration;
using JetBrains.Annotations;
using Zenject;

namespace FontChanger.UI;

[UsedImplicitly]
internal class SettingsMenuManager : IInitializable, IDisposable
{
    private static PluginConfig Config => PluginConfig.Instance;
    
    public void Initialize()
    {
        Managers.FontManager.FirstTimeFontLoad();
        
#if V1_39_1
        BeatSaberMarkupLanguage.Settings.BSMLSettings.Instance.AddSettingsMenu("FontChanger", "FontChanger.UI.BSML.Settings.bsml", this);
#else
        BeatSaberMarkupLanguage.Settings.BSMLSettings.instance?.AddSettingsMenu("FontChanger", "FontChanger.UI.BSML.Settings.bsml", this);
#endif
    }

    public void Dispose()
    {
#if V1_39_1
        BeatSaberMarkupLanguage.Settings.BSMLSettings.Instance?.RemoveSettingsMenu(this);
#else
        BeatSaberMarkupLanguage.Settings.BSMLSettings.instance?.RemoveSettingsMenu(this);
#endif
    }

    protected bool Enabled
    {
        get => Config.Enabled;
        set => Config.Enabled = value;
    }

    protected string FontName
    {
        get => Config.FontName;
        set => Config.FontName = value;
    }

    protected bool ForceDisableItalic
    {
        get => Config.ForceDisableItalic;
        set => Config.ForceDisableItalic = value;
    }
    
    [UIValue("font-choices")]
    internal static List<object> FontChoices = [];
}
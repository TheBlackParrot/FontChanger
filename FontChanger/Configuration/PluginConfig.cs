using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage.Attributes;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace FontChanger.Configuration;

internal class PluginConfig
{
    public static PluginConfig Instance { get; set; } = null!;
    
    public virtual bool Enabled { get; set; } = true;
    public virtual string FontName { get; set; } = string.Empty;
}

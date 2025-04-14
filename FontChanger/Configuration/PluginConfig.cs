using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using JetBrains.Annotations;

// ReSharper disable RedundantDefaultMemberInitializer

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace FontChanger.Configuration;

[UsedImplicitly]
internal class PluginConfig
{
    public static PluginConfig Instance { get; set; } = null!;
    
    public virtual bool Enabled { get; set; } = true;
    public virtual string FontName { get; set; } = string.Empty;
    public virtual bool ForceDisableItalic { get; set; } = false;
    public virtual bool ForceDisableCapitalize { get; set; } = false;
    public virtual bool FixSomeUIThings { get; set; } = false;
}

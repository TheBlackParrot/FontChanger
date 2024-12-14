using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace FontChanger.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        
        public virtual string FontName { get; set; } = "Freeman";
        public virtual bool FontItalic { get; set; } = true;
        public virtual float FontSizeMultiplier { get; set; } = 0.77f;
        public virtual float CharSpacing { get; set; } = -1.5f;
        public virtual float WordSpacingAdjustment { get; set; } = 2.5f;
    }
}
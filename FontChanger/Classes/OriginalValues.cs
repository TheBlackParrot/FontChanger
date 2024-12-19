using TMPro;

namespace FontChanger.Classes
{
    public class OriginalValues
    {
        internal readonly int InstanceID;
        internal float FontSize;
        internal float FontSizeMin;
        internal float FontSizeMax;
        internal FontStyles FontStyle;
        internal float LineSpacing;

        public OriginalValues(TMP_Text instance)
        {
            InstanceID = instance.GetInstanceID();
            FontSize = instance.fontSize;
            FontSizeMin = instance.fontSizeMin;
            FontSizeMax = instance.fontSizeMax;
            FontStyle = instance.fontStyle;
            LineSpacing = instance.lineSpacing;
        }
    }
}
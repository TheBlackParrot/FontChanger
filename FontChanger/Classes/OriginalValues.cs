using TMPro;

namespace FontChanger.Classes
{
    public class OriginalValues
    {
        internal readonly int InstanceID;
        internal readonly float FontSize;
        internal readonly float FontSizeMin;
        internal readonly float FontSizeMax;
        internal readonly FontStyles FontStyle;
        internal readonly float LineSpacing;

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
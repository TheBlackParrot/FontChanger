using TMPro;

namespace FontChanger.Classes
{
    internal class OriginalValues
    {
        public int InstanceID { get; }
        public float FontSize { get; set; }
        public float FontSizeMin { get; set; }
        public float FontSizeMax { get; set; }
        public FontStyles FontStyle { get; set; }
        public float LineSpacing { get; set; }

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
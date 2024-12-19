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

        public OriginalValues(int instanceID, float size, float min, float max, FontStyles style, float lineSpacing)
        {
            InstanceID = instanceID;
            FontSize = size;
            FontSizeMin = min;
            FontSizeMax = max;
            FontStyle = style;
            LineSpacing = lineSpacing;
        }
    }
}
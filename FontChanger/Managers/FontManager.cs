using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using BeatSaberMarkupLanguage;
using IPA.Utilities;
using TMPro;
using UnityEngine.TextCore.LowLevel;

namespace FontChanger.Managers
{
    class FontManager
    {
        public static GameObject Prefab;
        private static TMP_FontAsset Font;
        public static List<TMP_FontAsset> Fonts;
        public static List<TMP_FontAsset> StandardFonts;
        private static Material _cachedCurvedMaterial;
        private static Material _cachedStandardMaterial;

        private static TMP_FontAsset LoadFromTTF(string path, bool curved = true)
        {
            var fnt = new Font(path);
            var settings = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().LastOrDefault(f2 => f2.name == "Teko-Medium SDF").creationSettings;
            Enum.TryParse(settings.renderMode.ToString(), out GlyphRenderMode renderMode);
            
            Font = TMP_FontAsset.CreateFontAsset(fnt, settings.pointSize, settings.padding, renderMode, settings.atlasWidth, settings.atlasHeight);
            Font.SetName(Path.GetFileNameWithoutExtension(path) + (curved ? "-Curved" : "-Standard"));

            Material matCopy;
            if (curved)
            {
                matCopy = UnityEngine.Object.Instantiate(_cachedCurvedMaterial);
            }
            else
            {
                matCopy = UnityEngine.Object.Instantiate(_cachedStandardMaterial);
            }
            matCopy.mainTexture = Font.material.mainTexture;
            matCopy.mainTextureOffset = Font.material.mainTextureOffset;
            matCopy.mainTextureScale = Font.material.mainTextureScale;
            Font.material = matCopy;
            
            Font = UnityEngine.Object.Instantiate(Font);
            return Font;
        }
        
        public static void FirstTimeFontLoad()
        {
            _cachedCurvedMaterial = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(mat => mat.name.Contains("Teko-Medium SDF Curved"));
            _cachedStandardMaterial = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(mat =>
                mat.name.Contains("Teko-Medium SDF") && !mat.name.Contains("Curved"));
            
            List<TMP_FontAsset> fonts = new List<TMP_FontAsset>();
            List<TMP_FontAsset> standardFonts = new List<TMP_FontAsset>();

            string[] files = Directory.GetFiles(Path.Combine(UnityGame.UserDataPath,"FontChanger","Fonts"));
            List<string> TTFs = new List<string>();
            foreach (var file in files)
            {
                if (file.EndsWith(".ttf") || file.EndsWith(".otf"))
                {
                    TTFs.Add(file);
                }
            }
            foreach (var ttf in TTFs)
            {
                fonts.Add(LoadFromTTF(ttf));
                standardFonts.Add(LoadFromTTF(ttf, false));
            }
            Fonts = fonts;
            StandardFonts = standardFonts;

            Plugin.Log.Info("Font loading complete");
        }
    }
}
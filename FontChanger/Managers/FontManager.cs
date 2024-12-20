﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using System.IO;
using BeatSaberMarkupLanguage;
using IPA.Utilities;
using ModestTree;
using TMPro;
using UnityEngine.TextCore.LowLevel;

namespace FontChanger.Managers
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    internal class FontManager
    {
        private static TMP_FontAsset Font;
        public static readonly List<TMP_FontAsset> Fonts = new List<TMP_FontAsset>();
        public static readonly List<TMP_FontAsset> StandardFonts = new List<TMP_FontAsset>();
        private static Material _cachedCurvedMaterial;
        private static Material _cachedStandardMaterial;

        private static TMP_FontAsset LoadFromTTF(string path, bool curved = true)
        {
            // ReSharper disable once PossibleNullReferenceException
            // this will never be null, shush
            FontAssetCreationSettings settings = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().LastOrDefault(f2 => f2.name == "Teko-Medium SDF").creationSettings;
            Font fnt = new Font(path);
            Enum.TryParse(settings.renderMode.ToString(), out GlyphRenderMode renderMode);
            
            Font = TMP_FontAsset.CreateFontAsset(fnt, (int)Math.Round(settings.pointSize/1.5), settings.padding, renderMode, settings.atlasWidth, settings.atlasHeight);
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
            Plugin.Config.FontChoices.Clear();
            
            _cachedCurvedMaterial = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(mat => mat.name.Contains("Teko-Medium SDF Curved"));
            _cachedStandardMaterial = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(mat =>
                mat.name.Contains("Teko-Medium SDF") && !mat.name.Contains("Curved"));

            string[] files = Directory.GetFiles(Path.Combine(UnityGame.UserDataPath, "FontChanger", "Fonts"));
            foreach (string file in files)
            {
                if (file.EndsWith(".ttf") || file.EndsWith(".otf"))
                {
                    Log.Info("Adding font " + Path.GetFileName(file));
                    
                    Fonts.Add(LoadFromTTF(file));
                    StandardFonts.Add(LoadFromTTF(file, false));
                    
                    Plugin.Config.FontChoices.Add(Path.GetFileNameWithoutExtension(file));
                }
            }

            Plugin.Log.Info("Font loading complete");
        }
    }
}
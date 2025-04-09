using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatSaberMarkupLanguage;
using FontChanger.Configuration;
using FontChanger.UI;
using IPA.Utilities;
using JetBrains.Annotations;
using ModestTree;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using Font = UnityEngine.Font;

namespace FontChanger.Managers;

internal static class FontLists
{
    internal static readonly List<TMP_FontAsset> Fonts = [];
    internal static readonly List<TMP_FontAsset> StandardFonts = [];
}

[UsedImplicitly]
internal class FontManager
{
    private static TMP_FontAsset _font = null!;
    internal static List<TMP_FontAsset> Fonts => FontLists.Fonts;
    internal static List<TMP_FontAsset> StandardFonts => FontLists.StandardFonts;
    private static Material _cachedCurvedMaterial = null!;
    private static Material _cachedStandardMaterial = null!;
    
    private static PluginConfig Config => PluginConfig.Instance;

    private static TMP_FontAsset LoadFromTTF(string path, bool curved = true)
    {
        // ReSharper disable once PossibleNullReferenceException
        // this will never be null, shush
        FontAssetCreationSettings settings = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().LastOrDefault(f2 => f2.name == "Teko-Medium SDF").creationSettings;
        Font fnt = new(path);
        Enum.TryParse(settings.renderMode.ToString(), out GlyphRenderMode renderMode);
            
        _font = TMP_FontAsset.CreateFontAsset(fnt, (int)Math.Round(settings.pointSize/1.5), settings.padding, renderMode, settings.atlasWidth, settings.atlasHeight);
        _font.SetName(Path.GetFileNameWithoutExtension(path) + (curved ? "-Curved" : "-Standard"));

        Material matCopy = UnityEngine.Object.Instantiate(curved ? _cachedCurvedMaterial : _cachedStandardMaterial);
        matCopy.mainTexture = _font.material.mainTexture;
        matCopy.mainTextureOffset = _font.material.mainTextureOffset;
        matCopy.mainTextureScale = _font.material.mainTextureScale;
        _font.material = matCopy;
            
        _font = UnityEngine.Object.Instantiate(_font);
        return _font;
    }
        
    public static void FirstTimeFontLoad()
    {
        SettingsMenuManager.FontChoices.Clear();
            
        _cachedCurvedMaterial = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(mat => mat.name.Contains("Teko-Medium SDF Curved"))!;
        _cachedStandardMaterial = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(mat =>
            mat.name.Contains("Teko-Medium SDF") && !mat.name.Contains("Curved"))!;

        string[] files = Directory.GetFiles(Path.Combine(UnityGame.UserDataPath, "FontChanger", "Fonts"));
        foreach (string file in files)
        {
            if (!file.EndsWith(".ttf") && !file.EndsWith(".otf"))
            {
                continue;
            }
            
            string objectName = Path.GetFileNameWithoutExtension(file) + "-Curved(Clone)";
            if (Fonts.FindIndex(x => x.name == objectName) != -1)
            {
                Log.Info("Font " + Path.GetFileName(file) + " has already been loaded");
            }
            else
            {
                Log.Info("Adding font " + Path.GetFileName(file));
                    
                Fonts.Add(LoadFromTTF(file));
                StandardFonts.Add(LoadFromTTF(file, false));   
            }
                    
            SettingsMenuManager.FontChoices.Add(Path.GetFileNameWithoutExtension(file));
        }

        Plugin.Log.Info("Font loading complete");
    }
}
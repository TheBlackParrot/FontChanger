﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage.Attributes;
using HarmonyLib;
using IPA.Config.Stores;
using TMPro;
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace FontChanger.Configuration
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public string PercentageFormatter(float x) => x.ToString("0%");
        
        public virtual bool Enabled { get; set; } = true;
        public virtual string FontName { get; set; } = "SairaSemiCondensed-SemiBold";
        public virtual bool FontItalic { get; set; } = true;
        public virtual bool FontUppercase { get; set; } = true;
        public virtual float FontSizeMultiplier { get; set; } = 0.85f;
        public virtual float CharSpacing { get; set; } = -1.7f;
        public virtual float WordSpacingAdjustment { get; set; } = 4.0f;
        public virtual float LineSpacingMultiplier { get; set; } = 1.0f;
        
        [UIValue("font-choices")]
        internal List<object> FontChoices = new List<object>();
        
        public virtual void OnReload() { }
    }
}
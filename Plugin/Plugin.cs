using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;

namespace CustomUISounds
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.hj.customuisounds";
        public const string PluginName = "CustomUISounds";
        public const string PluginVersion = "1.0.0";

        public static ManualLogSource Log;

        ConfigEntry<string> soundPack;

        void Awake()
        {
            Log = Logger;

            BuildDropdown();

            new Harmony(PluginGuid).PatchAll();

            StartCoroutine(SoundManager.LoadPack(soundPack.Value));
            soundPack.SettingChanged += (sender, args) => StartCoroutine(SoundManager.LoadPack(soundPack.Value));
        }

        // Fills the dropdown with "Default (Vanilla)" plus one entry for every soundpack folder.
        void BuildDropdown()
        {
            List<string> packs = new List<string>();
            packs.Add("Default (Vanilla)");

            if (Directory.Exists(SoundManager.soundsFolder))
            {
                foreach (string folder in Directory.GetDirectories(SoundManager.soundsFolder))
                {
                    packs.Add(Path.GetFileName(folder));
                }
            }
            else
            {
                Logger.LogWarning("There's no Soundpack folder at " + SoundManager.soundsFolder);
            }

            soundPack = Config.Bind(
                "General",
                "Sound Pack",
                "Default (Vanilla)",
                new ConfigDescription(
                    "Which pack of sounds to use. Changes apply right away.",
                    new AcceptableValueList<string>(packs.ToArray())));
        }
    }
}

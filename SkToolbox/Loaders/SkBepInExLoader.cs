using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
// Thank you to wh0am15533 for the BepInEx examples
namespace SkToolbox.Loaders
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    class SkBepInExLoader : BaseUnityPlugin
    {
        public const string
            MODNAME = "SkToolbox",
            AUTHOR = "Skrip",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";

        private void Start()
        {
            base.transform.parent = null;
            Object.DontDestroyOnLoad(this);
            SkLoader.LoadedWithBepInEx = true;
            SkLoader.Init();
        }

        public void InitConfig() // These settings are only used if this is loaded from BepInEx
        {
            ConfigEntry.CDescriptor = Config.Bind("- Index", "ThisIsJustAnIndex-NotASetting", true
                , "Config sections:" +
                "\n0 - General" +
                //"\n1 - Auto Run [Currently Disabled due to Hearth and Home patch. Fix coming soon]" +
                // ...
                "\n");

            ConfigEntry.CConsoleEnabled = Config.Bind("0 - General", "ConsoleEnabled", true
                , "Enables the console without launch option.");
        }

        internal class ConfigEntry
        {
            public static ConfigEntry<bool> CDescriptor { get; set; }
            public static ConfigEntry<bool> CConsoleEnabled { get; set; }
        }
    }
}
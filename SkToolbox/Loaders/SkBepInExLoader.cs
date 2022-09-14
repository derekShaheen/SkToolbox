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
            VERSION = "2.0.0.0";

        private static Controllers.MainConsole m_Console;

        private static GameObject skGameObject;
        public static GameObject SkGameObject 
        { 
            get 
            {
                if (skGameObject == null)
                {
                    skGameObject = new GameObject("SkToolbox");
                }
                return skGameObject;
            } 
            set => skGameObject = value; 
        }

        private void Start()
        {
            base.transform.parent = null;
            Object.DontDestroyOnLoad(this);
            Object.DontDestroyOnLoad(SkGameObject);
            Init();
        }

        public void Init()
        {
            Application.runInBackground = true;
            Controllers.SettingsController.Init(Config);
            m_Console = SkGameObject.AddComponent<Controllers.MainConsole>();

        }

        internal class ConfigEntry
        {
            public static ConfigEntry<bool> CDescriptor { get; set; }
            public static ConfigEntry<bool> CConsoleEnabled { get; set; }
        }
    }
}
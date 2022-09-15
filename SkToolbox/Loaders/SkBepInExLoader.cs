using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace SkToolbox.Loaders
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class SkBepInExLoader : BaseUnityPlugin
    {
        public const string
            MODNAME = "SkToolbox",
            AUTHOR = "Skrip",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "2.0.0.0";

        private static Controllers.MainConsole m_Console;

        private Dictionary<KeyCode, string> m_binds = new Dictionary<KeyCode, string>();
        internal Dictionary<KeyCode, string> Binds { get => m_binds; set => m_binds = value; }

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
            UnityEngine.Object.DontDestroyOnLoad(this);
            UnityEngine.Object.DontDestroyOnLoad(SkGameObject);
            Init();
        }

        void Update()
        {
            if (m_Console.IsVisible == false)
            {
                foreach (KeyValuePair<KeyCode, string> pair in m_binds)
                {
                    if (Input.GetKeyDown(pair.Key))
                        m_Console.HandleInput(pair.Value);
                }
            }
        }

        public void Init()
        {
            Application.runInBackground = true;
            Controllers.SettingsController.Init(Config);
            m_Console = SkGameObject.AddComponent<Controllers.MainConsole>();
            m_Console.SkBepInExLoader = this;

            LoadAliases();
            LoadBinds();

            Utility.SkVersionChecker.CheckVersion();
            if(Utility.SkVersionChecker.currentVersion < Utility.SkVersionChecker.latestVersion)
            {
                m_Console.Submit($"New version of SkToolbox available! ({Utility.SkVersionChecker.currentVersion}) -> ({Utility.SkVersionChecker.latestVersion})");
            }
        }

        //
        /// <summary>
        /// Based on the Gungnir code by Zambony. Accessed 9/15/22
        /// Save all of the user's console keybinds to a file in the BepInEx config folder.
        /// </summary>
        public void SaveBinds()
        {
            StringBuilder builder = new StringBuilder();

            foreach (KeyValuePair<KeyCode, string> pair in m_binds)
                builder.AppendLine($"{pair.Key}={pair.Value}");

            string output = builder.ToString();

            string path = Path.Combine(Paths.ConfigPath, GUID + "_binds.txt");
            File.WriteAllText(path, output);
        }

        /// <summary>
        /// Save the user's custom command aliases.
        /// Based on the Gungnir code by Zambony. Accessed 9/15/22
        /// </summary>
        public void SaveAliases()
        {
            StringBuilder builder = new StringBuilder();

            foreach (KeyValuePair<string, string> pair in m_Console.GetCommandHandler().Aliases)
                builder.AppendLine($"{pair.Key}={pair.Value}");

            string output = builder.ToString();

            string path = Path.Combine(Paths.ConfigPath, GUID + "_aliases.txt");
            File.WriteAllText(path, output);
        }

        /// <summary>
        /// Load the user's console keybinds from the file in the BepInEx config folder.
        /// Based on the Gungnir code by Zambony. Accessed 9/15/22
        /// </summary>
        public void LoadBinds()
        {
            string path = Path.Combine(Paths.ConfigPath, GUID + "_binds.txt");

            if (!File.Exists(path))
                return;

            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                // Only split by the first instance of equals.
                string[] info = line.Trim().Split(new char[] { '=' }, 2);

                if (info.Length != 2)
                    continue;

                if (!Enum.TryParse(info[0].Trim(), true, out KeyCode key))
                    continue;

                m_binds.Add(key, info[1].Trim());
            }
        }

        /// <summary>
        /// Load the user's custom command aliases.
        /// Based on the Gungnir code by Zambony. Accessed 9/15/22
        /// </summary>
        public void LoadAliases()
        {
            string path = Path.Combine(Paths.ConfigPath, GUID + "_aliases.txt");

            if (!File.Exists(path))
                return;

            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                // Only split by the first instance of equals.
                string[] info = line.Trim().Split(new char[] { '=' }, 2);

                if (info.Length != 2)
                    continue;

                m_Console.GetCommandHandler().Aliases.Add(info[0], info[1].Trim());
            }
        }
    }
}
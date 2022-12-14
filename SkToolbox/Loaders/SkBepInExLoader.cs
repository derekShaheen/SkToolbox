using BepInEx;
using BepInEx.Configuration;
using SkToolbox.Controllers;
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
            VERSION = "2.0.1.2";

        private static SkBepInExLoader _loader;
        public static SkBepInExLoader Loader { get => _loader; set => _loader = value; }

        private static GameObject _skGameObject;
        public static GameObject SkGameObject
        {
            get
            {
                if (_skGameObject == null)
                {
                    _skGameObject = new GameObject("SkToolbox");
                }
                return _skGameObject;
            }
            set => _skGameObject = value;
        }

        private static MainConsole _console;
        public static MainConsole Console { get => _console; set => _console = value; }


        private Dictionary<KeyCode, string> m_binds = new Dictionary<KeyCode, string>();
        internal Dictionary<KeyCode, string> Binds { get => m_binds; set => m_binds = value; }

        private static ConfigFile _configFile;
        public static ConfigFile ConfigFile { get => _configFile; set => _configFile = value; }

        private void Start()
        {
            ConfigFile = Config;
            SkToolbox.Logger.Debug("Initialization success!");
            if (Loader == null)
            {
                Loader = this;
            }

            Init();
        }

        private void Update()
        {
            if (!Console.IsVisible)
            {
                foreach (KeyValuePair<KeyCode, string> pair in m_binds)
                {
                    if (Input.GetKeyDown(pair.Key))
                        Console.HandleInput(pair.Value, false);
                }
            }
        }

        private void Init()
        {
            Application.runInBackground = true;
            SettingsController.Init(ConfigFile);

            SkGameObject.transform.parent = null;
            UnityEngine.Object.DontDestroyOnLoad(SkGameObject);

            Console = SkGameObject.AddComponent<Controllers.MainConsole>();

            LoadAliases();
            LoadBinds();

            Utility.SkVersionChecker.CheckVersion();
        }

        public void Main()
        {
            Init();
        }

        public void ReInit()
        {
            Destroy(SkGameObject, 0f);
            SkGameObject = null;

            Init();
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

            foreach (KeyValuePair<string, string> pair in Console.GetCommandHandler().Aliases)
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

                Console.GetCommandHandler().Aliases.Add(info[0], info[1].Trim());
            }
        }
    }
}
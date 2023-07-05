using BepInEx;
using BepInEx.Configuration;
using SkToolbox.Controllers;
using SkToolbox.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SkToolbox.Loaders
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class SkBepInExLoader : BaseUnityPlugin
    {
        public const string
            MODNAME = "SkToolbox",
            AUTHOR = "Skrip",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "2.0.2.5";

        private static SkBepInExLoader _loader;
        public static SkBepInExLoader Loader { get => _loader; set => _loader = value; }

        private static GameObject _skGameObject;
        public static GameObject SkGameObject
        {
            get
            {
                if (_skGameObject == null)
                {
                    _skGameObject = new GameObject(MODNAME);
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

            Console = SkGameObject.AddComponent<MainConsole>();

            LoadAliases();
            LoadBinds();

            SkVersionChecker.RegisterCheckRequest(MODNAME,  new Version(VERSION), "https://raw.githubusercontent.com/derekShaheen/SkToolbox/release/SkToolbox/Loaders/SkBepInExLoader.cs", false);
            SkVersionChecker.RegisterCheckRequest("Hit",    new Version(VERSION), "https://hits.dwyl.com/derekShaheen/SkToolbox.svg", false);
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

        public void SaveBinds()
        {
            string path = Path.Combine(Paths.ConfigPath, $"{GUID}_binds.txt");

            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(path);

                foreach (var pair in m_binds)
                    writer.WriteLine($"{pair.Key}={pair.Value}");
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public void SaveAliases()
        {
            string path = Path.Combine(Paths.ConfigPath, $"{GUID}_aliases.txt");

            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(path);

                foreach (var pair in Console.GetCommandHandler().Aliases)
                    writer.WriteLine($"{pair.Key}={pair.Value}");
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public void LoadBinds()
        {
            string path = Path.Combine(Paths.ConfigPath, $"{GUID}_binds.txt");

            if (!File.Exists(path))
                return;

            foreach (var line in File.ReadLines(path))
            {
                var parts = line.Split(new[] { '=' }, 2);

                if (parts.Length != 2 || !Enum.TryParse(parts[0], out KeyCode key))
                    continue;

                m_binds[key] = parts[1].Trim();
            }
        }

        public void LoadAliases()
        {
            string path = Path.Combine(Paths.ConfigPath, $"{GUID}_aliases.txt");

            if (!File.Exists(path))
                return;

            foreach (var line in File.ReadLines(path))
            {
                var parts = line.Split(new[] { '=' }, 2);

                if (parts.Length != 2)
                    continue;

                Console.GetCommandHandler().Aliases[parts[0]] = parts[1].Trim();
            }
        }
    }
}
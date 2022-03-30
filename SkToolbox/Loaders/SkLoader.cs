using SkToolbox.Utility;
using System.Linq;
using UnityEngine;

/// <summary>
/// Credit for some of the code in this class to wh0am15533, who can be found on Github. Your trainers have great example code. Thank you for posting your loaders and injectors!
/// Namespace: SkToolbox
/// Class: SkLoader
/// Method: Init or InitThreading
/// </summary>
namespace SkToolbox.Loaders
{
    public class SkLoader : MonoBehaviour
    {
        public static GameObject _SkGameObject;
        public static SkMenuController MenuController;

        public static bool ConsoleEnabled = true;
        public static bool MenuEnabled = true;

        private static bool FirstLoad = true;
        private static bool InitLogging = false;

        private static bool ReadyForGameObject = true;

        public static void SelfDestruct()
        {
            SkLoader._SkGameObject = null; // https://answers.unity.com/questions/1186978/does-calling-destroy-on-a-gameobjectmonobehavior-d.html
            Destroy(_SkGameObject);
            CheckForUnknownInstance();
            Destroy(GameObject.FindObjectOfType<SkLoader>());
        }
        public static void Reload()
        {
            SkLoader._SkGameObject = null; // https://answers.unity.com/questions/1186978/does-calling-destroy-on-a-gameobjectmonobehavior-d.html
            Destroy(_SkGameObject);
            Init();
        }

        public static GameObject Load
        {
            get => SkLoader._SkGameObject;
            set => SkLoader._SkGameObject = value;
        }

        private void Start()
        {
            //ReadyForGameObject = DelayLoader.CheckReady();
            SkLoader.Init();
        }

        public static void Main(string[] args)
        {
            //ReadyForGameObject = DelayLoader.CheckReady();
            SkLoader.Init();
        }

        public static void InitWithLog()
        {
            //ReadyForGameObject = DelayLoader.CheckReady();
            InitLogging = true;
            Init();
        }

        private void Update()
        {
            //if(!ReadyForGameObject) // If we weren't ready for immediate load, check each frame to see if that has changed.
            //{
            //    ReadyForGameObject = DelayLoader.CheckReady();
            //    if(ReadyForGameObject)
            //    {
            //        Init();
            //    }
            //}
        }

        public static void Init()
        {
            Application.runInBackground = true;
            if (FirstLoad) SkUtilities.Logz(new string[] { "LOADER", "STARTUP" }, new string[] { "SUCCESS!" });
            FirstLoad = false;

            if (!ReadyForGameObject)
            {
                return;
            }

            if (InitLogging)
            {
                SkConsole.writeToFile = true;
                InitLogging = false;
            }

            SkLoader._SkGameObject = new GameObject("SkToolbox");
            if (ConsoleEnabled)
            {
                SkLoader._SkGameObject.AddComponent<SkConsole>(); // Load the console first so output from the controller can be observed on the following frame
            }

            CheckForUnknownInstance();

            SkLoader.Load.transform.parent = null;
            Transform root = SkLoader.Load.transform.root;
            if (root != null)
            {
                if (root.gameObject != SkLoader.Load)
                {
                    root.parent = SkLoader.Load.transform;
                }
            }
            if (MenuEnabled)
            {
                MenuController = SkLoader._SkGameObject.AddComponent<SkMenuController>(); // Load the menu controller
            }
            UnityEngine.Object.DontDestroyOnLoad(SkLoader._SkGameObject);
        }

        public static void CheckForUnknownInstance()
        {
            System.Collections.Generic.IEnumerable<GameObject> OtherSkToolBoxs = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "SkToolbox");

            foreach (GameObject Other in OtherSkToolBoxs)
            {
                if (Other != SkLoader._SkGameObject)
                {
                    Destroy(Other);
                    //SkUtilities.Logz(new string[] { "LOADER", "DETECT" }, new string[] { "Other SkToolbox Destroyed." });
                }
            }
        }
    }

}


//using System.Collections.Generic;
//using UnityEngine;
//using SkToolbox.Utility;
//using System;

//namespace SkToolbox.SkModules
//{

// Old / Outdated. Keeping this class for now in case some functionality may be required.


//    /// <summary>
//    /// All modules must inerhit from this base class. Within those modules, base.Ready() must be called when the module is ready for use, and this must happen within 3 frames of module initialization.
//    /// </summary>
//    public class SkBaseModule : IModule
//    {
//        internal bool conWriteToFile = false;

//        public SkMenu MenuOptions { get; set; } = new SkMenu();
//        public SkMenuItem CallerEntry { get; set; } = new SkMenuItem();
//        public SkUtilities.Status ModuleStatus { get; set; } = SkUtilities.Status.Initialized;
//        public bool IsEnabled
//        {
//            get { return isEnabled; }
//            set
//            {
//                if (IsEnabled && ModuleStatus != SkUtilities.Status.Ready) // If the module is ready, then it is loaded and running.
//                {   // To disable, set the status to "Unload" so it properly unloads.
//                    isEnabled = value;
//                }
//                if (!IsEnabled)
//                {
//                    IsEnabled = value;
//                }
//            }
//        }

//        public string ModuleName { get => moduleName; set => moduleName = value; }

//        private string moduleName = "UNNAMED";

//        private bool isEnabled = true;

//        public SkBaseModule()
//        {
//            Start();
//        }

//        public List<SkMenuItem> FlushMenu()
//        {
//            return MenuOptions.FlushMenu();
//        }

//        public void RequestMenu()
//        {
//            Loaders.SkLoader.MenuController.RequestSubMenu(MenuOptions.FlushMenu());
//        }

//        public void RequestMenu(SkMenu Menu)
//        {
//            Loaders.SkLoader.MenuController.RequestSubMenu(Menu);
//        }

//        public void RemoveModule()
//        {

//            throw new NotImplementedException();
//            //Destroy(this);
//        }

//        public void Ready()
//        {
//            ModuleStatus = SkUtilities.Status.Ready;
//        }
//        public void Loading()
//        {
//            ModuleStatus = SkUtilities.Status.Loading;
//        }
//        public void Error()
//        {
//            ModuleStatus = SkUtilities.Status.Error;
//        }
//        public void Unload()
//        {
//            ModuleStatus = SkUtilities.Status.Unload;
//        }


//        public void Start()
//        {
//            ModuleName = "Console Controller"; // Set the module name
//            Loading(); // Module is loading
//            CallerEntry = new SkMenuItem("Toolbox Menu\t►", () => Loaders.SkLoader.MenuController.RequestSubMenu(FlushMenu())); // Create the CallerEntry
//                                                                                                                                          // CallerEntry defines what will be seen on the main SkToolbox menu when opened and what will happen when the menu option is selected.
//                                                                                                                                          // Intended behavior is that this will flush the base menu into the menu controller through the request submenu method. This will happen in essetially every module.
//            BeginMenu(); // Generate the submenu when the module starts
//            Ready(); // Set the module status to ready. Must be set after loading on first frame.
//        }

//        public void BeginMenu()
//        {
//            SkMenu consoleOptMenu = new SkMenu(); // Create a new menu object and add items to it

//            consoleOptMenu.AddItem("Reload Menu", new Action(ReloadMenu), "Reload the toolbox");
//            consoleOptMenu.AddItem("Unload Toolbox", new Action(UnloadMenu), "Unload the toolbox from memory");
//            consoleOptMenu.AddItem("Open Log Folder", new Action(OpenLogFolder), "Open Unity log folder");
//            consoleOptMenu.AddItem("Advanced\t►", new Action(BeginAdvancedMenu), "Show advanced options");
//            MenuOptions = consoleOptMenu; // Set the module menu options to the menu we just created
//        }

//        //

//        public void BeginAdvancedMenu() // Generate submenu
//        {
//            SkMenu GenericMenu = new SkMenu();
//            GenericMenu.AddItem("Unload Toolbox", new Action(UnloadMenu), "Unload the toolbox from memory");
//            GenericMenu.AddItem("Open Log Folder", new Action(OpenLogFolder), "Open Unity log folder");
//            GenericMenu.AddItemToggle("Write to File", ref conWriteToFile, new Action(ToggleWriteFile), "Write log output to file?");
//            GenericMenu.AddItem("Dump Root Objects", new Action(DumpRootObjects), "Dump root object to log");
//            RequestMenu(GenericMenu); // Display the submenu
//        }

//        public void ToggleWriteFile()
//        {
//            conWriteToFile = !conWriteToFile;
//            SkConsole.writeToFile = conWriteToFile;
//            BeginMenu();
//        }

//        public void OpenLogFolder()
//        {
//            SkUtilities.Logz(new string[] { "CMD", "REQ" }, new string[] { "Opening Log Directory" });
//            Application.OpenURL(Application.persistentDataPath);
//        }

//        public void ReloadMenu()
//        {
//            SkUtilities.Logz(new string[] { "BASE", "CMD", "REQ" }, new string[] { "UNLOADING CONTROLLERS AND MODULES...", "SKTOOLBOX RELOAD REQUESTED.", });
//            Loaders.SkLoader.Reload();
//        }

//        public void UnloadMenu()
//        {
//            SkUtilities.Logz(new string[] { "BASE", "CMD", "REQ" }, new string[] { "SKTOOLBOX UNLOAD REQUESTED.", "UNLOADING CONTROLLERS AND MODULES..." });
//            Loaders.SkLoader.SelfDestruct();
//        }

//        public void DumpRootObjects()
//        {
//            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
//            {
//                GameObject[] rootObjs = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).GetRootGameObjects();
//                foreach (GameObject obj in rootObjs)
//                {
//                    SkUtilities.Logz(new string[] { "DUMP", "OBJ" }, new string[] { obj.name, obj.GetType().ToString() });
//                }

//            }
//        }
//    }
//}

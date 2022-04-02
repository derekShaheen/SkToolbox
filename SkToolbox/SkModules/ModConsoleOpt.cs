using SkToolbox.Utility;
using System;
using UnityEngine;

namespace SkToolbox.SkModules
{
    public class ModConsoleOpt : SkBaseModule, IModule
    {
        internal bool conWriteToFile = false;

        /// <summary>
        /// Initialize the module
        /// </summary>
        public ModConsoleOpt() : base()
        {
            base.ModuleName = "Console Controller"; // Set the module name
            base.Loading(); // Module is loading
            base.CallerEntry = new SkMenuItem("Toolbox Menu\t►", () => base.SkMC.RequestSubMenu(base.FlushMenu())); // Create the CallerEntry
                                // CallerEntry defines what will be seen on the main SkToolbox menu when opened and what will happen when the menu option is selected.
                                // Intended behavior is that this will flush the base menu into the menu controller through the request submenu method. This will happen in essetially every module.
        }

        public void Start()
        {
            BeginMenu(); // Generate the submenu when the module starts
            base.Ready(); // Set the module status to ready. Must be set after loading on first frame.
        }

        public void BeginMenu()
        {
            SkMenu consoleOptMenu = new SkMenu(); // Create a new menu object and add items to it

            consoleOptMenu.AddItem("Reload Menu", new Action(ReloadMenu), "Reload the toolbox");
            consoleOptMenu.AddItem("Unload Toolbox", new Action(UnloadMenu), "Unload the toolbox from memory");
            consoleOptMenu.AddItem("Advanced\t►", new Action(BeginAdvancedMenu), "Show advanced options");
            base.MenuOptions = consoleOptMenu; // Set the module menu options to the menu we just created
        }

        //

        public void BeginAdvancedMenu() // Generate submenu
        {
            SkMenu GenericMenu = new SkMenu();
            GenericMenu.AddItemToggle("Write to File", ref conWriteToFile, new Action(ToggleWriteFile), "Write log output to file?");
            GenericMenu.AddItem("Open Log Folder", new Action(OpenLogFolder), "Open Unity log folder");
            GenericMenu.AddItem("Dump Root Objects", new Action(DumpRootObjects), "Dump root object to log");
            base.RequestMenu(GenericMenu); // Display the submenu
        }

        public void ToggleWriteFile()
        {
            conWriteToFile = !conWriteToFile;
            SkConsole.writeToFile = conWriteToFile;
            BeginMenu();
        }

        public void OpenLogFolder()
        {
            SkUtilities.Logz(new string[] { "CMD", "REQ" }, new string[] { "Opening Log Directory" });
            Application.OpenURL(Application.persistentDataPath);
        }

        public void ReloadMenu()
        {
            SkUtilities.Logz(new string[] { "BASE", "CMD", "REQ" }, new string[] { "UNLOADING CONTROLLERS AND MODULES...", "SKTOOLBOX RELOAD REQUESTED.", });
            Loaders.SkLoader.Reload();
        }

        public void UnloadMenu()
        {
            SkUtilities.Logz(new string[] { "BASE", "CMD", "REQ" }, new string[] { "SKTOOLBOX UNLOAD REQUESTED.", "UNLOADING CONTROLLERS AND MODULES..." });
            Loaders.SkLoader.SelfDestruct();
        }

        public void DumpRootObjects()
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                GameObject[] rootObjs = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).GetRootGameObjects();
                foreach (GameObject obj in rootObjs)
                {
                    SkUtilities.Logz(new string[] { "DUMP", "OBJ" }, new string[] { obj.name });
                }

            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SkToolbox.Utility;
using Random = UnityEngine.Random;

namespace SkToolbox.SkModules
{
    public class ModConsoleOpt : SkBaseModule, IModule
    {
        internal bool conWriteToFile = false;

        public ModConsoleOpt() : base()
        {
            base.ModuleName = "Console Controller";
            base.Loading();
            base.CallerEntry = new SkMenuItem("Toolbox Menu\t►", () => base.SkMC.RequestSubMenu(base.FlushMenu()));
        }

        public void Start()
        {
            BeginMenu();
            base.Ready();
        }

        public void BeginMenu()
        {
            SkMenu consoleOptMenu = new SkMenu();

            consoleOptMenu.AddItem("Reload Menu", new Action(ReloadMenu), "Reload the toolbox");
            consoleOptMenu.AddItem("Unload Toolbox", new Action(UnloadMenu), "Unload the toolbox from memory");
            consoleOptMenu.AddItem("Advanced\t►", new Action(BeginAdvancedMenu), "Show advanced options");
            MenuOptions = consoleOptMenu;
        }

        //

        public void BeginAdvancedMenu()
        {
            SkMenu GenericMenu = new SkMenu();
            GenericMenu.AddItemToggle("Write to File", ref conWriteToFile, new Action(ToggleWriteFile), "Write log output to file?");
            GenericMenu.AddItem("Open Log Folder", new Action(OpenLogFolder), "Open Unity log folder");
            GenericMenu.AddItem("Dump Root Objects", new Action(DumpRootObjects), "Dump root object to log");
            base.RequestMenu(GenericMenu);
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

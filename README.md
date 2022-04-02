|Main / Nightly|Full Release|Hit Counter
| :------------: | :------------: | :------------: |
|![mainworkflow](https://github.com/derekShaheen/SkToolbox/actions/workflows/Beta.yml/badge.svg)|![releaseworkflow](https://github.com/derekShaheen/SkToolbox/actions/workflows/release.yml/badge.svg)| [![HitCount](https://hits.dwyl.com/derekShaheen/SkToolbox.svg?style=flat)](http://hits.dwyl.com/derekShaheen/SkToolbox)|

![SkToolbox Header](https://i.imgur.com/bTaEOXP.png "SkToolbox Header")

#### The SkToolbox is a framework used for quickly designing overlay menus in Unity Games. Typically used for creating a custom menu for use with existing projects. Includes loader module for mono injection or via BepInEx (recommended). These menus can be controlled both via keyboard or mouse input.

Index:
- Examples
- How To Use (Developer)
	- BepInEx Injection	
	- Adding menu modules
	- Adding console commands
	
------------
### - Example releases with feature highlights:
- Integrated Console with easy to add commands. This console will capture all debug output to console for whatever game it is attached to. You can also see the output from the Toolbox and your modules easily here.

![Console1](https://i.imgur.com/f34jbtC.png)

- **[SkToolbox - Valheim](https://www.nexusmods.com/valheim/mods/8 "SkToolbox for Valheim")**

![Valheim](https://i.imgur.com/zDRoDUI.png "Valheim")
![Valheim2](https://i.imgur.com/NkOaQYp.png)
![Valheim3](https://i.imgur.com/SQiiOVi.png)

- **[SkToolbox - For The King](https://www.nexusmods.com/fortheking/mods/3 "SkToolbox - For The King")**

[Video Demonstration](https://www.youtube.com/watch?v=TWPHwp9luHU "Video Demonstration")

![FTK1](https://i.imgur.com/UVkiC8m.png)

- **[SkToolbox - War for the Overworld](https://www.nexusmods.com/warfortheoverworld/mods/2 "SkToolbox - War for the Overworld")**

[Video Demonstration](https://www.youtube.com/watch?v=9QpRQ4nQ1P8 "Video Demonstration")

- **[SkToolbox - Mini Metro](https://www.nexusmods.com/minimetro/mods/1 "SkToolbox for Mini Metro")**
![MiniMetro2](https://i.imgur.com/FoiZsr3.png)
![MiniMetro1](https://media.giphy.com/media/YmMcwlLIClhx2yST7F/giphy.gif "MiniMetro1")

------------
### How To Use (Developer)
- In order to use the framework, simply download it and reference it in your .NET Framework 4.7.X project. You will then be able to both create *commands* (```SkToolbox.Commands.SkCommand```) for the integrated console and *modules* (```SkToolbox.SkModules.SkBaseModule```) which will be automatically rendered and handled by the menu controller.

- Below we describe and provide examples for:
	- BepInEx Injection
	- Adding Menu Modules
	- Adding Console Commands

#### BepInEx Injection
- It is recommended that you use BepInEx for your injection. The example below can be used in conjunction with either adding menu modules, adding console commands, or both. This example shows a class that adds a menu module to a running instance of SkToolbox. Commands are automatically detected, but because menu modules are GameObjects, they must be handled with more care.

```chsarp
using BepInEx;
using System.Collections;
using UnityEngine;

namespace TestModule
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency("com.Skrip.SkToolbox")] // Set dependency so this loads after the SkToolbox
    class SkBepInExLoader : BaseUnityPlugin
    {
        private int retrycount = 0;
        public const string
            MODNAME = "SkToolboxExtension",
            AUTHOR = "Skrip",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";


        private void Start()
        {
            StartCoroutine(Process());
        }

        IEnumerator Process()
        {
            GameObject _SkGameObject = null; // Initialize
            ModAsset module = new ModAsset(); // Create module object
            while (_SkGameObject == null && retrycount < 10) // Try 10 times
            {
                yield return new WaitForSecondsRealtime(1); // Wait 1 second between each try
                _SkGameObject = SkToolbox.Loaders.SkLoader._SkGameObject; // Has the SkToolbox initalized yet? If so, drop out of the loop
                retrycount += 1;

            }
            if (_SkGameObject != null)
            {
                SkToolbox.Loaders.SkLoader.MenuController.SkModuleController.AddModule(module); // Add the module
            }
        }
    }
}
```

#### Add Menu Modules
- What is a menu module? In the image below, we refer to the entries in the *yellow* box as *modules* (```SkToolbox.SkModules.SkBaseModule```) and the entries in the *cyan* box as *menu items* (```SkToolbox.SkMenuItem```). These *menu items* are stored inside of the *modules*. Each module is a self contained Unity GameObject, and runs independently of each other *module*.

![MenuModules](https://i.imgur.com/lARhjfv.png)

See example code below for a module that is standard within the SkToolbox, for controlling the toolbox, and some other example options. Functionality will be expanded eventually.

```csharp
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

```

#### Add Commands
- Commands are automatically detected by the SkToolbox and added to the console for use upon load. Commands can also be manually manipulated or added via methods in the *command processor* (```SkToolbox.SkCommandProcessor```). 

See example code below of a command actually used in the SkToolbox - Mini Metro release.

```csharp
    public class CmdSetGamemode : SkCommand
    {
	    Game game;
        GameController gameController;
        public override string Command => "SetGamemode";

        public override string Description => "[Gamemode] - CLASSIC, ZEN, EXTREME, SANDBOX, PLAYABLE_COUNT, FAQ";

        public override SkCommandEnum.VisiblityFlag VisibilityFlag => SkCommandEnum.VisiblityFlag.Visible;

        public override bool Enabled => true;

        public override void Execute(string[] args)
        {
            if (args.Length > 0) // Arg was provided, attempt to set gamemode
            {
                GetObjects(); // Get the objects needed from the game

                GameMode gameMode = GameMode.CLASSIC; // Set default gamemode

                Enum.TryParse(args[0], out gameMode); // Attempt to parse the arg provided

                if (game != null)
                {
                    game.Mode = gameMode;
                    game?.HudScreen?.HandleGameModeChanged();
                    SkUtilities.Logz(new string[] { "SetGamemode" }, new string[] { "Game mode set to: " + game.ModeString }); // Report to the console the successful change
                }
            }
            else // No arg provided, display the current gamemode
            {
                GetObjects(); // Get the objects needed from the game
                SkUtilities.Logz(new string[] { "SetGamemode" }, new string[] { "Gamemode: " + game.ModeString }); // Display the gamemode
            }
        }

        public void GetObjects() // Find the objects needed to set the gamemode
        {
            if (Main.Instance != null) // If main isn't loaded, we're not in game
            {
                gameController = SkUtilities.GetPrivateField<GameController>(Main.Instance, "controller"); // Use the SkUtilities from the SkToolbox to get the private GameController "controller" field in Main.instance (Main.instance.controller)
                if (gameController == null)
                {
                    SkUtilities.Logz("Could not find game controller.");
                }

                game = SkUtilities.GetPrivateField<Game>(gameController, "game");
                if (gameController == null)
                {
                    SkUtilities.Logz("Could not find game.");
                }
            }
        }
    }
```
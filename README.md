|Main / Nightly|Stable / Release|Hit Counter
| :------------: | :------------: | :------------: |
|![mainworkflow](https://github.com/derekShaheen/SkToolbox/actions/workflows/nightly.yml/badge.svg)|![releaseworkflow](https://github.com/derekShaheen/SkToolbox/actions/workflows/release.yml/badge.svg)| [![HitCount](https://hits.dwyl.com/derekShaheen/SkToolbox.svg?style=flat)](http://hits.dwyl.com/derekShaheen/SkToolbox)|

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
- The SkToolbox framework is designed to facilitate the creation of on-screen menus and in-game console commands. It is expected that you'll have an SkToolbox.dll and your own dll module that will be loaded by the framework.
- In order to use the framework, simply download it and reference it in your .NET Framework 4.7.X project. You will then be able to both create *commands* (```SkToolbox.Commands.SkCommand```) for the integrated console and *modules* (```SkToolbox.SkModules.IModule```) which will be automatically rendered and handled by the menu controller.
- While it is recommended that you use BepInEx for injection, it is not required, and any mono injector can be used. The SkToolbox is designed for use with BepInEx, essentially any mono injector, doorstop, etc. Take into account that most of the module examples you'll find are created for use with BepInEx, but you can simply substitute that class with another loader and it should work as expected.

	#### 🟢 Module Example: [Link to working module for Mini Metro](https://github.com/derekShaheen/SkToolbox-for-MiniMetro "Link to full, working module for Mini Metro")

- Below we describe and provide examples for:
	- BepInEx Injection
	- Adding Menu Modules
	- Adding Console Commands

#### BepInEx Injection
- It is recommended that you use BepInEx for your injection. The example below can be used in conjunction with either adding menu modules, adding console commands, or both. This example shows a class that adds a menu module to a running instance of SkToolbox. Commands are automatically detected, but because menu modules are GameObjects, they must be handled with more care.

```csharp
using BepInEx;
using System.Collections;
using UnityEngine;

namespace TestModule
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency("com.Skrip.SkToolbox")] // Set dependency so this loads after the SkToolbox
    class SkBepInExLoader : BaseUnityPlugin
    {
        public const string
            MODNAME = "SkToolboxExtension",
            AUTHOR = "Skrip",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";


        /*private void Start()
        {
            // Simply allow BepInEx to load the plugin. The SkToolbox will automatically detect menu modules and console commands.
            // You can manually add and remove modules from the plugin though if you choose.
            // No other code is required in this class by default, and this method only needs to be uncommented if you have your own reason.
        }*/
    }
}
```

#### Add Menu Modules
- What is a menu module? In the image below, we refer to the entries in the *yellow* box as *modules* (```SkToolbox.SkModules.IModule```) and the entries in the *cyan* box as *menu items* (```SkToolbox.SkMenuItem```). 
These *menu items* are stored inside of the *modules*. Each module is a self contained class, and runs independently of each other *module*. These classes are free to spawn ```UnityEngine.GameObject```s if standard Unity MonoBehavior methods are needed.

![MenuModules](https://i.imgur.com/lARhjfv.png)

See example code below for a module that is standard within the SkToolbox, for controlling the toolbox, and some other example options. Functionality will be expanded eventually.
Be sure to look at the region notes.

```csharp
using System.Collections.Generic;
using UnityEngine;
using SkToolbox.Utility;
using System;

namespace SkToolbox.SkModules
{
    /// <summary>
    /// All modules must inerhit from this base class. Within those modules, base.Ready() must be called when the module is ready for use, and this must happen within 3 frames of module initialization.
    /// </summary>
    public class ModConsoleOpt : IModule
    {
        private string moduleName = "Console Controller";

        #region Standard Methods
        /// <summary>
        /// These methods will be used in every menu module. Copy / Paste this entire Standard Methods region to new modules as they are created.
        /// These are placed here instead of a base class due to Activator.CreateInstance() not being able to call subclass constructors. 
        /// 
        /// !! This region generally does not need to be modified.
        /// </summary>
        public SkMenu MenuOptions { get; set; } = new SkMenu();
        public SkMenuItem CallerEntry { get; set; } = new SkMenuItem();
        public SkUtilities.Status ModuleStatus { get; set; } = SkUtilities.Status.Initialized;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (IsEnabled && ModuleStatus != SkUtilities.Status.Ready) // If the module is ready, then it is loaded and running.
                {   // To disable, set the status to "Unload" so it properly unloads.
                    isEnabled = value;
                }
                if (!IsEnabled)
                {
                    IsEnabled = value;
                }
            }
        }
        
        public string ModuleName { get => moduleName; set => moduleName = value; }


        private bool isEnabled = true;

        internal bool conWriteToFile = false;

        public List<SkMenuItem> FlushMenu()
        {
            return MenuOptions.FlushMenu();
        }

        public void RequestMenu()
        {
            Loaders.SkLoader.MenuController.RequestSubMenu(MenuOptions.FlushMenu());
        }

        public void RequestMenu(SkMenu Menu)
        {
            Loaders.SkLoader.MenuController.RequestSubMenu(Menu);
        }

        public void RemoveModule()
        {

            throw new NotImplementedException();
            //Destroy(this);
        }

        public void Ready()
        {
            ModuleStatus = SkUtilities.Status.Ready;
        }
        public void Loading()
        {
            ModuleStatus = SkUtilities.Status.Loading;
        }
        public void Error()
        {
            ModuleStatus = SkUtilities.Status.Error;
        }
        public void Unload()
        {
            ModuleStatus = SkUtilities.Status.Unload;
        }
        #endregion Standard Methods

        #region Required but Individual
        /// <summary>
        /// This region is also copied to each new module, but needs to be modified individually to set up each module.
        /// </summary>

        public ModConsoleOpt()
        {
            Start();
        }

        public void Start()
        {
            Loading(); // Module is loading
            CallerEntry = new SkMenuItem("Toolbox Menu\t►", () => Loaders.SkLoader.MenuController.RequestSubMenu(FlushMenu())); // Create the CallerEntry
                                                                                                                                // CallerEntry defines what will be seen on the main SkToolbox menu when opened and what will happen when the menu option is selected.
                                                                                                                                // Intended behavior is that this will flush the base menu into the menu controller through the request submenu method. This will happen in essetially every module.
            BeginMenu(); // Generate the submenu when the module starts
            Ready(); // Set the module status to ready. Must be set after loading on first frame.
        }

        public void BeginMenu()
        {
            SkMenu consoleOptMenu = new SkMenu(); // Create a new menu object and add items to it

            consoleOptMenu.AddItem("Reload Menu", new Action(ReloadMenu), "Reload the toolbox");
            consoleOptMenu.AddItem("Unload Toolbox", new Action(UnloadMenu), "Unload the toolbox from memory");
            consoleOptMenu.AddItem("Open Log Folder", new Action(OpenLogFolder), "Open Unity log folder");
            consoleOptMenu.AddItem("Advanced\t►", new Action(BeginAdvancedMenu), "Show advanced options");
            MenuOptions = consoleOptMenu; // Set the module menu options to the menu we just created
        }

        #endregion Required but Individual

        // Methods called from the menu

        public void BeginAdvancedMenu() // Generate submenu
        {
            SkMenu GenericMenu = new SkMenu();
            GenericMenu.AddItem("Unload Toolbox", new Action(UnloadMenu), "Unload the toolbox from memory");
            GenericMenu.AddItem("Open Log Folder", new Action(OpenLogFolder), "Open Unity log folder");
            GenericMenu.AddItemToggle("Write to File", ref conWriteToFile, new Action(ToggleWriteFile), "Write log output to file?");
            GenericMenu.AddItem("Dump Root Objects", new Action(DumpRootObjects), "Dump root object to log");
            RequestMenu(GenericMenu); // Display the submenu
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
                    SkUtilities.Logz(new string[] { "DUMP", "OBJ" }, new string[] { obj.name, obj.GetType().ToString() });
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
    public override string Command => "SetGamemode";

    public override string Description => "CLASSIC, ZEN, EXTREME, SANDBOX, PLAYABLE_COUNT, FAQ";

    public override SkCommandEnum.VisiblityFlag VisibilityFlag => SkCommandEnum.VisiblityFlag.Visible;

    public override bool Enabled => true;

    public override string[] Hints => new string[] {"Gamemode"};

    public override void Execute(string[] args)
    {
        if (args.Length > 0) // Search for specific commands
        {
            GetObjects();

            GameMode gameMode = GameMode.CLASSIC;

            Enum.TryParse(args[0], out gameMode);

            if (game != null)
            {
                game.Mode = gameMode;
                game?.HudScreen?.HandleGameModeChanged();
                SkUtilities.Logz(new string[] { "SetGamemode" }, new string[] { "Game mode set to: " + game.ModeString });
            }
        }
        else // Search for all commands
        {
            GetObjects();
            SkUtilities.Logz(new string[] { "SetGamemode" }, new string[] { "Gamemode: " + game.ModeString });
        }
    }

    Game game;
    GameController gameController;
    public void GetObjects()
    {
        if (Main.Instance != null)
        {
            gameController = SkUtilities.GetPrivateField<GameController>(Main.Instance, "controller");
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
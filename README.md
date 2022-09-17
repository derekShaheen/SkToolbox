|Main / Nightly|Stable / Release|Hit Counter
| :------------: | :------------: | :------------: |
|![mainworkflow](https://github.com/derekShaheen/SkToolbox/actions/workflows/nightly.yml/badge.svg)|![releaseworkflow](https://github.com/derekShaheen/SkToolbox/actions/workflows/release.yml/badge.svg)| [![HitCount](https://hits.dwyl.com/derekShaheen/SkToolbox.svg?style=flat)](http://hits.dwyl.com/derekShaheen/SkToolbox)|

![SkToolbox Header](https://i.imgur.com/bTaEOXP.png "SkToolbox Header")

#### The SkToolbox is a framework used for quickly implementing a custom console with executable commands. Typically used for creating a plugin for use with existing projects. This project is expected to be injected via [BepInEx](https://github.com/BepInEx/BepInEx "BepInEx"). Commands are automatically converted to clickable buttons to provide for a more complete user interface.


[![Sample](https://i.imgur.com/pvrEi8Q.gif)](https://i.imgur.com/pvrEi8Q.gif "Click for higher resolution!")
---
#### Index
- Features
- Examples
- How to Use Framework

### Features
This console supports the following features:
- Command auto-completion via tab. If the user has partially entered a command, they can use tab to cycle through the potential commands they might be entering.
- Command chaining, in which multiple commands can be input on a single line, separated by a semi-colon. (Ex. cmd1; cmd2; cmd3; ...)
- Command cycling via the up/down arrows. Unlimited commands will be remembered.
- Command suggestions will be displayed as the user types.
- Command parameters will be automatically suggested and highlighted as the user types.
- Command aliasing allows the user to create custom chains of commands.
- Command key binding allows users to run any number of commands via key press when outside of the console.
- Automatically generate on-screen buttons based on registered commands. As buttons are added, this will become a scrollable menu.
- Supports color theme changes on the fly! Set the theme to any color via command or config!
- Many options and preferences are found in the configuation file.
------------

### Examples
Full game examples coming soon!
[![Main](https://i.imgur.com/TF0oAnf.png "Main")](https://i.imgur.com/TF0oAnf.png "Main")
[![gifexample](https://media3.giphy.com/media/FrUmT2jbaIFzhLWqro/giphy.gif "Click for higher resolution!")](https://streamable.com/grjcam "gifexample")

### How to Use Framework
Here we will examine an example, made for the game Autonauts vs Pirates. In this example, we reference the SkToolbox.dll and we declare a command to be added to the left button panel and that can be run from the console.

Complete example with code that runs with BepInEx and declares the EnableTools command.
[![FrameworkScreenResult](https://i.imgur.com/EGERiQN.png "FrameworkScreenResult")](https://i.imgur.com/EGERiQN.png "FrameworkScreenResult")
```csharp
using BepInEx;
using SkToolbox;

/// <summary>
/// Plugin for SkToolbox, intended for running on Autonauts vs Pirates
/// </summary>
namespace SkToolboxAvPB
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency("com.Skrip.SkToolbox")] // Set the dependency 
    class SkBepInExLoader : BaseUnityPlugin
    {
        public const string // Declare plugin information
            MODNAME = "SkToolboxAvPB",
            AUTHOR = "Skrip",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";

        [Command("EnableTools", "Enables the in-game tools menu.", "World")] // Declare the 'EnableTools' command
        public static void EnableTools()
        {
            CheatManager.Instance.m_CheatsEnabled = true;
            GameStateManager.Instance.SetState(GameStateManager.State.CreativeTools);
        }
    }
}
```

##### What is a Command and how is it defined?
The following is the signature for a command. Simply apply these attributes to a method as shown in the example above, and it will be automatically detected upon injection. Only the keyword and description are required parameters.
Note: 
- Defined methods *must* be **public and static!**
- SkToolbox modules and commands are not compatible with version >=2.0.0.
```csharp
public static Command(string keyword, string description, string category = "zzBottom", Util.DisplayOptions displayOptions = Util.DisplayOptions.All, int sortPriority = 100)
```
- Keyword: This is the command that will be typed to run it. In the code above, "EnableTools" was the keyword. Key words are also used to display the text in the button panel on the left side. Camel case commands will be automatically converted to readable text for the buttons. NOTE: The keyword does *not* need to match the method name.
- Description: This is what will be shown when the user is receiving a hint for this command as they type it. This is also what will be shown from the help command.
- Category: This is the category that the button will be shown within on the left side. Leave default for no category.
- DisplayOptions: All, PanelOnly, ConsoleOnly - Display this command everywhere or just in one panel? Mostly used to prevent buttons from appearing for specific commands.
- Sort Priority: This will be used to sort the buttons within categories. Lower is higher on the menu.
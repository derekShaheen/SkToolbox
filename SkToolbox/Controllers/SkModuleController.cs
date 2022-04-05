using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SkToolbox.Utility.SkUtilities;

namespace SkToolbox
{
    /// <summary>
    /// This class controls the modules that will be used to generate the menu for use in-game. This class will enforce modules being in a ready state, and will automatically unload the module if it enters an error state.
    /// </summary>
    public class SkModuleController : MonoBehaviour
    {
        #region Initializations
        private static Version SkMainVersion = new Version(1, 1, 3); // 12/2020

        internal Status SkMainStatus = Status.Initialized;

        private bool firstLoad = true;
        private bool needLoadModules = true;
        private bool needRetry = false;
        private bool errorMonitor = false;
        private int retryCount = 1; // Current load try
        private int retryCountMax = 3; // How many frames should it check for ready before it unloads the module?

        public SkMenuController menuController;

        public List<SkModules.IModule> menuOptions { get => _menuOptions; set => _menuOptions = value; }
        private List<SkModules.IModule> retryModule { get; set; } = new List<SkModules.IModule>();


        #endregion

        public List<SkModules.IModule> _menuOptions = new List<SkModules.IModule>();

        //

        #region UnityStandardMethods
        public void Start()
        {
            SkMainStatus = Status.Loading;
            Logz(new string[] { "TOOLBOX", "NOTIFY" }, new string[] { "LOADING...", "MODULES LOADING..." }); // Notify the console that the menu is ready

            // Load the main menu
            BeginMainMenu();

            // Get the menu controller
            menuController = GetComponent<SkMenuController>();
            if (menuOptions.Count > 0 && menuController != null)
            {
                SkMainStatus = Status.Loading;
            }
            else
            {
                SkMainStatus = Status.Error;
            }

            Init();

        }
        public void Update()
        {
            if (!firstLoad && menuOptions.Count > 0)// This is set to false on the 2nd frame, giving the modules one frame to initialize and run their Start() method.
            {
                if (SkMainStatus == Status.Loading && needLoadModules && !needRetry) // Are we loading, still needing to load the modules, but don't need to retry yet...
                {
                    foreach (SkModules.IModule Module in menuOptions)
                    {
                        Logz(new string[] { "TOOLBOX", "MODULE", "NOTIFY" }, new string[] { "NAME: " + Module?.ModuleName.ToUpper(), "STATUS: " + Module.ModuleStatus.ToString().ToUpper() });
                        if (Module.ModuleStatus != Status.Ready) // Log any modules that aren't ready
                        {
                            needRetry = true;
                            retryModule.Add(Module);
                        }
                    }

                    if (!needRetry) // Nothing to retry
                    {
                        SkMainStatus = Status.Ready; // Ready
                        errorMonitor = true; // Enable the error monitor
                        retryCount = 1; // Reset the retry counter for later
                    }
                    if (SkMainStatus == Status.Ready && menuOptions.Count > 0)
                    {
                        needLoadModules = false; // Only run this once
                        Logz(new string[] { "TOOLBOX", "NOTIFY" }, new string[] { menuOptions.Count + " MODULES LOADED", "TOOLBOX READY." }); // Notify the console that the menu is ready
                    }
                    else if (SkMainStatus == Status.Error || menuOptions.Count <= 0)
                    {
                        Logz(new string[] { "TOOLBOX", "NOTIFY" }, new string[] { menuOptions.Count + " MODULES LOADED", "TOOLBOX FAILED TO LOAD MODULES." }, LogType.Error); // Notify the console that the menu is ready
                    }
                }
                else if (SkMainStatus == Status.Loading && needRetry) // Need to check the modules again for ready status
                {
                    if (retryCount < (retryCountMax + 1))
                    {
                        for (int x = 0; x < retryModule?.Count; x++)
                        {
                            Logz(new string[] { "TOOLBOX", "MODULE", "NOTIFY", "RECHECK " + retryCount },
                                    new string[] { "NAME: " + retryModule[x].ModuleName.ToUpper(), "STATUS: " + retryModule[x].ModuleStatus.ToString().ToUpper() });
                            if (retryModule[x].ModuleStatus != Status.Ready)
                            {
                                SkMainStatus = Status.Loading;
                                needRetry = true;
                            }
                            else if (retryModule[x].ModuleStatus == Status.Ready)
                            {
                                retryModule.Remove(retryModule[x]);
                                if (retryModule.Count == 0)
                                {
                                    SkMainStatus = Status.Ready;
                                    break;
                                }
                            }
                        }
                        retryCount++;
                    }
                    if (menuOptions.Count <= 0)
                    {
                        SkMainStatus = Status.Error;
                    }

                    if (SkMainStatus == Status.Ready)
                    {
                        errorMonitor = true;
                        retryCount = 1;
                        Logz(new string[] { "TOOLBOX", "NOTIFY" }, new string[] { menuOptions.Count + " MODULES LOADED", "TOOLBOX READY." }); // Notify the console that the menu is ready
                    }
                    else if (retryCount >= (retryCountMax + 1))
                    {
                        Logz(new string[] { "TOOLBOX", "NOTIFY" }, new string[] { "MODULE NOT MOVING TO READY STATUS.", "UNLOADING THE MODULE(S)." }, LogType.Warning); // Notify the console that the menu is ready
                        foreach (SkModules.IModule Module in retryModule)
                        {
                            if (Module.ModuleStatus != Status.Ready)
                            {
                                //Module.RemoveModule();
                                menuOptions.Remove(Module);
                            }
                        }
                        retryModule.Clear();
                        needRetry = false;
                        SkMainStatus = Status.Loading;
                        menuController.UpdateMenuOptions(menuOptions);
                    }
                }
            }
            else
            {
                firstLoad = false;
            }

            if (errorMonitor) // Everything is initialized. Monitor each module for error status and unload if required.
            {
                for (int Module = 0; Module < menuOptions?.Count; Module++)
                {
                    if (menuOptions[Module]?.ModuleStatus == Status.Error && !retryModule.Contains(menuOptions[Module]))
                    {
                        Logz(new string[] { "TOOLBOX", "NOTIFY" }, new string[] { "MODULE IN ERROR STATUS.", "CHECKING MODULE: " + menuOptions[Module].ModuleName.ToUpper() }, LogType.Warning);
                        retryModule.Add(menuOptions[Module]);
                    }
                    else if (menuOptions[Module]?.ModuleStatus == Status.Unload)
                    {
                        Logz(new string[] { "TOOLBOX", "NOTIFY" }, new string[] { "MODULE READY TO UNLOAD. UNLOADING MODULE: " + menuOptions[Module].ModuleName.ToUpper() }, LogType.Warning); // Notify ready to unload
                        //menuOptions[Module].RemoveModule();
                        menuOptions.Remove(menuOptions[Module]);
                        menuController.UpdateMenuOptions(menuOptions);
                    }
                }
                if (retryModule?.Count > 0 && retryCount < (retryCountMax + 1)) // There are modules to check, and we have retry frames available
                {
                    for (int Module = 0; Module < retryModule.Count; Module++)
                    {
                        if (retryModule[Module].ModuleStatus == Status.Ready)
                        {
                            retryModule.Remove(retryModule[Module]);
                            Logz(new string[] { "TOOLBOX", "NOTIFY" }, new string[] { "MODULE READY.", "MODULE: " + menuOptions[Module].ModuleName.ToUpper() });
                            if (retryModule.Count == 0)
                            {
                                break;
                            }
                        }
                    }
                    retryCount++;
                }
                else if (retryModule?.Count > 0 && retryCount >= (retryCountMax + 1)) // No retry frames remaining
                {
                    foreach (SkModules.IModule Module in retryModule)
                    {
                        if (Module.ModuleStatus != Status.Ready)
                        {
                            Logz(new string[] { "TOOLBOX", "NOTIFY" }, new string[] { "COULD NOT RESOLVE ERROR.", "UNLOADING THE MODULE: " + Module.ModuleName.ToUpper() }, LogType.Warning); // Notify the console that the menu is ready
                            //Module.RemoveModule();
                            menuOptions.Remove(Module);
                        }
                    }
                    retryModule.Clear();
                    retryCount = 1;
                    menuController.UpdateMenuOptions(menuOptions);
                    if (menuOptions.Count == 0)
                    {
                        SkMainStatus = Status.Error;
                        Logz(new string[] { "TOOLBOX", "NOTIFY" }, new string[] { "NO MODULES LOADED.", "TOOLBOX ENTERING ERROR STATE." }, LogType.Error); // Notify the console that the menu is ready
                    }

                }
            }

            OnUpdate();
        }

        public List<SkModules.IModule> GetOptions()
        {
            return menuOptions;
        }

        #endregion

        /// <summary>
        /// Each module must have an entry in this method. Each module must be added to this base object as a component, the CallerEntry must be set for that module, and the module must be added to the MenuOptions list.
        /// Example:
        /// moduleTestMenu = gameObject.AddComponent<SkModules.ModTestMenu>(); // Add the module as a component
        /// MenuOptions.Add(moduleTestMenu); // Add the CallerEntry to the Main Menu so the module can be accessed.
        /// </summary>
        public void BeginMainMenu()
        {

            RegisterModules();
            //Create a game object for each module
            //moduleConsole = gameObject.AddComponent<SkModules.ModConsoleOpt>();

            // Add modules to the menu list
            // This is the order the menu items will be shown as well.

            //menuOptions.Add(moduleConsole);
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        private void Init()
        {

        }

        private void OnUpdate()
        {

        }

        public void OnGUI()
        {

        }

        public List<SkModules.IModule> ListModules()
        {
            return menuOptions;
        }

        public void AddModule(SkModules.IModule pmodule)
        {
            if (pmodule.CallerEntry == null)
                pmodule.CallerEntry = new SkMenuItem((pmodule?.CallerEntry?.ItemText?.Length > 0) ? // We have to create a new caller entry to ensure one was either provided
                                                            pmodule.CallerEntry.ItemText : pmodule.ModuleName, // or will have a menu text applied to it.
                                                            () => menuController.RequestSubMenu(pmodule.FlushMenu())); // We also want to make sure the proper menu controller reference is set
            menuOptions.Add(pmodule);

            Logz(new string[] { "TOOLBOX", "NOTIFY" }, new string[] { "Module added.", pmodule.ModuleName });

            menuOptions.Sort((a, b) => a.ModuleName.CompareTo(b.ModuleName));
            menuController.UpdateMenuOptions(menuOptions);
        }

        public void RegisterModules()
        {
            var ourAssemblyList = AppDomain.CurrentDomain.GetAssemblies();

            try
            {
                foreach (Assembly ourAssembly in ourAssemblyList)
                {
                    Type[] theseTypes;
                    try
                    {
                        theseTypes = ourAssembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        theseTypes = e.Types;
                    }

                    foreach (Type foundType in theseTypes)
                    {
                        if (foundType.GetInterfaces().Contains(typeof(SkModules.IModule)) && foundType.GetConstructor(Type.EmptyTypes) != null)
                        {
                            SkModules.IModule module = (SkModules.IModule)Activator.CreateInstance(foundType);

                            if (module.IsEnabled)
                            {
                                if (module.CallerEntry == null)
                                {
                                    module.CallerEntry = new SkMenuItem(module.CallerEntry.ItemText + "\t►", () => menuController.RequestSubMenu(module.FlushMenu()));
                                }
                                menuOptions.Add(module);
                            }
                            else
                            {
                                //module.RemoveModule();
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
            menuOptions.Sort((a, b) => a.ModuleName.CompareTo(b.ModuleName));
        }
    }
}
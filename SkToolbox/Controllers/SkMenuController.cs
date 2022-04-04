using SkToolbox.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SkToolbox.Utility.SkUtilities;

namespace SkToolbox
{
    /// <summary>
    /// Designed to control the menu processes. Menu operations can be requested/performed via this class.
    /// </summary>

    public class SkMenuController : MonoBehaviour
    {
        private string contextTipInfo1 = "NumPad Arrows";
        private string contextTipInfo2 = "NumPad 5 to Select";
        private string contextTipInfo3 = "NumPad . to Back";
        internal static Version SkMenuControllerVersion = new Version(1, 5, 0); // 09/2021

        internal static Status SkMenuControllerStatus = Status.Initialized;

        private readonly string appName = "SkToolbox";
        private readonly string welcomeMsg = "[SkToolbox Loaded]\nPress NumPad 0\nto Toggle Menu.";
        //private readonly string welcomeMsg = "[SkToolbox Loaded]\nPress NumPad 0\nto Acknowledge.";
        private readonly string welcomeMotd = "";

        private bool displayWelcome = false;
        private bool initialCheck = true;
        internal bool logResponse = false;

        private bool menuOpen = false;
        private bool subMenuOpen = false;
        private bool menuProcessInitialOptSize = true; // Scale the main menu on start
        private bool subMenuProcessInitialOptSize = true; // Scale the submenu upon request
        public float inputDelay = -1f; // Set to -1f to disable. Default 0.2f if enabled.
        public float currentInputDelay = 0f;
        private int menuSelection = 1;
        private int subMenuSelection = 1;
        private int maxSelectionOption;
        private int subMaxSelectionOption = 1;
        private int subMenuMaxItemsPerPage = 12;
        private int subMenuCurrentPage = 1;

        public List<SkModules.IModule> menuOptions;
        public List<SkMenuItem> subMenuOptions;
        public List<SkMenuItem> subMenuOptionsDisplay;
        public SkModuleController SkModuleController;

        //GUI Positions
        private int ypos_initial = 0;
        private int ypos_offset = 22;
        private int mWidth = 0; // Main Menu width
        private int subMenu_xpos_offset = 35;
        private int sWidth = 0; // Extra submenu width

        private Color menuOptionHighlight = Color.cyan; // Colorblind friendly colors // Cyan for the currently highlighted item
        private Color menuOptionSelected = Color.yellow; // Set to yellow when the option is actually selected

        //Keycodes
        internal Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>()
        {
            { "selToggle",  KeyCode.Keypad0 },
            { "selUp",      KeyCode.Keypad8 },
            { "selDown",    KeyCode.Keypad2 },
            { "selChoose",  KeyCode.Keypad5 },
            { "selBack",    KeyCode.KeypadPeriod }
        };

        void Start()
        {
            SkMenuControllerStatus = Status.Loading;
            SkUtilities.Logz(new string[] { "CONTROLLER", "NOTIFY" }, new string[] { "LOADING...", "WAITING FOR TOOLBOX." }); // Notify the console that the menu is ready
            SkModuleController = gameObject.AddComponent<SkModuleController>(); // Load our module controller
        }


        void Update()
        {
            if (initialCheck) // It takes a frame to load the components. Attempt to load menu options in second frame.
            {
                if (menuOptions?.Count == 0) // If there is no menu, try to refresh it from SkMain
                { // There will be at least one frame where there is no menu when initialized
                    UpdateMenuOptions(SkModuleController.GetOptions());
                }
                else
                {
                    SkMenuControllerStatus = Status.Ready;
                    if (SkMenuControllerStatus == Status.Ready && SkModuleController.SkMainStatus == Status.Ready)
                    {
                        initialCheck = false;
                        SkUtilities.Logz(new string[] { "CONTROLLER", "NOTIFY" }, new string[] { "READY." }); // Notify the console that the menu is ready
                    }
                }
            }
            //Keycode menu activation
            if (Input.GetKeyDown(keyBindings["selToggle"]))
            {
                displayWelcome = false;
                menuOpen = !menuOpen;
                //MenuOpen = false;
            }
            if (menuOpen) // Menu is open
            {
                if (!subMenuOpen) // Main menu
                {

                    //if (Input.GetKey(keyBindings["selDown"]) && CurrentInputDelay <= 0)
                    if ((inputDelay == -1f && Input.GetKeyDown(keyBindings["selDown"])) || (inputDelay > -1f && Input.GetKey(keyBindings["selDown"]) && currentInputDelay <= 0))
                    {
                        currentInputDelay = inputDelay;
                        subMenuSelection = 1;
                        if (menuSelection != maxSelectionOption)
                        {
                            menuSelection += 1;
                        }
                        else
                        {
                            menuSelection = 1;
                        }
                    }

                    //if (Input.GetKey(keyBindings["selUp"]) && CurrentInputDelay <= 0)
                    if ((inputDelay == -1f && Input.GetKeyDown(keyBindings["selUp"])) || (inputDelay > -1f && Input.GetKey(keyBindings["selUp"]) && currentInputDelay <= 0))
                    {
                        //CurrentInputDelay = InputDelay;
                        subMenuSelection = 1;
                        if (menuSelection != 1)
                        {
                            menuSelection -= 1;
                        }
                        else
                        {
                            menuSelection = maxSelectionOption;
                        }
                    }

                    if (Input.GetKeyDown(keyBindings["selChoose"]))
                    {
                        try
                        {
                            RunMethod(menuOptions[menuSelection - 1].CallerEntry.ItemClass);
                        }
                        catch (Exception ex)
                        {
                            SkUtilities.Logz(new string[] { "CONTROLLER", "ERROR" }, new string[] { ex.Message });
                        }
                    }
                }
                else // We are in the submenu
                {
                    //if (Input.GetKey(keyBindings["selDown"]) && CurrentInputDelay <= 0)
                    if ((inputDelay == -1f && Input.GetKeyDown(keyBindings["selDown"])) || (inputDelay > -1f && Input.GetKey(keyBindings["selDown"]) && currentInputDelay <= 0))
                    {
                        currentInputDelay = inputDelay;
                        if (subMenuSelection != subMaxSelectionOption)
                        {
                            subMenuSelection += 1;
                        }
                        else
                        {
                            subMenuSelection = 1;
                        }
                    }

                    //if (Input.GetKey(keyBindings["selUp"]) && CurrentInputDelay <= 0)
                    if ((inputDelay == -1f && Input.GetKeyDown(keyBindings["selUp"])) || (inputDelay > -1f && Input.GetKey(keyBindings["selUp"]) && currentInputDelay <= 0))
                    {
                        currentInputDelay = inputDelay;
                        if (subMenuSelection != 1)
                        {
                            subMenuSelection -= 1;
                        }
                        else
                        {
                            subMenuSelection = subMaxSelectionOption;
                        }
                    }

                    if (Input.GetKeyDown(keyBindings["selChoose"]))
                    {
                        subMenuOpen = false;
                        //SkUtilities.Logz(new string[] { "CONTROLLER", "SUB SEL" }, new string[] { SubMenuSelection.ToString() });
                        try
                        {
                            RunMethod(subMenuOptionsDisplay[subMenuSelection - 1].ItemClass); // Pass back the method
                        }
                        catch (Exception)
                        {
                            try
                            {
                                if (subMenuOptionsDisplay[subMenuSelection - 1].ItemText.Equals("Next >"))
                                {
                                    IncreasePage();
                                }
                                else if (subMenuOptionsDisplay[subMenuSelection - 1].ItemText.Equals("< Previous"))
                                {
                                    DecreasePage();
                                }
                                else
                                {
                                    RunMethod(subMenuOptionsDisplay[subMenuSelection - 1].ItemClassStr, subMenuOptionsDisplay[subMenuSelection - 1].ItemText); // Pass back the method and string from the menu option

                                }
                            }
                            catch (Exception ex)
                            {
                                SkUtilities.Logz(new string[] { "CONTROLLER", "ERROR" }, new string[] { ex.Message });
                            }
                        }
                    }
                }
                if (Input.GetKeyDown(keyBindings["selBack"])) // Menu is open, but regardless of main or submenu...
                {
                    if (!subMenuOpen)
                    {
                        menuOpen = false;
                    }
                    subMenuOpen = false;
                }
            }
            currentInputDelay -= Time.deltaTime;
            currentInputDelay = Mathf.Clamp(currentInputDelay, 0f, 999f); // Prevent this from dipping beneath 0
        }

        void OnGUI()
        {
            GUI.color = Color.white;

            if (displayWelcome)
            { // Display the greeting message
                //GUI.Box(new Rect(10, ypos_initial + ypos_offset, 150, 55), welcomeMsg);
                var WelcomeWindow = GUILayout.Window(49101, new Rect(7, Screen.height / 2 - 150, 150, 55), ProcessWelcome, "");
            }

            if (menuOptions == null || menuOptions.Count == 0 || ypos_initial == 0) // If there is no menu, try to refresh it from SkMain // Also if the Screen.* was not able to be calculated,
                                                                                    //      ypos_initial will also still be 0, and the menu will appear in the top left corner.
            { // There will be at least one frame where there is no menu when initialized
                UpdateMenuOptions(SkModuleController.GetOptions());
            }
            else // We've received a menu from SkMain and can now display it
            {
                if (menuOpen) // Display the menu components
                {
                    if (menuProcessInitialOptSize) // Calculate the X size of the text and store the highest value for the width
                    {
                        float largestCalculatedWidth = 0;
                        GUIStyle style = GUI.skin.box;
                        // Calculate width
                        foreach (SkModules.IModule optList in menuOptions)
                        {
                            GUIContent menuTextItem = new GUIContent(optList.CallerEntry.ItemText);
                            Vector2 size = style.CalcSize(menuTextItem);
                            if (size.x > largestCalculatedWidth) largestCalculatedWidth = size.x;
                        }
                        mWidth = (mWidth == 0 ? Mathf.CeilToInt(largestCalculatedWidth) : mWidth);
                        mWidth = Mathf.Clamp(mWidth, 125, 1024); // Min/max size

                        menuProcessInitialOptSize = false; // Processing of the main menu size is complete, let's not calculate this every frame...
                    }
                    var MainWindow = GUILayout.Window(49000, new Rect(7, ypos_initial, mWidth + ypos_offset, 30 + (ypos_offset * menuOptions.Count)), ProcessMainMenu, "- [" + appName + "] -");

                    //GUILayout.Window(49002, new Rect(7, MainWindow.y + MainWindow.height + ypos_offset, mWidth + ypos_offset, 30 + (ypos_offset * menuTipSize)), ProcessContextMenu, "- Context -");

                    if (subMenuOpen)
                    {
                        if (subMenuProcessInitialOptSize) // only calculate the size once after the submenu was sent
                        {
                            float largestCalculatedWidth = 0;
                            GUIStyle style = GUI.skin.box;
                            // Calculate width
                            foreach (SkMenuItem optList in subMenuOptions)
                            {
                                GUIContent menuTextItem = new GUIContent(optList.ItemText);
                                Vector2 size = style.CalcSize(menuTextItem);
                                if (size.x > largestCalculatedWidth) largestCalculatedWidth = size.x;
                            }
                            sWidth = (sWidth == 0 ? Mathf.CeilToInt(largestCalculatedWidth) : sWidth);
                            sWidth = Mathf.Clamp(sWidth, 105, 1024); // Min/max width
                        }

                        if (subMenuOptions.Count > subMenuMaxItemsPerPage)
                        { // This will display the submenu box and title. It will also display what page we are on, and the maximum number of pages.
                            GUILayout.Window(49001, new Rect(mWidth + subMenu_xpos_offset, ypos_initial - ypos_offset, sWidth + subMenu_xpos_offset, (30 + (ypos_offset * subMenuMaxItemsPerPage))), ProcessSubMenu,
                                "- Submenu - " + subMenuCurrentPage + "/" + (Mathf.Ceil(subMenuOptions.Count / subMenuMaxItemsPerPage) + (subMenuOptions.Count % subMenuMaxItemsPerPage == 0 ? 0 : 1)));
                        }
                        else
                        {
                            GUILayout.Window(49001, new Rect(mWidth + subMenu_xpos_offset, ypos_initial - ypos_offset, sWidth + subMenu_xpos_offset, (30 + (ypos_offset * subMenuOptions.Count))), ProcessSubMenu, "- Submenu -");
                        }
                    }
                }
            }

            if (Event.current.button == 0
                    && Event.current.type != EventType.Repaint
                    && Event.current.type != EventType.Layout)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    Event.current.Use();
                }
            }

        }

        void ProcessMainMenu(int windowID)
        {
            try
            {
                ProcessMainMenu();
                ProcessContextMenu(windowID);
            }
            catch (Exception ex)
            {
                SkUtilities.Logz(new string[] { "CONTROLLER", "ERROR" }, new string[] { ex.Message });
            }
        }

        void ProcessSubMenu(int windowID)
        {
            try
            {
                ProcessSubMenu(subMenuOptions);
            }
            catch (Exception ex)
            {
                SkUtilities.Logz(new string[] { "CONTROLLER", "ERROR" }, new string[] { ex.Message });
            }
        }

        void ProcessContextMenu(int windowID)
        {
            try
            {
                ProcessMenuTip();
            }
            catch (Exception ex)
            {
                SkUtilities.Logz(new string[] { "CONTROLLER", "ERROR" }, new string[] { ex.Message });
            }
        }

        void ProcessWelcome(int windowID)
        {
            try
            {
                GUILayout.BeginVertical();
                GUILayout.Label(welcomeMsg);
                if (!welcomeMotd.Equals(""))
                {
                    GUILayout.Label(welcomeMotd);
                }
                GUILayout.EndVertical();
            }
            catch (Exception ex)
            {
                SkUtilities.Logz(new string[] { "CONTROLLER", "ERROR" }, new string[] { ex.Message });
            }
        }

        /// <summary>
        /// Process the main menu options, display the background, and display the menu options
        /// </summary>
        private void ProcessMainMenu()
        {
            GUIStyle styMenuItems = new GUIStyle(GUI.skin.box);
            //styMenuItems.normal.background = null;

            GUILayout.BeginVertical();
            for (var i = 0; i < menuOptions.Count; i++)
            {
                if (i == (menuSelection - 1)) // These if statements perform color changing based on selections
                {
                    if (subMenuOpen)
                    {
                        GUI.color = menuOptionSelected;
                    }
                    else
                    {
                        GUI.color = menuOptionHighlight;
                    }
                }
                else
                {
                    GUI.color = Color.white;
                }

                if(GUILayout.Button(menuOptions[i].CallerEntry.ItemText, styMenuItems))
                {
                    subMenuOpen = false;
                    menuSelection = i + 1;
                    try
                    {
                        RunMethod(menuOptions[i].CallerEntry.ItemClass);
                    }
                    catch (Exception ex)
                    {
                        SkUtilities.Logz(new string[] { "CONTROLLER", "ERROR" }, new string[] { ex.Message });
                    }
                }
            }
            GUI.color = Color.white;
            GUILayout.EndVertical();
        }

        public void IncreasePage()
        {
            subMenuCurrentPage++;
            if (subMenuCurrentPage == 2) { subMenuSelection++; }
            subMenuOpen = true;
        }

        public void DecreasePage()
        {
            subMenuCurrentPage--;
            subMenuOpen = true;
        }

        /// <summary>
        /// Used to generate the submenus based on incoming options in the subMenuOptions list. The menu can also be set to refresh automatically. The width can also be set instead of automatically calculated.
        /// </summary>
        /// <param name="subMenuOptions">SkMenuItem List containing the menu text, return methods, and context tips</param>
        /// <param name="refreshTime">If set to >0, the menu will refresh automatically based on the timer. The first method will be called for refresh.</param>
        /// <param name="subWidth">If set to >0, the submenu width will be set manually as opposed to automatic calculation.</param>
        public void RequestSubMenu(List<SkMenuItem> subMenuOptions, float refreshTime = 0, int subWidth = 0)
        {
            if (subMenuOptions != null && subMenuOptions.Count != 0)
            {
                subMenuCurrentPage = 1; // Reset the page to the first
                sWidth = subWidth; // Use custom width if passed in
                subMenuOpen = true; // A submenu was requested, enable it
                subMenuProcessInitialOptSize = true; // Need to calculate sizes of the new submenu. This is later calculated only if we are not using the custom subWidth (subWidth <> 0)

                this.subMenuOptions = subMenuOptions; // Set the submenu options
                subMaxSelectionOption = subMenuOptions.Count; // How many options are there?
                if (subMenuSelection > subMenuOptions.Count) { subMenuSelection = 1; } // Select 1st item if previous selection is out of bounds

                if (refreshTime > 0)
                { // Real time menu? Call the subroutine
                    refreshTime = Mathf.Clamp(refreshTime, 0.01f, 5f);
                    StartCoroutine(RealTimeMenuUpdate(refreshTime));
                }
                else
                { // Don't show the response for realtime menus, as it just spams the log
                    if (logResponse) SkUtilities.Logz(new string[] { "CONTROLLER", "RESP" }, new string[] { "Submenu created." });
                }
            }
        }

        /// <summary>
        /// This is an overload that allows passing in an SkMenu object which contains our list of menu items. Take in the menu, flush the options, pass it into the RequestSubMenu method to handle the items.
        /// </summary>
        /// <param name="subMenuOptions">SkMenuItem List containing the menu text, return methods, and context tips</param>
        /// <param name="refreshTime">If set to >0, the menu will refresh automatically based on the timer. The first method will be called for refresh.</param>
        /// <param name="subWidth">If set to >0, the submenu width will be set manually as opposed to automatic calculation.</param>
        public void RequestSubMenu(SkMenu subMenuOptions, float refreshTime = 0, int subWidth = 0)
        {
            if (subMenuOptions != null)
            {
                RequestSubMenu(subMenuOptions.FlushMenu(), refreshTime, subWidth);
            }
        }

        private void ProcessSubMenu(List<SkMenuItem> subMenuOptions)
        {
            if (subMenuOpen)
            {
                GUIStyle styMenuItems = new GUIStyle(GUI.skin.box);

                /// Todo: Fix this so it doesn't need to run every frame
                if (subMenuOptions.Count > subMenuMaxItemsPerPage) // If there are more items than we can display on one page
                {
                    List<SkMenuItem> tempOptionsList = new List<SkMenuItem>();
                    if (subMenuCurrentPage > 1) // Should we display the previous button?
                    {
                        tempOptionsList.Add(new SkMenuItem("◄\tPrevious Page", () => DecreasePage(), "Previous Page"));
                    }
                    try
                    {
                        for (int x = ((subMenuMaxItemsPerPage * subMenuCurrentPage) - subMenuMaxItemsPerPage); // This will iterate over all items by page. The ternary on next line allows final page to have correct number of elements
                            x < ((subMenuOptions.Count > ((subMenuMaxItemsPerPage * (subMenuCurrentPage + 1)) - subMenuMaxItemsPerPage)) ? ((subMenuMaxItemsPerPage * (subMenuCurrentPage + 1)) - subMenuMaxItemsPerPage) : subMenuOptions.Count);
                            // Example: Is 35 items > (( 10 * (2 + 1)) - 10)? If it is, then there is another page after this one, and we can select a full page worth of items. Otherwise, use the final menu item as the end so we over get an index exception.
                            x++) // This selects the correct menu items to display for this page number
                        {
                            tempOptionsList.Add(subMenuOptions[x]);
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        for (int x = ((subMenuMaxItemsPerPage * subMenuCurrentPage) - subMenuMaxItemsPerPage); x < subMenuOptions.Count - 1; x++) // This selects the correct menu items to display for this page number
                        {
                            tempOptionsList.Add(subMenuOptions[x]);
                        }
                        subMenuProcessInitialOptSize = true;
                    }

                    if ((subMenuOptions.Count > ((subMenuMaxItemsPerPage * (subMenuCurrentPage + 1)) - subMenuMaxItemsPerPage)))
                    { // Should we display the next button?
                        tempOptionsList.Add(new SkMenuItem("Next Page\t►", () => IncreasePage(), "Next Page"));
                    }

                    subMenuOptions = tempOptionsList;
                    subMenuOptionsDisplay = tempOptionsList;
                    subMaxSelectionOption = subMenuOptions.Count; // How many options are there?
                    if (subMenuSelection > subMenuOptions.Count) { subMenuSelection = 1; }
                }
                else
                {
                    subMenuOptionsDisplay = subMenuOptions;
                }

                GUILayout.BeginVertical();

                for (var i = 0; i < subMenuOptions.Count; i++)
                {
                    if (i == (subMenuSelection - 1))
                    {
                        GUI.color = menuOptionHighlight;
                    }
                    else
                    {
                        GUI.color = Color.white;
                    }
                    if(GUILayout.Button(subMenuOptions[i].ItemText, styMenuItems))
                    {
                        subMenuSelection = i + 1;
                        try
                        {
                            RunMethod(subMenuOptionsDisplay[i].ItemClass); // Pass back the method and parameter
                        }
                        catch (Exception)
                        {
                            try
                            {
                                if (subMenuOptionsDisplay[i].ItemText.Equals("Next >"))
                                {
                                    IncreasePage();
                                }
                                else if (subMenuOptionsDisplay[i].ItemText.Equals("< Previous"))
                                {
                                    DecreasePage();
                                }
                                else
                                {
                                    RunMethod(subMenuOptionsDisplay[i].ItemClassStr, subMenuOptionsDisplay[i].ItemText); // Pass back the method and parameter
                                }
                            }
                            catch (Exception ex)
                            {
                                SkUtilities.Logz(new string[] { "CONTROLLER", "ERROR" }, new string[] { ex.Message });
                            }
                        }
                    }
                }
                GUILayout.EndVertical();
                GUI.color = Color.white;

            }
        }

        private void ProcessMenuTip()
        {
            GUIStyle styHeader = new GUIStyle(GUI.skin.label);
            styHeader.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginVertical();
            try
            {
                if (subMenuOpen) // Display tip for submenu items
                {
                    try
                    {
                        if (subMenuOptionsDisplay.Count > 0 && subMenuOptionsDisplay[subMenuSelection - 1] != null && subMenuOptionsDisplay[subMenuSelection - 1].ItemTip != null)
                        {
                            if (!subMenuOptionsDisplay[subMenuSelection - 1].ItemTip.Equals(""))
                            {
                                GUILayout.Label("- Context -", styHeader);
                                GUILayout.Label(subMenuOptionsDisplay[subMenuSelection - 1].ItemTip);
                            }
                        }
                    }
                    catch (NullReferenceException)
                    {
                        //
                    }

                }
                else // Display tip for main menu items
                {
                    if (menuOptions[menuSelection - 1].CallerEntry != null && menuOptions[menuSelection - 1].CallerEntry.ItemTip != null)
                    {
                        if (!menuOptions[menuSelection - 1].CallerEntry.ItemTip.Equals(""))
                        {
                            GUILayout.Label("- Context -", styHeader);
                            GUILayout.Label(menuOptions[menuSelection - 1].CallerEntry.ItemTip);
                        }
                    }
                }

                GUILayout.Label("- Controls -", styHeader);
                GUILayout.Label(contextTipInfo1);

                GUILayout.Label(contextTipInfo2);

                if (subMenuOpen)
                {
                    GUILayout.Label(contextTipInfo3);

                }
            } catch (ArgumentException)
            {
                // This error could be thrown if certain variables change in the middle of a frame. Just suppress it and the new element will show on the following frame.
            }
            GUILayout.EndVertical();
            GUI.color = Color.white;
        }

        private IEnumerator RealTimeMenuUpdate(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            if (subMenuOpen)
            {
                RunMethod(subMenuOptions[subMenuSelection - 1].ItemClass);
            }
        }

        public void UpdateMenuOptions(List<SkModules.IModule> newMenuOptions)
        {
            subMenuOpen = false;
            menuOpen = false;
            menuSelection = 1;
            subMenuSelection = 1;
            menuOptions = newMenuOptions;
            if (menuOptions?.Count > 0)
            {
                ypos_initial = (Screen.height / 2) - (menuOptions.Count / 2 * ypos_offset); // Rescale the Y axis calculation
                maxSelectionOption = menuOptions.Count; // How many options were sent?
            }
            menuProcessInitialOptSize = true; // Initialize the main menu resize on next frame
        }

        private void RunMethod(Action methodName)
        {
            methodName.Invoke();
        }

        private void RunMethod(Action<string> methodName, string methodParameter = "")
        {
            try
            {// Try to invoke with string parameter
                methodName?.Invoke(methodParameter);
            }
            catch (Exception ex)
            {
                SkUtilities.Logz(new string[] { "CONTROLLER", "ERROR" }, new string[] { "Error running method. Likely not found... " + ex.Message }, LogType.Error);
            }
        }

        public void CloseMenu()
        {
            menuOpen = false;
            subMenuOpen = false;
        }
    }
}
using SkToolbox.Settings;
using SkToolbox.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SkToolbox.Controllers
{
    public class MainConsole : MonoBehaviour
    {
        public static MainConsole _instance;

        private bool isVisible = false;
        public bool IsVisible { get => isVisible; }

        // Styles
        private GUIStyle m_StyleOutput = new GUIStyle(GUIStyle.none);
        private GUIStyle m_StyleInput;
        private GUIStyle m_StyleWindow;
        private GUIStyle m_StyleBanner;
        private GUIStyle m_StyleCategoryBanner;
        private GUIStyle m_StylePanelButtons;
        private GUIStyle m_StyleHint;
        public Font font;
        private string bannerUrl = "https://raw.githubusercontent.com/derekShaheen/SkToolbox/main/res/header.png"; //https://github.com/derekShaheen/SkToolbox/blob/main


        private Texture2D bannerTexture;

        //Window
        private Rect m_MainWindow;
        private Rect m_Fullscreen;
        private int m_Xpos = Console.Margin;
        private int m_Ypos = Console.Margin;
        private int m_Height = -1;
        private int m_Width = -1;
        private int m_LineMargin = 11;

        private Vector2 m_LinesScrollPosition = Vector2.zero;

        //Button Panel
        private Vector2 m_LinesScrollPosition2 = Vector2.zero;
        private int m_PanelXSize = 100;
        private bool m_NeedsXAdjustment = true;
        private string m_currentCategory = string.Empty;
        //Text
        private List<string> m_OutputHistory = new List<string>();
        private HistoryController m_InputHistory = new HistoryController();

        //Hint
        private CommandMeta m_currentCommand;

        private string m_currentHint = string.Empty;

        //Input
        private string m_InputString = string.Empty;
        private string m_CurrentString = string.Empty;
        private int m_caretPos = 0;
        private int m_caretPosStored = 0;
        private int m_CurrentSkip = 0;
        private TextEditor m_Editor;

        private bool m_MoveCursorOnNextFrame = false;

        //
        private CommandHandler m_handler;
        public CommandHandler Handler
        {
            get
            {
                if (m_handler == null)
                {
                    m_handler = new CommandHandler(this);
                }
                return m_handler;
            }
            set
            {
                m_handler = value;
            }
        }

        ///
        private const string s_argPattern = @"((?:<[^>]+>)|(?:\[[^\]]+\]))";

        void Start()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            StartCoroutine(WebHandler.GetTextureRequest(bannerUrl, (response) =>
            {
                bannerTexture = response;
            }));

            m_Fullscreen = new Rect(0, 0, Screen.width, Screen.height);

            gameObject.name = "SkConsole";

            HandlePositioning();

            Logger.Submit("Welcome to SkToolbox " + Loaders.SkBepInExLoader.VERSION + "!", false);
            StartCoroutine(Handler.Register());
        }

        void Update()
        {
            if (m_OutputHistory.Count > Console.MaxOutputEntries)
            {
                m_OutputHistory.RemoveAt(0);
            }
            if (isVisible)
            {
                if (Settings.Console.ShowConsole)
                {
                    m_caretPos = m_Editor.cursorIndex;
                    UpdateCommandHint();
                }

                if (m_InputString.EndsWith("`"))
                {
                    isVisible = false;
                    m_InputString = string.Empty;
                }

            }
            HandleKeys();
        }

        private void OnGUI()
        {
            AdjustPanelXSizeIfNeeded();
            HandleGUIEvents();

            if (isVisible)
            {
                Stylize();
                m_StyleWindow = GUI.skin.window;

                if (Settings.Console.DarkenBackground)
                {
                    GUI.Box(m_Fullscreen, "");
                }

                GUI.Box(m_MainWindow, "", m_StyleWindow);
                m_MainWindow = GUILayout.Window(24950, m_MainWindow, DrawWindow, GetWindowTitle(), m_StyleWindow, GetWindowSizeOptions());

                if (ShouldFocusInputBar())
                {
                    GUI.FocusControl("InputBar");
                }

                EatInputInRect(m_MainWindow);
            }
        }

        private void AdjustPanelXSizeIfNeeded()
        {
            if (!m_NeedsXAdjustment || Handler.GetAllCommands().Count == 0) return;

            foreach (KeyValuePair<string, CommandMeta> command in Handler.GetAllCommands())
            {
                GUIContent textItem = new GUIContent(command.Value.data.Keyword);
                Vector2 tempSize = GUI.skin.button.CalcSize(textItem);

                if (tempSize.x + 50 > m_PanelXSize)
                {
                    m_PanelXSize = (int)tempSize.x + 50;
                }
            }

            if (!Console.ShowConsole)
            {
                HandlePositioning(m_PanelXSize);
            }

            m_NeedsXAdjustment = false;
        }

        private string GetWindowTitle()
        {
            return Console.ShowPanel ? string.Empty : "SkToolbox";
        }

        private GUILayoutOption[] GetWindowSizeOptions()
        {
            return new GUILayoutOption[]
            {
        GUILayout.MinWidth(m_Width),
        GUILayout.MaxWidth(m_Width),
            };
        }

        private bool ShouldFocusInputBar()
        {
            return (Settings.Console.ShowConsole && Settings.Console.KeyToggleWindow == KeyCode.BackQuote)
                    || IsPointerOnGUI(Event.current.mousePosition, m_MainWindow);
        }

        private void HandleKeys()
        {
            if (Input.GetKeyDown(Console.KeyToggleWindow))
            {
                isVisible = !isVisible;

                if (!isVisible)
                {
                    m_InputString = string.Empty;
                }
                else
                {
                    ScrollToBottom();
                }
            }
        }

        private bool IsPointerOnGUI(Vector2 vector, Rect rect)
        {
            return rect.Contains(vector);
        }

        private void HandleGUIEvents()
        {
            Event evt = Event.current;

            if (!isVisible)
            {
                return;
            }

            PreventKeyPassthroughs(evt);

            if (evt.isKey)
            {
                HandleKeyEvent(evt);
            }

            PreventClickPassthroughs(evt);
        }

        private void PreventKeyPassthroughs(Event evt)
        {
            if (evt.type == EventType.KeyDown
                && (
                   evt.keyCode == Console.KeyAutoComplete
                || evt.keyCode == KeyCode.UpArrow
                || evt.keyCode == KeyCode.DownArrow
                || evt.keyCode == KeyCode.KeypadEnter
                || evt.keyCode == KeyCode.Return))
            {
                evt.Use();
            }
        }

        private void HandleKeyEvent(Event evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (!m_InputString.Equals(string.Empty))
                    {
                        HandleInput(m_InputString);
                    }
                    break;

                case KeyCode.UpArrow:
                    m_InputString = m_InputHistory.Fetch(m_InputString, true);
                    m_MoveCursorOnNextFrame = true;
                    break;
                case KeyCode.DownArrow:
                    m_InputString = m_InputHistory.Fetch(m_InputString, false);
                    m_MoveCursorOnNextFrame = true;
                    break;
                case KeyCode.Tab:
                    HandleAutoComplete();
                    break;
                default:
                    break;
            }
        }

        private void PreventClickPassthroughs(Event evt)
        {
            bool isMouseClick = evt.type == EventType.MouseDown || evt.type == EventType.MouseUp;

            if (!IsPointerOnGUI(evt.mousePosition, m_MainWindow) || !isMouseClick) return;

            switch (evt.button)
            {
                case 0: // Left mouse button
                case 1: // Right mouse button
                    evt.Use(); // Consume the event
                    break;
                default:
                    break;
            }
        }

        private void HandleAutoComplete()
        {
            if (string.IsNullOrEmpty(m_InputString)) return;

            string[] commands = m_InputString.Split(';');

            if (commands.Length > 1)
            {
                HandleAutoCompleteForMultipleCommands(commands);
            }
            else
            {
                HandleAutoCompleteForSingleCommand(commands);
            }
        }

        private void HandleAutoCompleteForMultipleCommands(string[] commands)
        {
            string lastCommand = commands[commands.Length - 1].Simplified().Split()[0];
            var matchCommand = Handler.GetLikelyCommand(lastCommand);

            if (!string.IsNullOrEmpty(matchCommand.Value?.data?.Keyword))
            {
                m_InputString = string.Join("; ", commands.Take(commands.Length - 1).Select(cmd => cmd.Simplified())) + "; " + matchCommand.Value.data.Keyword;
            }

            m_MoveCursorOnNextFrame = true;
        }

        private void HandleAutoCompleteForSingleCommand(string[] commands)
        {
            string strToCursor = m_InputString.Substring(0, m_caretPos);
            string command = strToCursor.Simplified().Split()[0];
            KeyValuePair<string, CommandMeta> matchCommand;

            if (m_caretPosStored != m_caretPos)
            {
                m_caretPosStored = m_caretPos;
                m_CurrentSkip = 0;
                matchCommand = Handler.GetLikelyCommand(command);
            }
            else
            {
                m_CurrentSkip++;
                matchCommand = Handler.GetLikelyCommand(command, m_CurrentSkip);
                if (string.IsNullOrEmpty(matchCommand.Value?.data?.Keyword))
                {
                    m_CurrentSkip = 0;
                    matchCommand = Handler.GetLikelyCommand(command, m_CurrentSkip);
                }
            }

            bool hasMultiplePossibleCommands = Handler.GetPossibleCommands(command).Skip(1).Any();
            if (!hasMultiplePossibleCommands)
            {
                m_MoveCursorOnNextFrame = true;
            }

            if (!string.IsNullOrEmpty(matchCommand.Value?.data?.Keyword))
            {
                m_InputString = matchCommand.Value.data.Keyword;
            }
        }

        private void DrawWindow(int GuiID)
        {
            if (Console.ShowPanel)
            {
                GUILayout.BeginHorizontal();
                BuildPanel();
            }
            if (Console.ShowConsole)
            {
                GUILayout.BeginVertical(m_StyleInput);

                if (SkVersionChecker.NewVersionAvailable)
                {
                    if (GUILayout.Button("New version (" + SkVersionChecker.latestVersion + ") of " + Loaders.SkBepInExLoader.MODNAME + " (" + SkVersionChecker.currentVersion + ") available on " + SkVersionChecker.ApplicationSource + "!", m_StylePanelButtons))
                    {
                        Application.OpenURL("https://github.com/derekShaheen/SkToolbox/releases");
                    }
                }

                m_LinesScrollPosition = GUILayout.BeginScrollView(m_LinesScrollPosition, false, false,
                    new GUILayoutOption[]
                    {
                        GUILayout.Height(m_Height)
                    });


                GUILayout.FlexibleSpace();
                foreach (string line in m_OutputHistory)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(line, m_StyleOutput);
                    if (GUILayout.Button(" ", GUI.skin.box, GUILayout.Width(22)))
                    {

                        m_InputString = Util.StripTags(line).Trim();
                        m_MoveCursorOnNextFrame = true;
                    };
                    GUILayout.EndHorizontal();
                    GUILayout.Space(m_LineMargin);
                }

                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();

                GUI.SetNextControlName("InputBar");
                m_InputString = GUILayout.TextField(m_InputString, m_StyleInput);
                m_Editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                m_Editor.multiline = false;

                if (m_MoveCursorOnNextFrame)
                {
                    m_Editor.MoveCursorToPosition(new Vector2(0, 9999));
                    m_MoveCursorOnNextFrame = false;

                    m_Editor.SelectNone();
                }
                if (GUILayout.Button("Submit", m_StyleInput, GUILayout.Width(60)))
                {
                    HandleInput(m_InputString);
                }
                GUILayout.EndHorizontal();

                GUILayout.Label(m_currentHint, m_StyleHint);
                GUILayout.EndVertical();
            }
            if (Console.ShowPanel)
            {
                GUILayout.EndHorizontal();
            }
        }

        private void UpdateCommandHint()
        {
            if (!NewInput)
                return;

            if (string.IsNullOrEmpty(m_InputString))
            {
                m_currentHint = string.Empty;
                return;
            }

            if (m_currentCommand != null)
            {
                UpdateCommandHintForCurrentCommand();
            }
            else
            {
                UpdateCommandHintForInputName();
            }
        }

        private void UpdateCommandHintForCurrentCommand()
        {
            if (string.IsNullOrEmpty(m_currentCommand.hint))
            {
                m_currentHint = $"{m_currentCommand.data.Keyword.WithColor(Color.cyan)}\n{m_currentCommand.data.Description}";
                return;
            }

            var splitHints = Util.SplitByPattern(m_currentCommand.hint, s_argPattern);
            var splitArgs = Util.SplitArgs(m_InputString);

            int currentArg = GetCurrentArgumentIndex(splitArgs, splitHints);

            var final = new StringBuilder();
            for (int i = 0; i < splitHints.Count; ++i)
            {
                var hint = splitHints[i];
                if (i == currentArg)
                {
                    final.Append("<b>").Append(hint.WithColor(Color.yellow)).Append("</b> ");
                }
                else
                {
                    final.Append(hint.WithColor(new Color(0.8f, 0.8f, 0.8f))).Append(" ");
                }
            }

            m_currentHint = $"{m_currentCommand.data.Keyword.WithColor(Color.cyan)} {final.ToString().Trim()}\n{m_currentCommand.data.Description}";
        }

        private int GetCurrentArgumentIndex(Match[] splitArgs, List<string> splitHints)
        {
            int currentArg = 0;
            for (int i = 0; i < splitArgs.Length; ++i)
            {
                var match = splitArgs[i];
                var group = match.Groups[0];
                int groupEnd = group.Index + group.Length;

                if (m_caretPos <= groupEnd && i < splitHints.Count)
                {
                    currentArg = i;
                    break;
                }
                else if (m_caretPos > groupEnd && i + 1 < splitHints.Count)
                {
                    currentArg = i + 1;
                }
                else if (i >= splitHints.Count)
                {
                    currentArg = splitHints.Count - 1;
                    break;
                }
            }
            return currentArg;
        }

        private void UpdateCommandHintForInputName()
        {
            m_currentCommand = null;
            m_currentHint = string.Empty;

            string[] commands = m_InputString.Split(';');
            string command = commands[commands.Length - 1].Simplified().Split()[0];
            if (string.IsNullOrEmpty(command))
            {
                return;
            }

            var possibleCommands = Handler.GetPossibleCommands(command);
            var possibleAliasCommands = Handler.GetPossibleAliasCommands(command);

            var hints = new List<string>();
            foreach (var kv in possibleCommands)
            {
                hints.Add(kv.Value.data.Keyword);
            }
            foreach (var kv in possibleAliasCommands)
            {
                hints.Add($"{kv.Key} [Alias]");
            }

            if (hints.Count > 0)
            {
                m_currentHint = string.Join(", ", hints);
            }
        }

        private void BuildPanel()
        {
            GUILayout.BeginVertical(m_StyleInput, new GUILayoutOption[]
            {
                GUILayout.MinWidth(Mathf.Max(m_PanelXSize, 130)), // Must be at least 130
                GUILayout.MaxWidth(Mathf.Max(m_PanelXSize, 130)),
                GUILayout.MaxHeight(m_MainWindow.height),
            });

            if (bannerTexture != null)
            {
                if (GUILayout.Button(bannerTexture, m_StyleBanner))
                {
                    Logger.Submit(SkVersionChecker.currentVersion.ToString() + " on " + Application.productName +
                        "\n" + (SkVersionChecker.NewVersionAvailable ?
                                    ("New Version Available: " + SkVersionChecker.latestVersion).WithColor(Color.yellow) :
                                    "\tUp to date!".WithColor(Color.green)));
                    ScrollToBottom();
                }
            }
            else
            {
                if (GUILayout.Button("<color=#F0D346>SkToolbox</color>", m_StyleBanner))
                {
                    Logger.Submit(SkVersionChecker.currentVersion.ToString() + " on " + Application.productName +
                        "\n" + (SkVersionChecker.NewVersionAvailable ?
                                    ("New Version Available: " + SkVersionChecker.latestVersion).WithColor(Color.yellow) :
                                    "\tUp to date!".WithColor(Color.green)));
                    ScrollToBottom();
                }
            }

            if (Handler.GetAllCommands().Count == 0 && Handler.IsSearching)
            {
                GUILayout.Label("Searching...", m_StylePanelButtons);
            }

            m_LinesScrollPosition2 = GUILayout.BeginScrollView(m_LinesScrollPosition2);

            foreach (KeyValuePair<string, CommandMeta> command in Handler.GetAllCommands())
            {
                if (command.Value.data.DisplayOptions == Util.DisplayOptions.All || command.Value.data.DisplayOptions == Util.DisplayOptions.PanelOnly)
                {
                    if (!command.Value.data.Category.Equals(m_currentCategory))
                    {
                        m_currentCategory = command.Value.data.Category;
                        if (m_currentCategory.Equals("zzBottom"))
                        {
                            GUILayout.Label("");
                        }
                        else
                        {
                            string displayCategory = m_currentCategory.Substring(0, 1).ToUpper() + m_currentCategory.Substring(1);
                            GUILayout.Label("[ " + displayCategory.Trim() + " ]", m_StyleCategoryBanner);
                        }
                    }
                    //Strip prefix '/' if the command contains one, then make it readable for display
                    string buttonText = command.Value.data.Keyword;
                    buttonText = Util.CleanInput(buttonText); // Remove symbols
                    buttonText = Util.ConvertCamelToHuman(buttonText); // Convert to readable

                    if (command.Value.requiredArguments == 0)
                    {
                        if (GUILayout.Button(buttonText, m_StylePanelButtons))
                        {
                            Logger.Submit(command.Value.data.Keyword, false);
                            Handler.Run(command.Value.data.Keyword);
                            ScrollToBottom();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(buttonText, m_StylePanelButtons))
                        {
                            m_InputString = command.Value.data.Keyword + " ";
                            m_Editor.SelectNone();
                            m_MoveCursorOnNextFrame = true;
                            ScrollToBottom();
                        }
                    }
                }
                else
                { // These buttons normally do not show
                    if (!Console.ShowConsole)
                    {
                        if (command.Value.data.Keyword.Equals("OpenConsole"))
                        { // Allow the user to open the console if it's disabled
                            if (GUILayout.Button("Open Console", m_StylePanelButtons))
                            {
                                Handler.Run(command.Value.data.Keyword);
                                ScrollToBottom();
                            }
                        }
                    }
                }
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        public bool NewInput
        {
            get
            {
                if (m_InputString.Equals(m_CurrentString))
                {
                    return false;
                }

                m_CurrentString = m_InputString;
                string[] commands = m_InputString.Split(';');
                string command = commands[commands.Length - 1].Simplified().Split()[0];

                m_currentCommand = Handler.GetCommand(command);
                return true;
            }
        }

        public void Submit(string entry, bool prefix = true)
        {
            if (prefix)
            {
                entry = Console.OutputPrefix + entry;
            }

            m_OutputHistory.Add(entry);
        }

        public void Clear()
        {
            m_OutputHistory.Clear();
            m_InputString = string.Empty;
        }

        public void HandleInput(string consoleInput, bool logToInputHistory = true)
        {
            if (!string.IsNullOrEmpty(consoleInput))
            {
                if (logToInputHistory)
                {
                    m_InputHistory.Add(consoleInput);
                }

                Submit(consoleInput, false);
                Handler.Run(consoleInput);
                m_InputString = string.Empty;
                m_currentCommand = null;
                m_currentHint = string.Empty;
                ScrollToBottom();
            }
        }

        private void ScrollToBottom()
        {
            m_LinesScrollPosition.y = Mathf.Infinity;
        }

        private void Stylize()
        {
            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont("Consolas", Console.FontSize);
            }
            GUI.color = Console.Theme;
            GUI.skin.window.font = font;
            GUI.skin.window.fontStyle = FontStyle.Bold;

            m_StyleOutput.font = font;
            m_StyleOutput.fontSize = font.fontSize;
            m_StyleOutput.richText = true;
            m_StyleOutput.normal.textColor = Color.white;
            m_StyleOutput.wordWrap = true;
            m_StyleOutput.alignment = TextAnchor.UpperLeft;

            if (m_StyleHint == null)
            {
                m_StyleHint = new GUIStyle(m_StyleOutput);
                m_StyleHint.fontSize = 13;
            }

            if (m_StyleInput == null)
            {
                m_StyleInput = new GUIStyle(GUI.skin.box);
                m_StyleInput.alignment = TextAnchor.LowerLeft;
                m_StyleInput.normal.textColor = Color.white;
                //m_StyleInput.fontSize = font.fontSize;
            }

            if (m_StyleBanner == null)
            {
                m_StyleBanner = new GUIStyle(m_StyleInput);
                m_StyleBanner.fontSize = 17;
                m_StyleBanner.fontStyle = FontStyle.BoldAndItalic;
                m_StyleBanner.alignment = TextAnchor.MiddleCenter;
            }

            if (m_StyleCategoryBanner == null)
            {
                m_StyleCategoryBanner = new GUIStyle(m_StyleInput);
                m_StyleCategoryBanner.fontSize = 14;
                m_StyleCategoryBanner.alignment = TextAnchor.MiddleCenter;
                m_StyleCategoryBanner.fontStyle = FontStyle.Bold;
            }

            if (m_StylePanelButtons == null)
            {
                m_StylePanelButtons = new GUIStyle(GUI.skin.textArea);
                m_StylePanelButtons.alignment = TextAnchor.MiddleCenter;
            }
            m_StylePanelButtons.fontSize = font.fontSize;
        }

        public void HandlePositioning(int xOverride = -1, bool panelNeedsXAdjustment = false)
        {
            // Setup sizing
            int margin = Console.Margin * 2;
            m_Width = Console.Width >= 0 ? Console.Width - margin : (Screen.width / Mathf.Abs(Console.Width)) - margin;
            m_Height = Console.Height >= 0 ? Console.Height : (Screen.height / Mathf.Abs(Console.Height) - 125);

            // Set min/max sizing
            m_Width = Mathf.Clamp(m_Width, 320, Screen.width);
            m_Height = Mathf.Clamp(m_Height, 240, Screen.height);

            // Setup positioning
            int centerX = (Screen.width / 2) - (m_Width / 2);
            int centerY = (Screen.height / 2) - (m_Height / 2);
            int bottomY = Screen.height - m_Height - Console.Margin - 125;

            switch (Console.Position)
            {
                case Console.ConsolePos.TopCentered:
                    m_Xpos = centerX;
                    m_Ypos = Console.Margin;
                    break;
                case Console.ConsolePos.LeftCentered:
                    m_Xpos = Console.Margin;
                    m_Ypos = centerY;
                    break;
                case Console.ConsolePos.RightCentered:
                    m_Xpos = Screen.width - m_Width - Console.Margin;
                    m_Ypos = centerY;
                    break;
                case Console.ConsolePos.Centered:
                    m_Xpos = centerX;
                    m_Ypos = centerY;
                    break;
                case Console.ConsolePos.TopLeft:
                    m_Xpos = Console.Margin;
                    m_Ypos = Console.Margin;
                    break;
                case Console.ConsolePos.TopRight:
                    m_Xpos = Screen.width - m_Width - Console.Margin;
                    m_Ypos = Console.Margin;
                    break;
                case Console.ConsolePos.BottomLeft:
                    m_Xpos = Console.Margin;
                    m_Ypos = bottomY;
                    break;
                case Console.ConsolePos.BottomRight:
                    m_Xpos = Screen.width - m_Width - Console.Margin;
                    m_Ypos = bottomY;
                    break;
                case Console.ConsolePos.BottomCentered:
                    m_Xpos = centerX;
                    m_Ypos = bottomY;
                    break;
                default:
                    break;
            }

            if (xOverride > 0)
            {
                m_Width = xOverride;
            }

            m_MainWindow = new Rect(m_Xpos, m_Ypos, m_Width, m_Height);

            if (panelNeedsXAdjustment) { m_NeedsXAdjustment = true; }
        }

        public CommandHandler GetCommandHandler()
        {
            return Handler;
        }

        private void EatInputInRect(Rect eatRect)
        {
            var mousePos = Input.mousePosition;
            if (eatRect.Contains(new Vector2(mousePos.x, Screen.height - mousePos.y)))
                Input.ResetInputAxes();
        }

        /// <summary>
        /// A history controller that stores a list of items and allows fetching the previous and next item in the list.
        /// </summary>
        private class HistoryController
        {
            public List<string> history = new List<string>();
            private int index;
            private string current;

            public void Add(string item)
            {
                history.Add(item);
                index = 0;
            }

            public string Fetch(string current, bool next)
            {
                if (index == 0)
                {
                    this.current = current;
                }
                if (history.Count == 0)
                {
                    return current;
                }
                index += ((!next) ? 1 : -1);
                if (history.Count + index < 0 || history.Count + index > history.Count - 1)
                {
                    this.index = 0;
                    return this.current;
                }
                return history[history.Count + index];
            }
        }
    }
}
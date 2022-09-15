using SkToolbox.Settings;
using SkToolbox.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace SkToolbox.Controllers
{
    public class MainConsole : MonoBehaviour
    {
        public static MainConsole _instance;
        public Loaders.SkBepInExLoader SkBepInExLoader;

        private bool isVisible = false;
        public bool IsVisible { get => isVisible; }

        // Styles
        public GUIStyle m_StyleOutput = new GUIStyle(GUIStyle.none);
        private GUIStyle m_StyleInput;
        private GUIStyle m_StyleWindow;
        private GUIStyle m_StyleBanner;
        private GUIStyle m_StyleCategoryBanner;
        private GUIStyle m_StylePanelButtons;
        private GUIStyle m_StyleHint;
        Font font;

        //Window
        private Rect m_MainWindow;
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
                if(m_handler == null)
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

            gameObject.name = "SkConsole";

            font = Font.CreateDynamicFontFromOSFont("Consolas", Console.FontSize);

            HandlePositioning();

            Logger.Submit("Welcome to SkToolbox " + SkVersionChecker.currentVersion + "!", false);
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
                m_caretPos = m_Editor.cursorIndex;
                UpdateCommandHint();
            }
            HandleKeys();
        }

        private void OnGUI()
        {
            if (m_NeedsXAdjustment) // Run once
            {
                if (Handler.GetAllCommands().Count > 0)
                {
                    foreach (KeyValuePair<string, CommandMeta> command in Handler.GetAllCommands())
                    {

                        GUIContent textItem = new GUIContent(command.Value.data.keyword);
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
            }

            HandleGUIEvents();
            if (isVisible)
            {
                Stylize();

                m_StyleWindow = GUI.skin.window;
                if (Console.ShowPanel)
                {
                    m_MainWindow = GUILayout.Window(24950, m_MainWindow, DrawWindow, string.Empty, m_StyleWindow, new GUILayoutOption[]
                    {
                        GUILayout.MinWidth(m_Width),
                        GUILayout.MaxWidth(m_Width),
                    });
                } else
                {
                    m_MainWindow = GUILayout.Window(24950, m_MainWindow, DrawWindow, "SkToolbox", m_StyleWindow, new GUILayoutOption[]
                    {
                        GUILayout.MinWidth(m_Width),
                        GUILayout.MaxWidth(m_Width),
                    });
                }


                if (IsPointerOnGUI(Event.current.mousePosition, m_MainWindow))
                {
                    GUI.FocusControl("InputBar");
                }
            }
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

            // Prevent pressthroughs
            if (Event.current.type == EventType.KeyDown
                && (
                   Event.current.keyCode == Console.KeyAutoComplete
                || Event.current.keyCode == KeyCode.UpArrow
                || Event.current.keyCode == KeyCode.DownArrow
                || Event.current.keyCode == KeyCode.KeypadEnter
                || Event.current.keyCode == KeyCode.Return))
            {
                Event.current.Use();
            }

            if (evt.isKey)
            {
                switch (evt.keyCode)
                {
                    case KeyCode.Return:
                        if (!m_InputString.Equals(string.Empty))
                        {
                            HandleInput(m_InputString);
                        }
                        break;
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

            switch (Event.current.button)
            {
                case 0: // Left mouse button // Prevent clickthroughs
                    if (IsPointerOnGUI(Event.current.mousePosition, m_MainWindow) && (Event.current.type == EventType.MouseDown
                                                                                    || Event.current.type == EventType.MouseUp))
                    {
                        Event.current.Use(); // Consume the event
                    }
                    break;
                case 1://Right mouse button // Prevent clickthroughs
                    if (IsPointerOnGUI(Event.current.mousePosition, m_MainWindow) && (Event.current.type == EventType.MouseDown
                                                                                    || Event.current.type == EventType.MouseUp))
                    {
                        Event.current.Use(); // Consume the event
                    }
                    break;
                default:
                    break;
            }

        }

        private void HandleAutoComplete()
        {
            if (!string.IsNullOrEmpty(m_InputString))
            {
                string[] commands = m_InputString.Split(';');
                if (commands.Length > 1)
                {
                    string command = commands[commands.Length - 1].Simplified().Split()[0];
                    var matchCommand = Handler.GetLikelyCommand(command);

                    if (!string.IsNullOrEmpty(matchCommand.Value?.data?.keyword))
                    {
                        m_InputString = string.Empty;
                        for (int i = 0; i < commands.Length - 1; i++) // Select the whole length other than the last command
                        {
                            m_InputString = m_InputString + commands[i].Simplified() + "; ";
                        }
                        m_InputString = m_InputString + matchCommand.Value.data.keyword;
                    }
                    m_MoveCursorOnNextFrame = true;
                }
                else
                {
                    string strToCursor = m_InputString.Substring(0, m_caretPos);
                    string command = strToCursor.Simplified().Split()[0];
                    KeyValuePair<string, CommandMeta> matchCommand = new KeyValuePair<string, CommandMeta>();
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
                        if (string.IsNullOrEmpty(matchCommand.Value?.data?.keyword))
                        { // Did we loop the whole list? Go back to the start
                            m_CurrentSkip = 0;
                            matchCommand = Handler.GetLikelyCommand(command, m_CurrentSkip);
                        }
                    }

                    int tempCounter = 0;
                    // Are there more than 1 possible commands found?
                    foreach (var possibleCommands in Handler.GetPossibleCommands(command))
                    {
                        tempCounter++;
                        if (tempCounter > 1)
                        {
                            break;
                        }
                    }
                    // If there's only one command, then move the cursor to the end 
                    if (tempCounter == 1)
                    {
                        m_MoveCursorOnNextFrame = true;
                    }

                    if (!string.IsNullOrEmpty(matchCommand.Value?.data?.keyword))
                    {
                        m_InputString = string.Empty;
                        for (int i = 0; i < commands.Length - 1; i++) // Select the whole length other than the last command
                        {
                            m_InputString = m_InputString + commands[i].Simplified() + "; ";
                        }
                        m_InputString = m_InputString + matchCommand.Value.data.keyword;
                    }
                }
            }
            else
            {
                m_InputString = m_InputHistory.Fetch(m_InputString, true).Split()[0] + " ";
                m_MoveCursorOnNextFrame = true;
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
                        m_InputString = line;
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

        // Code sampled from https://github.com/zambony/Gungnir
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

                if (string.IsNullOrEmpty(m_currentCommand.hint))
                {
                    m_currentHint = $"{(m_currentCommand.data.keyword).WithColor(Color.cyan)}\n{m_currentCommand.data.description}";
                    return;
                }

                // Split the hint string into pieces so we can add color or boldness to each one.
                var splitHints = Util.SplitByPattern(m_currentCommand.hint, s_argPattern);
                // Split each argument manually, because we SplitByQuotes does not preserve the quotations, or return the match list.
                // We want the quotation marks so we can see exactly where in the string an argument starts/ends.
                var splitArgs = Util.SplitArgs(m_InputString);

                int currentArg = 0;

                string final = "";

                for (int i = 0; i < splitArgs.Length; ++i)
                {
                    var match = splitArgs[i];
                    var group = match.Groups[0];
                    int groupEnd = group.Index + group.Length;

                    // If our caret is before the end of this argument, and the argument the caret is touching
                    // is not beyond the number of argument hints we have, select this argument as the one to highlight.
                    if (m_caretPos <= groupEnd && i < splitHints.Count)
                    {
                        currentArg = i;
                        break;
                    }
                    // If our caret is past the last character of our current argument (e.g. the user inserted a space
                    // after the argument), assume the next argument is what we're targeting, but only if there is a next argument to
                    // use.
                    else if (m_caretPos > groupEnd && i + 1 < splitHints.Count)
                    {
                        currentArg = i + 1;
                    }
                    // Otherwise, we're probably entering an array argument and typing multiple things, so mark the last argument
                    // as the current one.
                    else if (i >= splitHints.Count)
                    {
                        currentArg = splitHints.Count - 1;
                        break;
                    }
                }

                // Apply coloring/highlighting to each hint.
                for (int i = 0; i < splitHints.Count; ++i)
                {
                    var hint = splitHints[i];

                    if (i == currentArg)
                        final += "<b>" + hint.WithColor(Color.yellow) + "</b> ";
                    else
                        final += hint.WithColor(new Color(0.8f, 0.8f, 0.8f)) + " ";
                }

                m_currentHint = $"{(m_currentCommand.data.keyword).WithColor(Color.cyan)} {final.Trim()}\n{m_currentCommand.data.description}";
            }
            else
            {
                m_currentCommand = null;
                m_currentHint = string.Empty;
                string[] commands = m_InputString.Split(';');
                string command = commands[commands.Length - 1].Simplified().Split()[0];
                if (!string.IsNullOrEmpty(command))
                {
                    foreach (KeyValuePair<string, CommandMeta> kv in Handler.GetPossibleCommands(command))
                    {
                        m_currentHint = m_currentHint + kv.Value.data.keyword + ", ";
                    }
                    if (m_currentHint.EndsWith(", "))
                    {
                        m_currentHint = m_currentHint.Substring(0, m_currentHint.Length - 2);
                    }
                }
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

            if (GUILayout.Button("<color=#F0D346>SkToolbox</color>", m_StyleBanner))
            {
                Logger.Submit(SkVersionChecker.currentVersion.ToString() + " on " + Application.productName);
                ScrollToBottom();
            }

            m_LinesScrollPosition2 = GUILayout.BeginScrollView(m_LinesScrollPosition2);

            foreach (KeyValuePair<string, CommandMeta> command in Handler.GetAllCommands())
            {
                if (command.Value.data.displayOnPanel)
                {
                    if (!command.Value.data.category.Equals(m_currentCategory))
                    {
                        m_currentCategory = command.Value.data.category;
                        if (m_currentCategory.Equals("zzzzz"))
                        {
                            GUILayout.Label("");
                        }
                        else
                        {
                            GUILayout.Label("[ " + m_currentCategory.Trim() + " ]", m_StyleCategoryBanner);
                        }
                    }
                    //Strip prefix '/' if the command contains one, then make it readable for display
                    string buttonText = command.Value.data.keyword;
                    buttonText = Util.CleanInput(buttonText); // Remove symbols
                    buttonText = Util.ConvertCamelToHuman(buttonText); // Convert to readable

                    if (command.Value.requiredArguments == 0)
                    {
                        if (GUILayout.Button(buttonText, m_StylePanelButtons))
                        {
                            Logger.Submit(command.Value.data.keyword, false);
                            Handler.Run(command.Value.data.keyword);
                            ScrollToBottom();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(buttonText, m_StylePanelButtons))
                        {
                            m_InputString = command.Value.data.keyword + " ";
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
                        if (command.Value.data.keyword.Equals("OpenConsole"))
                        { // Allow the user to open the console if it's disabled
                            if (GUILayout.Button("Open Console", m_StylePanelButtons))
                            {
                                Handler.Run(command.Value.data.keyword);
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
                if (!m_CurrentString.Equals(m_InputString))
                {
                    m_CurrentString = m_InputString;
                    string[] commands = m_InputString.Split(';');
                    string command = commands[commands.Length - 1].Simplified().Split()[0];

                    m_currentCommand = Handler.GetCommand(command);

                    return true;
                }
                return false;
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
            font = Font.CreateDynamicFontFromOSFont("Consolas", Console.FontSize);
            GUI.skin.window.fontStyle = FontStyle.Bold;

            m_StyleOutput.font = font;
            m_StyleOutput.fontSize = font.fontSize;
            m_StyleOutput.richText = true;
            m_StyleOutput.normal.textColor = Color.white;
            m_StyleOutput.wordWrap = true;
            m_StyleOutput.alignment = TextAnchor.UpperLeft;

            m_StyleHint = new GUIStyle(m_StyleOutput);
            m_StyleHint.fontSize = 13;

            m_StyleInput = new GUIStyle(GUI.skin.box);
            m_StyleInput.alignment = TextAnchor.LowerLeft;

            m_StyleInput.fontSize = font.fontSize;
            m_StyleInput.normal.textColor = Color.white;

            m_StyleBanner = new GUIStyle(m_StyleInput);
            m_StyleBanner.fontSize = 17;
            m_StyleBanner.fontStyle = FontStyle.BoldAndItalic;
            m_StyleBanner.alignment = TextAnchor.MiddleCenter;

            m_StyleCategoryBanner = new GUIStyle(m_StyleInput);
            m_StyleCategoryBanner.fontSize = 14;
            m_StyleCategoryBanner.alignment = TextAnchor.MiddleCenter;
            m_StyleCategoryBanner.fontStyle = FontStyle.Bold;

            m_StylePanelButtons = new GUIStyle(GUI.skin.textArea);
            m_StylePanelButtons.fontSize = font.fontSize;
            m_StylePanelButtons.alignment = TextAnchor.MiddleCenter;

        }

        public void HandlePositioning(int xOverride = -1, bool panelNeedsXAdjustment = false)
        {
            // Setup sizing
            if (Console.Width == 0) Console.Width = -1;
            if (Console.Height == 0) Console.Height = -1;

            if (Console.Width >= 0)
            {
                m_Width = Console.Width - (Console.Margin * 2);

            }
            else
            {
                m_Width = (Screen.width / Mathf.Abs(Console.Width)) - (Console.Margin * 2);
            }

            if (Console.Height >= 0)
            {
                m_Height = Console.Height;
            }
            else
            {
                m_Height = (Screen.height / Mathf.Abs(Console.Height) - 125);
            }
            // Set min/max sizing
            m_Width = Mathf.Clamp(m_Width, 320, Screen.width);
            m_Height = Mathf.Clamp(m_Height, 240, Screen.height);

            // Setup positioning
            switch (Console.Position)
            {
                case Console.ConsolePos.TopCentered:
                    m_Xpos = (Screen.width / 2) - (m_Width / 2);
                    m_Ypos = Console.Margin;
                    break;
                case Console.ConsolePos.LeftCentered:
                    m_Xpos = Console.Margin;
                    m_Ypos = (Screen.height / 2) - (m_Height / 2);
                    break;
                case Console.ConsolePos.RightCentered:
                    m_Xpos = Screen.width - m_Width - Console.Margin;
                    m_Ypos = (Screen.height / 2) - (m_Height / 2);
                    break;
                case Console.ConsolePos.Centered:
                    m_Xpos = (Screen.width / 2) - (m_Width / 2);
                    m_Ypos = (Screen.height / 2) - (m_Height / 2);
                    break;
                case Console.ConsolePos.TopLeft:
                    m_Xpos = Console.Margin;
                    m_Ypos = Console.Margin;
                    break;
                case Console.ConsolePos.TopRight:
                    m_Xpos = (Screen.width / 2) - (m_Width / 2);
                    m_Ypos = Console.Margin;
                    break;
                case Console.ConsolePos.BottomLeft:
                    m_Xpos = Console.Margin;
                    m_Ypos = (Screen.height - m_Height - Console.Margin - 125);
                    break;
                case Console.ConsolePos.BottomRight:
                    m_Xpos = Screen.width - m_Width - Console.Margin;
                    m_Ypos = (Screen.height - m_Height - Console.Margin - 125);
                    break;
                case Console.ConsolePos.BottomCentered:
                    m_Xpos = (Screen.width / 2) - (m_Width / 2);
                    m_Ypos = (Screen.height - m_Height - Console.Margin - 125);
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
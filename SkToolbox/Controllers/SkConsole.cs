using SkToolbox.Commands;
using SkToolbox.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A console to display Unity's debug logs in-game.
/// Credit: github. com/ mminer/ consolation - Thank you for this base code!
/// </summary>
namespace SkToolbox
{
    internal class SkConsole : MonoBehaviour
    {
        public static Version SkConsoleVersion = new Version(1, 5, 0);

        #region Inspector Settings

        /// <summary>
        /// The hotkey to show and hide the console window.
        /// </summary>
        public KeyCode toggleKey = KeyCode.BackQuote;

        /// <summary>
        /// Whether to open as soon as the game starts.
        /// </summary>
        public bool openOnStart = false;

        /// <summary>
        /// Whether to open the window by shaking the device (mobile-only).
        /// </summary>
        public bool shakeToOpen = false;

        /// <summary>
        /// Also require touches while shaking to avoid accidental shakes.
        /// </summary>
        public bool shakeRequiresTouch = false;

        /// <summary>
        /// The (squared) acceleration above which the window should open.
        /// </summary>
        public float shakeAcceleration = 3f;

        /// <summary>
        /// The number of seconds that have to pass between visibility toggles.
        /// This threshold prevents closing again while shaking to open.
        /// </summary>
        public float toggleThresholdSeconds = .5f;
        private float lastToggleTime;

        /// <summary>
        /// Whether to only keep a certain number of logs, useful if memory usage is a concern.
        /// </summary>
        public bool restrictLogCount = true;

        /// <summary>
        /// Number of logs to keep before removing old ones.
        /// </summary>
        public int maxLogCount = 250;

        /// <summary>
        /// Font size to display log entries with.
        /// </summary>
        public int logFontSize = 14;
        private int logLineNumber = 0;
        /// <summary>
        /// Amount to scale UI by.
        /// </summary>
        public float scaleFactor = 1f;

        public static bool writeToFile = false;

        public static bool cursorUnlock = false;

        public static bool enableInput = true;
        #endregion

        private string logSavePath;
        private History consoleHistory = new History();

        private const string windowTitle = "SkToolbox [Console]";
        private static readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
        private static readonly GUIContent submitLabel = new GUIContent("Submit", "Submit the input.");
        private static readonly GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");
        private static string inputString = string.Empty;
        private static List<Commands.SkCommand> matchCommandList = new List<Commands.SkCommand>();
        private static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>
        {
            { LogType.Log, Color.white },
            { LogType.Assert, Color.white },
            { LogType.Warning, Color.yellow },
            { LogType.Error, Color.red },
            { LogType.Exception, Color.red },
        };
        private bool isCollapsed;
        private bool isVisible;
        private bool announcedCmds = false;
        private Font customFont;
        private readonly List<Log> logs = new List<Log>();
        private readonly ConcurrentQueue<Log> queuedLogs = new ConcurrentQueue<Log>();
        private Vector2 scrollPosition;
        private readonly Rect titleBarRect = new Rect(0, 0, 10000, 21);
        private const int margin = 150;
        private float windowX = margin;
        private float windowY = margin;
        private float width;
        private float height;

        private TextEditor editor;
        private string suggestedCommands = string.Empty;
        private string previousInput = string.Empty;
        private string hints = string.Empty;
        private SkCommand suggestedCommand = null;

        private bool singleFrame = true;
        //private bool mouseOnWindow = false;

        public bool NewInput
        {
            get
            {
                if (!previousInput.Equals(inputString))
                {
                    previousInput = inputString;
                    return true;
                }
                return false;
            }
        }

        private List<LogType> lFKeys;

        private readonly Dictionary<LogType, bool> logTypeFilters = new Dictionary<LogType, bool>
        {
            { LogType.Log, true },
            { LogType.Assert, false },
            { LogType.Warning, false },
            { LogType.Error, false },
            { LogType.Exception, false },
        };

        #region MonoBehaviour Messages

        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= HandleLogThreaded;
        }

        private void OnEnable()
        {
            Application.logMessageReceivedThreaded += HandleLogThreaded;
        }

        private void Start()
        {
            lFKeys = logTypeFilters.Keys.ToList();
            logSavePath = Application.persistentDataPath + "/!SkToolbox Console Log.txt";
            if (openOnStart)
            {
                isVisible = true;
            }
            SkCommandProcessor.Instance.DiscoverCommands();

            customFont = Font.CreateDynamicFontFromOSFont("Consolas.ttf", logFontSize);
            width = (Screen.width / scaleFactor) - (margin * 2);
            height = (Screen.height / scaleFactor) - (margin * 2);
            width = Mathf.Clamp(width, 100, 1280);
            height = Mathf.Clamp(height, 100, 720);
            windowX = (Screen.width / 2) - (width / 2);
            windowY = (Screen.height / 2) - (height / 2);
        }

        private void OnGUI()
        {
            Event evt = Event.current;
            if (evt.isKey)
            {
                switch (evt.keyCode)
                {
                    case KeyCode.BackQuote:
                        isVisible = !isVisible;
                        inputString = string.Empty;
                        break;
                    default:
                        break;
                }
            }

            if (!isVisible)
            {
                return;
            }
            else
            {
                if (enableInput)
                    GUI.FocusControl("inputBar");

            }

            if (evt.isKey && singleFrame && enableInput)
            {
                switch (evt.keyCode)
                {
                    case KeyCode.UpArrow:
                        inputString = consoleHistory.Fetch(inputString, true);
                        editor.MoveCursorToPosition(new Vector2(0, 9999));
                        //Console.instance.m_input.caretPosition = Console.instance.m_input.text.Length;
                        break;
                    case KeyCode.DownArrow:
                        inputString = consoleHistory.Fetch(inputString, false);
                        break;
                    case KeyCode.Return:
                        //if (inputString.Equals(string.Empty) && !consoleLastMessage.Equals(string.Empty))
                        if (!inputString.Equals(string.Empty))
                        {
                            HandleInput(inputString);
                            ScrollToBottom();
                        }
                        break;
                    case KeyCode.KeypadEnter:
                        if (!inputString.Equals(string.Empty))
                        {
                            HandleInput(inputString);
                            ScrollToBottom();
                        }
                        break;
                    case KeyCode.Tab:
                        if (!string.IsNullOrEmpty(inputString))
                        {
                            singleFrame = false;
                            try
                            {
                                string[] inputSplit = inputString.Split(';');
                                string inputSplit2 = inputSplit[inputSplit.Length - 1].Trim().Split(' ')[0];
                                Commands.SkCommand matchCommand = SkCommandProcessor.Instance.CommandList.Where(cmd => cmd.Command.ToLower().StartsWith(inputSplit2.ToLower(), StringComparison.InvariantCultureIgnoreCase)
                                                                                                                    && !cmd.Command.ToLower().Equals(inputSplit2.ToLower())
                                                                                                                    ).First();

                                if (!string.IsNullOrEmpty(matchCommand.Command))
                                {
                                    if (inputSplit.Length > 1)
                                    {
                                        inputString = string.Empty;
                                        for (int x = 0; x < inputSplit.Length - 1; x++)
                                        {
                                            inputString += inputSplit[x].Trim() + "; ";
                                        }
                                        inputString += matchCommand.Command;
                                    }
                                    else
                                    {
                                        inputString = matchCommand.Command;
                                    }
                                }
                                editor.MoveCursorToPosition(new Vector2(0, 9999));
                            }
                            catch (Exception)
                            {
                                //
                            }
                        }
                        break;
                    default:
                        break;
                }
                Event.current.Use();
            }

            GUI.matrix = Matrix4x4.Scale(Vector3.one * scaleFactor);


            Rect windowRect = new Rect(windowX, windowY, width, height);

            Rect newWindowRect = GUILayout.Window(8675309, windowRect, DrawWindow, windowTitle);
            windowX = newWindowRect.x;
            windowY = newWindowRect.y;

            switch (Event.current.button)
            {
                //case 0: // Left mouse button
                //    //if(mouseOnWindow && Event.current.type == EventType.MouseDown)
                //    //{
                //    //    Event.current.Use();
                //    //}
                //    break;
                case 1://Right mouse button window drag - resize
                    if (Event.current.type == EventType.MouseDrag)
                    {
                        width += Event.current.delta.x;
                        height += Event.current.delta.y;
                        width = Mathf.Clamp(width, 426, 9999);
                        height = Mathf.Clamp(height, 240, 9999);
                        Event.current.Use();
                    }
                    break;
            }

            if (cursorUnlock)
            {
                unlockCursor();
            }

            if (Event.current.type == EventType.KeyUp
                  && enableInput
                  && (Event.current.keyCode == KeyCode.Tab
                        || Event.current.keyCode == KeyCode.UpArrow
                        || Event.current.keyCode == KeyCode.DownArrow))
            {
                singleFrame = true;
                editor.MoveCursorToPosition(new Vector2(0, 9999));
                Event.current.Use();
            }
        }

        private void Update()
        {
            if ((!announcedCmds && SkMenuController.SkMenuControllerStatus == SkUtilities.Status.Ready) || (!announcedCmds && !Loaders.SkLoader.MenuEnabled))
            {
                Utility.SkUtilities.Logz(new string[] { "TOOLBOX", "CONSOLE", "NOTIFY" }, new string[] { SkCommandProcessor.Instance.CommandList.Count + " COMMANDS LOADED", "STATUS: READY" });
                announcedCmds = true;
            }
            UpdateQueuedLogs();

            float curTime = Time.realtimeSinceStartup;


            if (Input.GetKeyDown(toggleKey))
            {
                isVisible = !isVisible;
                if (!isVisible)
                {
                    inputString = string.Empty;
                }
                else
                {
                    ScrollToBottom();
                    GUI.FocusControl("inputBar");
                }
            }

            if (isVisible)
            {
                if (!string.IsNullOrEmpty(inputString))
                {
                    if (!NewInput)// Calculate suggestions
                    {
                        suggestedCommands = string.Empty;
                        string[] commandsInInput = editor.text.Split(';');
                        string commandOnRight = commandsInInput[commandsInInput.Length - 1].Trim().Split(' ')[0];
                        //Commands.SkCommand matchCommand = SkCommandProcessor.Instance.CommandList.Find(cmd => cmd.Command.ToLower().Contains(inputSplit2.ToLower()));
                        matchCommandList = SkCommandProcessor.Instance.CommandList.Where(cmd =>
                                                                           cmd.Command.ToLower().Contains(commandOnRight.ToLower())
                                                                        && cmd.Enabled
                                                                        && cmd.VisibilityFlag != Commands.SkCommandEnum.VisiblityFlag.Hidden
                                                                        && cmd.VisibilityFlag != Commands.SkCommandEnum.VisiblityFlag.FullHidden
                                                                        //&& !cmd.Command.ToLower().Equals(commandOnRight.ToLower())
                                                                        ).ToList();
                        foreach (Commands.SkCommand cmd in matchCommandList)
                        {
                            suggestedCommands += cmd.Command + " "; // Change suggested commands to buttons instead of labels
                        }
                        if (matchCommandList.Count == 1)
                        {
                            suggestedCommand = matchCommandList[0];
                        }
                    }
                }
                else
                {
                    suggestedCommands = string.Empty;
                    suggestedCommand = null;
                }
            }

            if (shakeToOpen &&
                Input.acceleration.sqrMagnitude > shakeAcceleration &&
                curTime - lastToggleTime >= toggleThresholdSeconds &&
                (!shakeRequiresTouch || Input.touchCount > 2))
            {
                isVisible = !isVisible;
                lastToggleTime = curTime;
            }
        }

        #endregion

        private void DrawLog(Log log, GUIStyle logStyle, GUIStyle badgeStyle)
        {
            GUI.contentColor = logTypeColors[log.type];

            if (isCollapsed)
            {
                // Draw collapsed log with badge indicating count.
                GUILayout.BeginHorizontal();
                GUILayout.Label(log.GetTruncatedMessage(), logStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label(log.count.ToString(), GUI.skin.box);
                GUILayout.EndHorizontal();
            }
            else
            {
                // Draw expanded log.
                for (int i = 0; i < log.count; i += 1)
                {
                    GUILayout.Label(log.GetTruncatedMessage(), logStyle);
                }
            }

            GUI.contentColor = Color.white;
        }

        private void DrawLogList()
        {
            GUIStyle badgeStyle = GUI.skin.box;
            badgeStyle.fontSize = logFontSize;
            try
            {
                badgeStyle.font = customFont;
            }
            catch (Exception)
            {

            }

            GUIStyle logStyle = GUI.skin.label;
            logStyle.fontSize = logFontSize;
            try
            {
                logStyle.font = customFont;
            }
            catch (Exception)
            {

            }
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            // Used to determine height of accumulated log labels.
            GUILayout.BeginVertical();

            IEnumerable<Log> visibleLogs = logs.Where(IsLogVisible);

            foreach (Log log in visibleLogs)
            {
                DrawLog(log, logStyle, badgeStyle);
            }

            GUILayout.EndVertical();
            Rect innerScrollRect = GUILayoutUtility.GetLastRect();
            GUILayout.EndScrollView();
            Rect outerScrollRect = GUILayoutUtility.GetLastRect();

            // If we're scrolled to bottom now, guarantee that it continues to be in next cycle.
            if (Event.current.type == EventType.Repaint && IsScrolledToBottom(innerScrollRect, outerScrollRect))
            {
                ScrollToBottom();
            }
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(clearLabel))
            {
                logs.Clear();
            }

            foreach (LogType logType in lFKeys)
            {
                bool currentState = logTypeFilters[logType];
                string label = logType.ToString();
                logTypeFilters[logType] = GUILayout.Toggle(currentState, label, GUILayout.ExpandWidth(false));
                GUILayout.Space(20);
            }

            isCollapsed = GUILayout.Toggle(isCollapsed, collapseLabel, GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }

        private void DrawSuggestions()
        {
            if (suggestedCommand != null)
            {
                suggestedCommands = suggestedCommand.Command + " ";
                foreach (string hint in suggestedCommand.Hints)
                {
                    suggestedCommands += "[" + hint + "] ";
                }

                GUILayout.Label("Suggested: " + suggestedCommands);
            }
            else
            {
                GUILayout.Label(string.IsNullOrEmpty(suggestedCommands) ? "" : "Suggested: " + suggestedCommands);
            }
        }

        private void DrawInput()
        {
            GUILayout.BeginHorizontal();

            GUI.SetNextControlName("inputBar");
            inputString = GUILayout.TextField(inputString, 2000);

            editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            editor.multiline = false;
            if (GUILayout.Button(submitLabel, GUILayout.ExpandWidth(false)))
            {
                //Utility.SkUtilities.Logz(new string[] { "INPUT" }, new string[] { outputString, SkCommandProcessor.Instance.ExecuteCommand(outputString).ToString()});
                HandleInput(inputString);
                ScrollToBottom();
                inputString = String.Empty;
            }

            GUILayout.EndHorizontal();
        }

        private void HandleInput(string consoleInput)
        {
            if (!string.IsNullOrEmpty(consoleInput))
            {
                if (!consoleInput.Equals("cls"))
                {
                    Utility.SkUtilities.Logz(new string[] { "IN" }, new string[] { consoleInput });
                }
                consoleHistory.Add(inputString);
                SkCommandProcessor.Instance.ExecuteCommand(inputString);
                suggestedCommands = string.Empty;
                suggestedCommand = null;
                inputString = string.Empty;
                matchCommandList.Clear();
            }
        }

        private void DrawWindow(int windowID)
        {
            DrawLogList();
            if (enableInput)
            {
                DrawSuggestions();
                DrawInput();
            }
            DrawToolbar();

            // Allow the window to be dragged by its title bar.
            GUI.DragWindow(titleBarRect);
        }

        private Log? GetLastLog()
        {
            if (logs.Count == 0)
            {
                return null;
            }

            return logs.Last();
        }

        private void UpdateQueuedLogs()
        {
            Log log;
            while (queuedLogs.TryDequeue(out log))
            {
                ProcessLogItem(log);
            }
        }

        private void HandleLogThreaded(string message, string stackTrace, LogType type)
        {
            Log log = new Log
            {
                count = 1,
                message = message,
                stackTrace = stackTrace,
                type = type,
            };
            if (writeToFile)
            {
                logLineNumber++;
                using (StreamWriter writer = new StreamWriter(logSavePath, true))
                {
                    //writer.WriteLine(message + "\n\t\t" + type + ": " + stackTrace.Replace("\n", "\n\t\t"));
                    writer.WriteLine(logLineNumber + "\t:\t" + message);
                }
            }

            // Queue the log into a ConcurrentQueue to be processed later in the Unity main thread,
            // so that we don't get GUI-related errors for logs coming from other threads
            queuedLogs.Enqueue(log);
        }

        private void ProcessLogItem(Log log)
        {
            Log? lastLog = GetLastLog();
            bool isDuplicateOfLastLog = lastLog.HasValue && log.Equals(lastLog.Value);

            if (isDuplicateOfLastLog)
            {
                // Replace previous log with incremented count instead of adding a new one.
                log.count = lastLog.Value.count + 1;
                logs[logs.Count - 1] = log;
            }
            else
            {
                logs.Add(log);
                TrimExcessLogs();
            }
        }

        private bool IsLogVisible(Log log)
        {
            return logTypeFilters[log.type];
        }

        private bool IsScrolledToBottom(Rect innerScrollRect, Rect outerScrollRect)
        {
            float innerScrollHeight = innerScrollRect.height;

            // Take into account extra padding added to the scroll container.
            float outerScrollHeight = outerScrollRect.height - GUI.skin.box.padding.vertical;

            // If contents of scroll view haven't exceeded outer container, treat it as scrolled to bottom.
            if (outerScrollHeight > innerScrollHeight)
            {
                return true;
            }

            // Scrolled to bottom (with error margin for float math)
            return Mathf.Approximately(innerScrollHeight, scrollPosition.y + outerScrollHeight);
        }

        private void ScrollToBottom()
        {
            scrollPosition = new Vector2(0, Int32.MaxValue);
        }

        private void TrimExcessLogs()
        {
            if (!restrictLogCount)
            {
                return;
            }

            int amountToRemove = logs.Count - maxLogCount;

            if (amountToRemove <= 0)
            {
                return;
            }

            logs.RemoveRange(0, amountToRemove);
        }

        public void ClearConsole()
        {
            logs.Clear();
        }

        private void unlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private class History
        {
            public void Add(string item)
            {
                this.history.Add(item);
                this.index = 0;
            }

            public string Fetch(string current, bool next)
            {
                if (this.index == 0)
                {
                    this.current = current;
                }
                if (this.history.Count == 0)
                {
                    return current;
                }
                this.index += ((!next) ? 1 : -1);
                if (this.history.Count + this.index < 0 || this.history.Count + this.index > this.history.Count - 1)
                {
                    this.index = 0;
                    return this.current;
                }
                return this.history[this.history.Count + this.index];
            }

            public List<string> history = new List<string>();
            private int index;
            private string current;
        }
    }

    /// <summary>
    /// A basic container for log details.
    /// </summary>
    internal struct Log
    {
        public int count;
        public string message;
        public string stackTrace;
        public LogType type;

        /// <summary>
        /// The max string length supported by UnityEngine.GUILayout.Label without triggering this error:
        /// "String too long for TextMeshGenerator. Cutting off characters."
        /// </summary>
        private const int maxMessageLength = 16382;

        public bool Equals(Log log)
        {
            return message == log.message && stackTrace == log.stackTrace && type == log.type;
        }

        /// <summary>
        /// Return a truncated message if it exceeds the max message length.
        /// </summary>
        public string GetTruncatedMessage()
        {
            if (string.IsNullOrEmpty(message))
            {
                return message;
            }

            return message.Length <= maxMessageLength ? message : message.Substring(0, maxMessageLength);
        }
    }

    /// <summary>
    /// Alternative to System.Collections.Concurrent.ConcurrentQueue
    /// (It's only available in .NET 4.0 and greater)
    /// </summary>
    /// <remarks>
    /// It's a bit slow (as it uses locks), and only provides a small subset of the interface
    /// Overall, the implementation is intended to be simple & robust
    /// </remarks>
    internal class ConcurrentQueue<T>
    {
        private readonly Queue<T> queue = new Queue<T>();
        private readonly object queueLock = new object();

        public void Enqueue(T item)
        {
            lock (queueLock)
            {
                queue.Enqueue(item);
            }
        }

        public bool TryDequeue(out T result)
        {
            lock (queueLock)
            {
                if (queue.Count == 0)
                {
                    result = default(T);
                    return false;
                }

                result = queue.Dequeue();
                return true;
            }
        }
    }
}
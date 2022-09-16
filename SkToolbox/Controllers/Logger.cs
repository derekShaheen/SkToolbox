using SkToolbox.Controllers;
using UnityEngine;

namespace SkToolbox
{
    public static class Logger
    {
        private static MainConsole mainConsole = null;

        public static MainConsole MainConsole {
            get {
                if (mainConsole == null)
                {
                    mainConsole = GameObject.FindObjectOfType<MainConsole>();
                }
                return mainConsole;
                }
            set => mainConsole = value; }

        public static void Submit(string inputString, bool prefix = true)
        {
            if(MainConsole != null)
            {
                MainConsole.Submit(inputString, prefix);
            }
        }

        public static void Debug(string inputString, bool prefix = true)
        {
            if (prefix)
            {
                inputString = Settings.Console.OutputPrefix + inputString;
            }
            UnityEngine.Debug.Log(inputString);
        }
    }
}

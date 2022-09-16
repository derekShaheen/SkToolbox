using SkToolbox.Controllers;
using UnityEngine;

namespace SkToolbox
{
    public static class Logger
    {
        public static void Submit(string inputString, bool prefix = true)
        {
            if(Loaders.SkBepInExLoader.Console != null)
            {
                Loaders.SkBepInExLoader.Console.Submit(inputString, prefix);
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

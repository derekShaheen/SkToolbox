using HarmonyLib;
using SkToolbox.Utility;
using System;
using System.Reflection;

namespace SkToolbox.Controllers
{
    public static class SkPatcher
    {
        private static Harmony harmony = null;

        public static void InitPatch()
        {
            //InitPatch(typeof(SkPatcher).Assembly);
            InitPatch(Assembly.GetExecutingAssembly());
        }

        public static void InitPatch(Type type)
        {
            InitPatch(type.Assembly);
        }

        public static void InitPatch(Assembly assembly)
        {

            if (harmony == null)
            {
                //SkUtilities.Logz(new string[] { "SkCommandPatcher", "INJECT" }, new string[] { "Attempting injection..." });
                try
                {
                    //Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
                    harmony = Harmony.CreateAndPatchAll(assembly);
                    //SkUtilities.Logz(new string[] { "SkCommandPatcher", "INJECT" }, new string[] { "INJECT => COMPLETE" });
                }
                catch (Exception ex)
                {
                    SkUtilities.Logz(new string[] { "SkCommandPatcher", "PATCH" }, new string[] { "PATCH => FAILED.", ex.Message, ex.StackTrace }, UnityEngine.LogType.Error);
                }

            }
        }
    }
}

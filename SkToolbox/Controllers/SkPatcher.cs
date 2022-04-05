using HarmonyLib;
using SkToolbox.Utility;
using System;

namespace SkToolbox.Controllers
{
    public static class SkPatcher
    {
        private static Harmony harmony = null;
        private static bool initComplete = false;

        public static bool InitComplete { get => initComplete; set => initComplete = value; }

        public static void InitPatch()
        {

            if (!InitComplete)
            {
                //SkUtilities.Logz(new string[] { "SkCommandPatcher", "INJECT" }, new string[] { "Attempting injection..." });
                try
                {
                    //Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
                    harmony = Harmony.CreateAndPatchAll(typeof(SkPatcher).Assembly);
                    //SkUtilities.Logz(new string[] { "SkCommandPatcher", "INJECT" }, new string[] { "INJECT => COMPLETE" });
                }
                catch (Exception ex)
                //catch (Exception)
                {
                    SkUtilities.Logz(new string[] { "SkCommandPatcher", "PATCH" }, new string[] { "PATCH => FAILED.", ex.Message, ex.StackTrace }, UnityEngine.LogType.Error);
                }
                finally
                {
                    InitComplete = true;
                }
            }
        }
    }
}

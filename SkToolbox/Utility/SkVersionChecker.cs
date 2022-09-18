using System;
using System.Linq;
using System.Net;
using UnityEngine;

namespace SkToolbox.Utility
{
    internal static class SkVersionChecker
    {
        public static string VersionURL = "https://raw.githubusercontent.com/derekShaheen/SkToolbox/release/SkToolbox/Utility/SkVersionChecker.cs";
        private static readonly string HitTracker = "https://hits.dwyl.com/derekShaheen/SkToolbox.svg"; // 
        public static string ApplicationSource = "Github";
        public static Version currentVersion;
        public static Version latestVersion = new Version("0.0.0.0");

        public static void CheckVersion()
        {
            try
            {
                currentVersion = new Version(Loaders.SkBepInExLoader.VERSION);
                WebClient wClient = new WebClient();
                wClient.Headers.Add("User-Agent: " + Loaders.SkBepInExLoader.MODNAME + "User");

                wClient.DownloadStringAsync(new Uri(HitTracker)); // Log the hit (only 454 btyes)
                
                WebClient wClient2 = new WebClient();
                wClient2.Headers.Add("User-Agent: " + Loaders.SkBepInExLoader.MODNAME + "User");
                Uri uri = new Uri(VersionURL);
                

                wClient2.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadStringCompletedCallback);

                wClient2.DownloadStringAsync(uri); // Check version

            }
            catch (Exception)
            {
                return;
            }
        }

        public static void DownloadStringCompletedCallback(System.Object sender, DownloadStringCompletedEventArgs e)
        {

            if (!e.Cancelled && e.Error == null)
            {
                ProcessResult(e.Result);
            }
        }

        public static void ProcessResult(string result)
        {
            String[] strSplit = result.Split('\n');
            foreach (string line in strSplit)
            {
                if (line.Contains("currentVersion"))
                {
                    String versionExtract = line.Substring(line.IndexOf('\"') + 1, 7);
                    latestVersion = new Version(versionExtract);

                    if (latestVersion != null && currentVersion != null
                        && latestVersion > currentVersion)
                    {
                        SkUtilities.Logz(new string[] { "VERSION", "CHECK" },
                            //new string[] { "New version (" + latestVersion + ") of " + ApplicationName + "(" + currentVersion + ") available on " + ApplicationSource + "." });
                            new string[] { "New version of " + Loaders.SkBepInExLoader.MODNAME + " (" + latestVersion + ") available on " + ApplicationSource + ". Current version: " + currentVersion });
                    }

                    break;
                }
                else
                {
                    continue;
                }
            }

        }
    }
}

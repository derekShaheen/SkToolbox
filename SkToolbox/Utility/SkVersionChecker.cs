using System;
using System.Linq;
using System.Net;
using UnityEngine;

namespace SkToolbox.Utility
{
    internal static class SkVersionChecker
    {
        public static readonly string VersionURL = "https://raw.githubusercontent.com/derekShaheen/SkToolbox/release/SkToolbox/Utility/SkVersionChecker.cs";
        public static string ApplicationName = "SkToolbox";
        public static string ApplicationSource = "Github";
        public static Version currentVersion = new Version("1.0.0.0");
        public static Version latestVersion = new Version("0.0.0.0");
        public static void CheckVersion()
        {
            try
            {
                WebClient wClient = new WebClient();
                wClient.Headers.Add("User-Agent: SkToolboxUser" + UnityEngine.Random.Range(0, 999999).ToString());
                Uri uri = new Uri(VersionURL);

                wClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadStringCompletedCallback);

                wClient.DownloadStringAsync(uri);
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
            String versionExtract = String.Empty;
            String[] strSplit = result.Split('\n');
            foreach (string line in strSplit)
            {
                if (line.Contains("currentVersion"))
                {
                    versionExtract = line.Substring(line.IndexOf('\"') + 1, 7);
                    latestVersion = new Version(versionExtract);
                    break;
                }
                else
                {
                    continue;
                }
            }
            if (latestVersion != null && currentVersion != null
                && latestVersion > currentVersion)
            {
                SkUtilities.Logz(new string[] { "VERSION", "CHECK" },
                    //new string[] { "New version (" + latestVersion + ") of " + ApplicationName + "(" + currentVersion + ") available on " + ApplicationSource + "." });
                    new string[] { "New version of " + ApplicationName + "(" + latestVersion + ") available on " + ApplicationSource + ". Current version: " + currentVersion });
            }
        }
    }
}

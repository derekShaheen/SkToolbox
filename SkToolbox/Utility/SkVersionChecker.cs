using System;
using System.Net;

namespace SkToolbox.Utility
{
    internal static class SkVersionChecker
    {
        private static readonly string VersionURL = "https://pastebin.com/raw/ubRAdqxz";
        internal static Version currentVersion = new Version("1.0.0.0");
        internal static Version latestVersion = new Version("0.0.0.0");
        public static bool VersionCurrent()
        {
            try
            {
                if (latestVersion == new Version("0.0.0.0"))
                {
                    WebClient wClient = new WebClient();
                    wClient.Headers.Add("User-Agent: SkToolboxUser" + UnityEngine.Random.Range(0, 999999).ToString());

                    string latestVersionStr = wClient.DownloadString(VersionURL);
                    latestVersion = new Version(latestVersionStr);
                }
                if (latestVersion > currentVersion)
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}

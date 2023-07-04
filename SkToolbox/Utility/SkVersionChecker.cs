using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace SkToolbox.Utility
{
    /// <summary>
    /// Provides methods to check for new versions of registered modules.
    /// </summary>
    public static class SkVersionChecker
    {

        /// <summary>
        /// Struct representing a version check request.
        /// </summary>
        public struct CheckRequest
        {
            /// <summary>
            /// The name of the module.
            /// </summary>
            public string ModuleName { get; set; }

            /// <summary>
            /// The current version of the module.
            /// </summary>
            public Version CurrentVersion { get; set; }

            /// <summary>
            /// The URL of the version check endpoint.
            /// </summary>
            public string EndpointUrl { get; set; }

            /// <summary>
            /// The latest version of the module, as returned by the version check endpoint.
            /// </summary>
            public Version LatestVersion { get; set; }

            /// <summary>
            /// Indicates whether there is a newer version available for the module.
            /// </summary>
            public bool HasNewerVersion { get; set; }

            /// <summary>
            /// Indicates whether there is a newer version available for the module.
            /// </summary>
            public bool HasProcessed { get; set; }

            /// <summary>
            /// Indicates whether to announce the result of the version check automatically.
            /// </summary>
            public bool Announce { get; set; }

            public override string ToString()
            {
                return $"CheckRequest: {ModuleName} {CurrentVersion} {EndpointUrl} {LatestVersion} {HasNewerVersion} {HasProcessed} {Announce}";

            }
        }

        private static readonly List<CheckRequest> _checkRequests = new List<CheckRequest>();
        private static readonly Queue<CheckRequest> _checkQueue = new Queue<CheckRequest>();
        private static bool _isChecking = false;
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Registers a new version check request.
        /// </summary>
        /// <param name="moduleName">The name of the module.</param>
        /// <param name="currentVersion">The current version of the module.</param>
        /// <param name="endpointUrl">The URL of the version check endpoint.</param>
        /// <param name="announce">Indicates whether to announce the result of the version check.</param>
        public static void RegisterCheckRequest(string moduleName, Version currentVersion, string endpointUrl, bool announce = true)
        {
            // Check if the module is already in the check list
            var existingRequest = _checkRequests.Find(req => req.ModuleName.Equals(moduleName));
            if (existingRequest.HasProcessed)
            {
                existingRequest.CurrentVersion = currentVersion;
                _checkQueue.Enqueue(existingRequest);
            }
            else
            {
                _checkQueue.Enqueue(new CheckRequest
                {
                    ModuleName = moduleName,
                    CurrentVersion = currentVersion,
                    EndpointUrl = endpointUrl,
                    HasNewerVersion = false,
                    Announce = announce,
                    HasProcessed = false
                });
            }

            if (!_isChecking)
            {
                CheckVersion();
            }
        }

        private static async Task CheckVersionAsync()
        {
            while (_checkQueue.Count > 0)
            {
                CheckRequest request = _checkQueue.Dequeue();
                try
                {
                    _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SkToolbox User");
                    var response = await _httpClient.GetStringAsync(request.EndpointUrl);

                    _checkRequests.Add(ProcessResult(response, request));
                }
                catch (Exception ex)
                {
                    // Handle exception
                }
            }
            _isChecking = false;
        }

        private static void CheckVersion()
        {
            _isChecking = true;

            // Start checking versions asynchronously.
            Task.Run(async () => await CheckVersionAsync());
        }

        private static CheckRequest ProcessResult(string result, CheckRequest request)
        {
            Version latestVersion = null;
            Version.TryParse(result, out latestVersion);

            if(latestVersion == null)
            {
                String[] strSplit = result.Split('\n');
                foreach (string line in strSplit)
                {
                    if (line.Contains("VERSION") && !line.Contains("BepInPlugin"))
                    {
                        if (line.Length > 9)
                        {
                            String versionExtract = line.Substring(line.IndexOf('"') + 1, 7);
                            Version.TryParse(versionExtract, out latestVersion);
                        } else
                        {
                            Version.TryParse(line, out latestVersion);
                        }
                        break;
                    }
                }
            }   

            request.LatestVersion = latestVersion;
            request.HasNewerVersion = latestVersion > request.CurrentVersion;
            request.HasProcessed = true;
            Console.WriteLine("Processed:" + request.ToString());
            if (request.HasNewerVersion)
            {
                Console.WriteLine("New version of " + request.ModuleName + " available! Current version: " + request.CurrentVersion.ToString() + ", Latest version: " + request.LatestVersion.ToString());
                if (request.Announce)
                {
                    Logger.Submit("New version of " + request.ModuleName + " available! Current version: " + request.CurrentVersion.ToString() + ", Latest version: " + request.LatestVersion.ToString());
                }
            }

            return request;
        }

        /// <summary>
        /// Searches the list of version check requests for a particular module.
        /// </summary>
        /// <param name="moduleName">The name of the module to search for.</param>
        /// <returns>A CheckRequest object containing the specified module name, or null if no such request exists.</returns>
        public static CheckRequest GetCheckRequest(string moduleName)
        {
            var existingRequest = _checkRequests.Find(req => req.ModuleName.Equals(moduleName));
            return existingRequest;
        }

        /// <summary>
        /// Returns a list of all version check requests made by the SkVersionChecker class.
        /// </summary>
        /// <returns>A List of CheckRequest objects containing all version check requests.</returns>
        public static List<CheckRequest> GetAllCheckRequests()
        {
            return _checkRequests;
        }
    }
}
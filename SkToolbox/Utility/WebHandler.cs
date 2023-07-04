using SkToolbox.Controllers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace SkToolbox.Utility
{
    internal static class WebHandler
    {
        /// <summary>
        /// Download a texture via a coroutine
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        /// Example Call
        /*
         *  StartCoroutine(WebHandler.GetTextureRequest(bannerUrl, (response) => {
                bannerTexture = response;
            }));
         */
        internal static IEnumerator GetTextureRequest(string url, System.Action<Texture2D> callback)
        {
            if(!SettingsController.Get<bool>("NetworkFunctions"))
            {
                yield return null;
            }

            using (var www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    //Debug.Log(www.error);
                }
                else
                {
                    if (www.isDone)
                    {
                        var texture = DownloadHandlerTexture.GetContent(www);
                        callback(texture);
                    }
                }
            }
        }

        /// <summary>
        /// Get a generic request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        /*
         * StartCoroutine(GetRequest("url", (UnityWebRequest request) =>
            {
                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.Log($"{request.error}: {request.downloadHandler.text}");
                } else
                {
                    Debug.Log(request.downloadHandler.text);
                }
            }));
         */
        internal static IEnumerator GetRequest(string url, Action<UnityWebRequest> callback)
        {
            if (!SettingsController.Get<bool>("NetworkFunctions"))
            {
                yield return null;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("User-Agent", "SkToolbox User");
                // Send the request and wait for a response
                yield return request.SendWebRequest();
                callback(request);
            }
        }
    }
}

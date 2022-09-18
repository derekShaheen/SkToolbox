using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SkToolbox.Utility
{
    internal static class WebHandler
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        /// Example Call
        /*
         *  StartCoroutine(GetTextureRequest(url, (response) => {
                targetSprite = response;
                spriteRenderer.sprite = targetSprite;
            })); 
         */
        internal static IEnumerator GetTextureRequest(string url, System.Action<Texture2D> callback)
        {
            using (var www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
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
    }
}

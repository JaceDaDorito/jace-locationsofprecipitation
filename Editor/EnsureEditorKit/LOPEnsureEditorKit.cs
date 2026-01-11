using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

namespace LOP.Editor
{
    internal static class InstallFromThunderstore
    {
        private static UnityWebRequest _webRequest;
        private static UnityWebRequestAsyncOperation _asyncOperation;

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            Debug.Log("R2EK Thunderstore version installed, installing latest github release...");


            string url = $"https://api.github.com/repos/risk-of-thunder/RoR2EditorKit/releases/latest";
            _webRequest = UnityWebRequest.Get(url);
            _asyncOperation = _webRequest.SendWebRequest();
            _asyncOperation.completed += AsyncOp_completed;
        }

        private static void AsyncOp_completed(AsyncOperation _)
        {
            var release = JsonUtility.FromJson<GitHubRelease>(_webRequest.downloadHandler.text);
            var request = Client.Add("https://github.com/risk-of-thunder/RoR2EditorKit.git" + "#" + release.tag_name);
            while (!request.IsCompleted)
            {
                Debug.Log("Waiting for package installation.");
            }

            _webRequest.Dispose();
            _webRequest = null;
            _asyncOperation = null;

            Debug.Log("EditorKit Ensured...");
        }

        [Serializable]
        private class GitHubRelease
        {
            public string tag_name;
        }
    }
}

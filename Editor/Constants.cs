using System.IO;
using UnityEditor;
using UnityEngine;

namespace LOP
{
    public static class Constants
    {
        public static class AssetGUIDS
        {
            public const string geyserPrefabGUID = "be993fbf98b5dbc4198694798de33d7c";

            /// <summary>
            /// Loads an asset of type <typeparamref name="T"/> by using the <paramref name="guid"/> provided
            /// </summary>
            /// <typeparam name="T">The type of asset to load</typeparam>
            /// <param name="guid">The guid to use</param>
            /// <returns>The loaded Asset</returns>
            public static T QuickLoad<T>(string guid) where T : Object
            {
                return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            }
            /// <summary>
            /// Transforms <paramref name="guid"/> into its asset path.
            /// </summary>
            /// <param name="guid">The asset's guid</param>
            /// <returns>The Asset's path</returns>
            public static string GetPath(string guid)
            {
                return AssetDatabase.GUIDToAssetPath(guid);
            }
        }
    }
}

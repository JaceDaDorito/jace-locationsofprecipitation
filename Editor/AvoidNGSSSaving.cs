using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LOP.Editor
{
    public class AvoidNGSSSaving : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            foreach (var gameObject in scene.GetRootGameObjects())
            {
                NGSS_Directional[] directionals = gameObject.GetComponentsInChildren<NGSS_Directional>();
                foreach (NGSS_Directional dir in directionals)
                {
                    if (dir.NGSS_NOISE_TEXTURE)
                        dir.NGSS_NOISE_TEXTURE.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.HideAndDontSave | HideFlags.DontSaveInBuild;
                }
            }
            return paths;
        }
    }
}
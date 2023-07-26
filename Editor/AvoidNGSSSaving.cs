using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using RoR2;

namespace LOP.Editor
{
    public class AvoidNGSSSaving : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            var addressableTexture = LegacyResourcesAPI.Load<Texture2D>("BlueNoise_R8_8");

            Scene scene = EditorSceneManager.GetActiveScene();
            foreach (var gameObject in scene.GetRootGameObjects())
            {
                NGSS_Directional[] directionals = gameObject.GetComponentsInChildren<NGSS_Directional>();
                foreach (NGSS_Directional dir in directionals)
                {
                    if (dir.NGSS_NOISE_TEXTURE && dir.NGSS_NOISE_TEXTURE == addressableTexture)
                        dir.NGSS_NOISE_TEXTURE = null;
                }
            }
            return paths;
        }
    }
}
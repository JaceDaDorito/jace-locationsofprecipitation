using RoR2.ContentManagement;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LOP
{
    /// <summary>
    /// Instantiates the RoR2 main camera, which allows preview of post processing effects
    /// <para>Do not leave this on finalized builds, as it causes errors</para>
    /// </summary>
    [ExecuteAlways]
    public class CameraInstantiator : MonoBehaviour
    {
        //"RoR2/Base/Core/Camera/Main Camera.prefab"
        public const string CAMERA_ADDRESS = "55d9d47bebc68734d9e0afdd4d1337ca";

        [field: NonSerialized]
        public GameObject CameraInstance { get; private set; }

        private AsyncOperationHandle<GameObject> _asyncOperationHandle;
        
        private void OnEnable() => Refresh();
        private void OnDisable()
        {
            if(CameraInstance)
                LOPUtil.DestroyImmediateSafe(CameraInstance, true);
        
            //OnEnable calls refresh so the handle has a value there.
            if(_asyncOperationHandle.IsValid())
            {
                Addressables.Release(_asyncOperationHandle);
            }
        }

        /// <summary>
        /// Instantiates the camera or destroys the attached game object if the component is instantiated at runtime and not in the editor.
        /// </summary>
        public void Refresh()
        {
            if (Application.isPlaying && !Application.isEditor)
            {
                LOPLog.Fatal($"Lingering camera injector in {gameObject}, Ensure that these scripts are NOT present on finalized builds!!!");
                Destroy(gameObject);
                return;
            }

            if (CameraInstance)
            {
                LOPUtil.DestroyImmediateSafe(CameraInstance, true);
            }

            _asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(CAMERA_ADDRESS);
            var go = _asyncOperationHandle.WaitForCompletion();
            CameraInstance = Instantiate(go, transform);
            CameraInstance.name = $"[EDITOR ONLY] {CameraInstance.name}";
            CameraInstance.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.NotEditable;
            foreach (Transform t in CameraInstance.GetComponentsInChildren<Transform>())
            {
                t.gameObject.hideFlags = CameraInstance.hideFlags | HideFlags.HideInHierarchy;
            }
            CameraInstance.hideFlags &= ~HideFlags.HideInHierarchy;
        }
    }
}
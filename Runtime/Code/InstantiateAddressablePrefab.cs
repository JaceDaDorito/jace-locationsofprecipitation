using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LOP
{
    /// <summary>
    /// Instantiates the prefab specified in <see cref="address"/>
    /// </summary>
    [ExecuteAlways]
    public class InstantiateAddressablePrefab : MonoBehaviour
    {
        [Tooltip("The address to use to load the prefab")]
        [SerializeField] private AssetReferenceGameObject prefabAddress;
        [Obsolete]
        [SerializeField] private string address;

        [Tooltip("When the prefab is instantiated, and this is true, the prefab's position and rotation will be set to 0")]
        [SerializeField] private bool setPositionAndRotationToZero = true;
        [Tooltip("setPositionAndRotationToZero would work relative to it's parent")]
        [SerializeField] private bool useLocalPositionAndRotation = true;
        [Tooltip("Wether the Refresh method will be called in the editor")]
        [SerializeField] private bool refreshInEditor = true;
        [SerializeField, HideInInspector] private bool hasNetworkIdentity;

        /// <summary>
        /// The instantiated prefab
        /// </summary>
        [field: NonSerialized]
        public GameObject Instance { get; private set; }

        private AsyncOperationHandle<GameObject> _asyncOperationHandle;

        private void OnEnable() => Refresh();
        private void OnDisable()
        {
            DestroyInstanceAndRelease();
        }

        private void DestroyInstanceAndRelease()
        {
            if (Instance)
                LOPUtil.DestroyImmediateSafe(Instance, true);

            if (_asyncOperationHandle.IsValid())
            {
                Addressables.Release(_asyncOperationHandle);
            }
        }
        /// <summary>
        /// Destroys the instantiated object and re-instantiates using the prefab that's loaded via <see cref="address"/>
        /// </summary>
        public void Refresh()
        {
            if (Application.isEditor && !refreshInEditor)
                return;

            DestroyInstanceAndRelease();

            if (prefabAddress.RuntimeKeyIsValid() || string.IsNullOrWhiteSpace(address))
            {
                LOPLog.Warning($"Invalid address in {this}, address is null, empty, or white space");
                return;
            }

            _asyncOperationHandle = LoadPrefab();
            GameObject prefab = _asyncOperationHandle.WaitForCompletion();

            Instance = Instantiate(prefab);
            if (!Application.isEditor && NetworkServer.active && Instance.TryGetComponent<NetworkIdentity>(out var networkIdentity))
            {
                NetworkServer.Spawn(Instance);
            }

            if(Application.isEditor)
            {
                Instance.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.NotEditable;
                foreach (Transform t in Instance.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.hideFlags = Instance.hideFlags;
                }
            }

            if (setPositionAndRotationToZero)
            {
                Transform t = Instance.transform;
                if (useLocalPositionAndRotation)
                {
                    t.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                }
                else
                {
                    t.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                }
            }
        }

        private AsyncOperationHandle<GameObject> LoadPrefab()
        {
            if(prefabAddress.RuntimeKeyIsValid())
            {
                return Addressables.LoadAssetAsync<GameObject>(prefabAddress.RuntimeKey);
            }
            return Addressables.LoadAssetAsync<GameObject>(address);
        }
    }
}

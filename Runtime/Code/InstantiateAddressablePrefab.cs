using System;
using UnityEngine;
using UnityEngine.Networking;

namespace LOP
{
    /// <summary>
    /// Instantiates the prefab specified in <see cref="address"/>
    /// </summary>
    [ExecuteAlways]
    public class InstantiateAddressablePrefab : MonoBehaviour
    {
        [Tooltip("The address to use to load the prefab")]
        [SerializeField] private string address;
        [Tooltip("When the prefab is instantiated, and this is true, the prefab's position and rotation will be set to 0")]
        [SerializeField] private bool setPositionAndRotationToZero = true;
        [Tooltip("When the prefab is instantiated, and this is true, the prefab's position will be set to 0")]
        [SerializeField] private bool setPositionToZero = true;
        [Tooltip("When the prefab is instantiated, and this is true, the prefab's rotation will be set to 0")]
        [SerializeField] private bool setRotationToZero = true;
        [Tooltip("setPosition/RotationToZero would work relative to it's parent")]
        [SerializeField] private bool useLocalPositionAndRotation = true;
        [Tooltip("When the prefab is instantiated, and this is true, the prefab's local scale will be set to value of newLocalScale")]
        [SerializeField] private bool overrideLocalScale;
        [SerializeField] private Vector3 newLocalScale = Vector3.one;
        [Tooltip("Wether the Refresh method will be called in the editor")]
        [SerializeField] private bool refreshInEditor = true;
        [SerializeField, HideInInspector] private bool hasNetworkIdentity;

        /// <summary>
        /// The instantiated prefab
        /// </summary>
        public GameObject Instance => instance;
        [NonSerialized]
        private GameObject instance;

        private void OnEnable() => Refresh();
        private void OnDisable()
        {
            if (instance)
                LOPUtil.DestroyImmediateSafe(instance, true);
        }
        /// <summary>
        /// Destroys the instantiated object and re-instantiates using the prefab that's loaded via <see cref="address"/>
        /// </summary>
        public void Refresh()
        {
            if (instance)
            {
                LOPUtil.DestroyImmediateSafe(instance, true);
            }

            if (Application.isEditor && !refreshInEditor)
                return;

            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrEmpty(address))
            {
                LOPLog.Warning($"Invalid address in {this}, address is null, empty, or white space");
                return;
            }

            GameObject prefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(address).WaitForCompletion();
            hasNetworkIdentity = prefab.GetComponent<NetworkIdentity>();

            if (hasNetworkIdentity && !Application.isEditor)
            {
                if (NetworkServer.active)
                {
                    instance = Instantiate(prefab, transform);
                    NetworkServer.Spawn(instance);
                }
            }
            else
            {
                instance = Instantiate(prefab, transform);
            }

            if (instance) 
            {
#if UNITY_EDITOR
                instance.AddComponent<InstantiateProtection>();
#endif

                instance.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.NotEditable;
                foreach (Transform t in instance.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.hideFlags = instance.hideFlags;
                }

                if (setPositionAndRotationToZero)
                {
                    Transform t = instance.transform;
                    if (setPositionToZero)
                    {
                        if (useLocalPositionAndRotation)
                        {
                            t.localPosition = Vector3.zero;
                        }
                        else
                        {
                            t.position = Vector3.zero;
                        }
                    }
                    if (setRotationToZero)
                    {
                        if (useLocalPositionAndRotation)
                        {
                            t.localRotation = Quaternion.identity;
                        }
                        else
                        {
                            t.rotation = Quaternion.identity;
                        }
                    }
                }

                if (overrideLocalScale)
                {
                    t.localScale = newLocalScale;
                }
            }
        }
    }
}

using RoR2;
using RoR2.Navigation;
using System;
using System.Net;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LOP
{
    /// <summary>
    /// Instantiates the geyser specified in <see cref="geyserType"/>
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(JumpVolume))]
    
    public class InstantiateGeyserPrefab : MonoBehaviour
    {
#if R2API_ADDRESSABLES_INSTALLED
        [R2API.AddressReferencedAssets.AddressableComponentRequirement(typeof(JumpVolume), searchInChildren = true)]
#endif
        [Tooltip("The prefab used for the geyser")]
        [SerializeField] private AssetReferenceGameObject geyserAddress;

        #region Obsolete
        [Obsolete("Utilize geyserAddress directly instead")]
        public enum GeyserType
        {
            Default = 0,
            Ambry = 1,
            Moon = 2,
            Aphelian = 3,
            Siphoned = 4,
            Void = 5,
            Fan = 6,
            Shroom = 7,
            Meridian = 8,
            Verdant = 9,
            Helminth = 10
        }
        [Obsolete("Utilize geyserAddress directly instead")]
        [SerializeField] public GeyserType geyserType;
        #endregion

        [Tooltip("When the prefab is instantiated, and this is true, the prefab's position and rotation will be set to 0")]
        [SerializeField] private bool setPositionAndRotationToZero = true;
        [Tooltip("setPositionAndRotationToZero would work relative to it's parent")]
        [SerializeField] private bool useLocalPositionAndRotation = true;
        [Tooltip("Wether the Refresh method will be called in the editor")]
        [SerializeField] private bool refreshInEditor = true;
        [Tooltip("Wether the sound string remains the same as the original instance")]
        [SerializeField] private bool conserveSoundString;

        //Fan specific
        [HideInInspector] public bool gateToggleOnPurchase;
        [HideInInspector] public string gateToEnableWhenPurchased;
        [HideInInspector] public string gateToDisableWhenPurchased;

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
            DestroyAndReleaseInstance();
        }

        private void DestroyAndReleaseInstance()
        {
            if(Instance)
            {
                LOPUtil.DestroyImmediateSafe(Instance);
            }

            if(_asyncOperationHandle.IsValid())
            {
                Addressables.Release(_asyncOperationHandle);
            }
        }

        /// <summary>
        /// Destroys the instantiated object and re-instantiates using the prefab that's loaded via <see cref="geyserType"/>
        /// </summary>
        public void Refresh()
        {
            if (Application.isEditor && !refreshInEditor)
                return;

            DestroyAndReleaseInstance();

            _asyncOperationHandle = LoadGeyserPrefab();
            Instance = Instantiate(_asyncOperationHandle.WaitForCompletion(), transform);

            JumpVolume jVolOG = gameObject.GetComponent<JumpVolume>();
            JumpVolume jVolIn = Instance.GetComponentInChildren<JumpVolume>(true);

            if (jVolOG.targetElevationTransform)
                jVolIn.targetElevationTransform = jVolOG.targetElevationTransform;
            else
                LOPLog.Warning($"Target elevation transform on {this} is invalid.");

            jVolIn.jumpVelocity = jVolOG.jumpVelocity;

            jVolIn.time = jVolOG.time;
            if (!conserveSoundString)
            {
                if(string.IsNullOrEmpty(jVolOG.jumpSoundString))
                    LOPLog.Warning($"Jump sound string is empty on {this}. This will result in silence for your geyser.");
                jVolIn.jumpSoundString = jVolOG.jumpSoundString;
            }
                
            jVolIn.onJump = jVolOG.onJump;

            if(geyserType == GeyserType.Fan && gateToggleOnPurchase)
            {
                GateStateSetter gateStateSetter = jVolIn.gameObject.AddComponent<GateStateSetter>();
                gateStateSetter.gateToEnableWhenEnabled = gateToEnableWhenPurchased;
                gateStateSetter.gateToDisableWhenEnabled = gateToDisableWhenPurchased;
            }

            jVolOG.enabled = false;

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

        private AsyncOperationHandle<GameObject> LoadGeyserPrefab()
        {
            if(geyserAddress.RuntimeKeyIsValid())
            {
                return Addressables.LoadAssetAsync<GameObject>(geyserAddress.RuntimeKey);
            }

            string addressToUse = string.Empty;
            switch (geyserType)
            {
                case GeyserType.Default:
                    //"RoR2/Base/Common/Props/Geyser.prefab";
                    addressToUse = "9c0a2a20613c6394697114fdadf3ba03";
                    break;
                case GeyserType.Ambry:
                    //"RoR2/Base/artifactworld/AWGeyser.prefab";
                    addressToUse = "f2b98784391a3804885f587c7a639b58";
                    break;
                case GeyserType.Moon:
                    //"RoR2/Base/moon/MoonGeyser.prefab";
                    addressToUse = "afbf86191e743f54c8b0e12528f62f89";
                    break;
                case GeyserType.Aphelian:
                    //"RoR2/DLC1/ancientloft/AL_Geyser.prefab";
                    addressToUse = "55c0ca4592e95d94aa0394b31f089a52";
                    break;
                case GeyserType.Siphoned:
                    //"RoR2/DLC1/snowyforest/SFGeyser.prefab";
                    addressToUse = "d08d1f2687a38ad43bb9bc74a04702f5";
                    break;
                case GeyserType.Void:
                    //"RoR2/DLC1/voidstage/mdlVoidGravityGeyser.prefab";
                    addressToUse = "1d2f4ad7033395045b93f0f6e1cf8912";
                    break;
                case GeyserType.Fan:
                    //"RoR2/Base/frozenwall/FW_HumanFan.prefab";
                    addressToUse = "d6bca18aaaadae74eab053937b6cedb5";
                    break;
                case GeyserType.Shroom:
                    //"RoR2/Base/rootjungle/RJ_BounceShroom.prefab";
                    addressToUse = "5901a16604ce9234a82a7be66b65a3cb";
                    break;
                case GeyserType.Meridian:
                    //"RoR2/DLC2/meridian/PMLaunchPad.prefab";
                    addressToUse = "9a2a37039a3df7e4db9ae187d9f2243a";
                    break;
                case GeyserType.Verdant:
                    //"RoR2/DLC2/lakes/Assets/TLJumpPad.prefab";
                    addressToUse = "5df549b3b55406045b3de3d5a725f8bb";
                    break;
                case GeyserType.Helminth:
                    //"RoR2/DLC2/helminthroost/Assets/HRLaunchPad.prefab";
                    addressToUse = "8119d60796de2624e9510e6884c3f9e3";
                    break;
            }

            return Addressables.LoadAssetAsync<GameObject>(addressToUse);
        }
    }
}

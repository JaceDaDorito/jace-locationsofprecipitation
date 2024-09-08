using System;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Navigation;

namespace LOP
{
    /// <summary>
    /// Instantiates the geyser specified in <see cref="geyserType"/>
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(JumpVolume))]
    
    public class InstantiateGeyserPrefab : MonoBehaviour
    {
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
            Meridian = 8/*,
            Verdant = 9,
            Helminth = 10*/
        }

        [Tooltip("The prefab used for the geyser")]
        [SerializeField] public GeyserType geyserType;
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

        



        private string address;

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
                LOPUtil.DestroyImmediateSafe(instance);
        }

        /// <summary>
        /// Destroys the instantiated object and re-instantiates using the prefab that's loaded via <see cref="geyserType"/>
        /// </summary>
        public void Refresh()
        {
            if (Application.isEditor && !refreshInEditor)
                return;

            if (instance)
            {
                LOPUtil.DestroyImmediateSafe(instance);
            }

            switch((int)geyserType){
                case 0:
                    address= "RoR2/Base/Common/Props/Geyser.prefab";
                    break;
                case 1:
                    address = "RoR2/Base/artifactworld/AWGeyser.prefab";
                    break;
                case 2:
                    address = "RoR2/Base/moon/MoonGeyser.prefab";
                    break;
                case 3:
                    address = "RoR2/DLC1/ancientloft/AL_Geyser.prefab";
                    break;
                case 4:
                    address = "RoR2/DLC1/snowyforest/SFGeyser.prefab";
                    break;
                case 5:
                    address = "RoR2/DLC1/voidstage/mdlVoidGravityGeyser.prefab";
                    break;
                case 6:
                    address = "RoR2/Base/frozenwall/FW_HumanFan.prefab";
                    break;
                case 7:
                    address = "RoR2/Base/rootjungle/RJ_BounceShroom.prefab";
                    break;
                case 8:
                    address = "RoR2/DLC2/meridian/PMLaunchPad.prefab";
                    break;
                case 9:
                    address = "RoR2/DLC2/lakes/Assets/TLJumpPad.prefab";
                    break;
                case 10:
                    address = "RoR2/DLC2/helminthroost/Assets/HRLaunchPad.prefab";
                    break;
                default:
                    LOPLog.Error($"This isn't supposed to print in {this}. Geyser Type is invalid.");
                    return;
            }

            GameObject prefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(address).WaitForCompletion();
            instance = Instantiate(prefab, transform);

            JumpVolume jVolOG = gameObject.GetComponent<JumpVolume>();
            JumpVolume jVolIn = instance.GetComponentInChildren<JumpVolume>(true);

            if (jVolOG.targetElevationTransform)
                jVolIn.targetElevationTransform = jVolOG.targetElevationTransform;
            else
                LOPLog.Warning($"Target elevation transform on {this} is invalid.");

            jVolIn.jumpVelocity = jVolOG.jumpVelocity;

            jVolIn.time = jVolOG.time;
            if (!conserveSoundString)
            {
                if(String.IsNullOrEmpty(jVolOG.jumpSoundString))
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

            instance.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.NotEditable;
            foreach (Transform t in instance.GetComponentsInChildren<Transform>())
            {
                t.gameObject.hideFlags = instance.hideFlags;
            }
            if (setPositionAndRotationToZero)
            {
                Transform t = instance.transform;
                if (useLocalPositionAndRotation)
                {
                    t.localPosition = Vector3.zero;
                    t.localRotation = Quaternion.identity;
                }
                else
                {
                    t.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                }
            }

        }
    }
}

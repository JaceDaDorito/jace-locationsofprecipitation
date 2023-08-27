﻿using System;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace LOP
{
    [RequireComponent(typeof(JumpVolume))]
    [ExecuteAlways]
    public class InstantiateGeyserPrefab : MonoBehaviour
    {
        private enum GeyserType
        {
            Default = 0,
            Ambry = 1,
            Moon = 2,
            Aphelian = 3,
            Siphoned = 4,
            Void = 5
        }

        [Tooltip("The prefab used for the geyser")]
        [SerializeField] private GeyserType geyserType;
        [Tooltip("When the prefab is instantiated, and this is true, the prefab's position and rotation will be set to 0")]
        [SerializeField] private bool setPositionAndRotationToZero;
        [Tooltip("setPositionAndRotationToZero would work relative to it's parent")]
        [SerializeField] private bool useLocalPositionAndRotation;
        [Tooltip("Wether the Refresh method will be called in the editor")]
        [SerializeField] private bool refreshInEditor;
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
                Destroy(instance);
        }

        public void Refresh()
        {
            if (Application.isEditor && !refreshInEditor)
                return;

            if (instance)
            {
                Destroy(instance);
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
                    address = "RoR2/DLC1/ancientloft/AncientLoft_Geyser.prefab";
                    break;
                case 4:
                    address = "RoR2/DLC1/snowyforest/SFGeyser.prefab";
                    break;
                case 5:
                    address = "RoR2/DLC1/voidstage/mdlVoidGravityGeyser.prefab";
                    break;
                default:
                    LOPLog.Error($"This isn't supposed to print in {this}. Geyser Type is invalid.");
                    return;
            }

            GameObject prefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(address).WaitForCompletion();
            instance = Instantiate(prefab, transform);

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

            JumpVolume jVolOg = gameObject.GetComponent<JumpVolume>();
            JumpVolume jVolInstance = instance.GetComponentInChildren<JumpVolume>();
            Transform jVolInstanceTr = jVolInstance.transform;

            Instantiate(jVolOg, jVolInstanceTr);
            jVolOg.enabled = false;
        }
    }
}
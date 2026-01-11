using System;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

namespace LOP
{
    /// <summary>
    /// Instantiates a Logbook Pickup given an Unlockable.
    /// </summary>
    [RequireComponent(typeof(VelocityRandomOnStart))]
    public class InstantiateLogbookPrefab : MonoBehaviour
    {
        [Tooltip("The unlock that triggers when the prefab is picked up.")]
        [SerializeField] private UnlockableDef unlockableDef;
        [Tooltip("The token for the text that appears when the prefab is picked up.")]
        [SerializeField] private string displayNameToken;
        [Tooltip("Turns on gravity for the pickup.")]
        [SerializeField] private bool enableGravity;

        private static Mesh cubeMesh;

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

        public void OnDrawGizmos()
        {
            cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            Transform tr = gameObject.transform;
            Vector3 size = new Vector3(1.16f, 0.43f, 1.39f);

            Color color = new Color(0.7255f, 0.4314f, 1f);
            Gizmos.color = color;
            Gizmos.DrawWireMesh(cubeMesh, tr.position, tr.rotation, size);
            GUI.color = color;
        }

        /// <summary>
        /// Destroys the instantiated object and re-instantiates the logbook prefab.>
        /// </summary>
        public void Refresh()
        {
            DestroyAndReleaseInstance();

            if (!unlockableDef)
            {
                LOPLog.Warning($"No unlockableDef in {this}. Cancelling instantiation.");
                return;
            }

            if(string.IsNullOrEmpty(displayNameToken)|| string.IsNullOrWhiteSpace(displayNameToken))
            {
                LOPLog.Warning($"Invalid displayNameToken in {this}, displayNameToken is null, empty, or white space. Cancelling instantiation.");
                return;
            }

            //"RoR2/Base/Common/LogPickup.prefab"
            _asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>("0c5bc9b656c4df344b7da9b8b11ff92b");
            GameObject prefab = _asyncOperationHandle.WaitForCompletion();

            if (NetworkServer.active)
            {
                Instance = Instantiate(prefab, transform);
                NetworkServer.Spawn(Instance);
                Transform t = Instance.transform;
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;

                VelocityRandomOnStart velOG = gameObject.GetComponent<VelocityRandomOnStart>();
                VelocityRandomOnStart velIn = Instance.GetComponent<VelocityRandomOnStart>();
                velIn.minSpeed = velOG.minSpeed;
                velIn.maxSpeed = velOG.maxSpeed;
                velIn.baseDirection = velOG.baseDirection;
                velIn.localDirection = velOG.localDirection;
                velIn.directionMode = velOG.directionMode;
                velIn.coneAngle = velOG.coneAngle;
                velIn.minAngularSpeed = velOG.minAngularSpeed;
                velIn.maxAngularSpeed = velOG.maxAngularSpeed;
                velOG.enabled = false;


                Rigidbody rb = Instance.GetComponent<Rigidbody>();
                rb.useGravity = enableGravity;

                UnlockPickup unlockPickup = Instance.GetComponentInChildren<UnlockPickup>();
                unlockPickup.displayNameToken = displayNameToken;
                unlockPickup.unlockableDef = unlockableDef;
            }

        }
    }
}

using System;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

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
        public GameObject Instance => instance;
        [NonSerialized]
        private GameObject instance;

        private void OnEnable() => Refresh();
        private void OnDisable()
        {
            if (instance)
                GameObject.Destroy(instance);
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
            if (instance)
                GameObject.Destroy(instance);

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

            GameObject prefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/LogPickup.prefab").WaitForCompletion();
            if (NetworkServer.active)
            {
                instance = Instantiate(prefab, transform);
                NetworkServer.Spawn(instance);
            }

            Transform t = instance.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;

            VelocityRandomOnStart velOG = gameObject.GetComponent<VelocityRandomOnStart>();
            VelocityRandomOnStart velIn = instance.GetComponent<VelocityRandomOnStart>();
            velIn.minSpeed = velOG.minSpeed;
            velIn.maxSpeed = velOG.maxSpeed;
            velIn.baseDirection = velOG.baseDirection;
            velIn.localDirection = velOG.localDirection;
            velIn.directionMode = velOG.directionMode;
            velIn.coneAngle = velOG.coneAngle;
            velIn.minAngularSpeed = velOG.minAngularSpeed;
            velIn.maxAngularSpeed = velOG.maxAngularSpeed;
            velOG.enabled = false;


            Rigidbody rb = instance.GetComponent<Rigidbody>();
            rb.useGravity = enableGravity;

            UnlockPickup unlockPickup = instance.GetComponentInChildren<UnlockPickup>();
            unlockPickup.displayNameToken = displayNameToken;
            unlockPickup.unlockableDef = unlockableDef;
        }
    }
}

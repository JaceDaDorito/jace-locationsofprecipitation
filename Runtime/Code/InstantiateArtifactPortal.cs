using R2API;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace LOP
{
    public class InstantiateArtifactPortal : NetworkBehaviour
    {
        [Tooltip("Location of artifact portal buttons. You need exactly 9 buttons and first button should be top left, 3 per row, left to right.")]
        public Transform[] artifactButtons;

        [Tooltip("Location of the portal that spawns on successful code. Forward indicates direction of portal particles flow.")]
        public Transform portalLocation;

        [Tooltip("Location of the table and laptop. Forward indicates the direction of laptop screen.")]
        public Transform laptopLocation;

        private GameObject laptopInstance;

        private static GameObject laptopPrefab;

        private static Dictionary<int, int> buttonsPositions = new Dictionary<int, int>()
        {
            {0, 6}, {1, 3}, {2, 0}, {3, 7}, {4, 5}, {5, 1}, {6, 8}, {7, 4}, {8, 2}
        };

        private const uint NET_ID_DIRTY_BIT = 1u;

        private void Awake()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (artifactButtons.Length != 9)
            {
                LOPLog.Warning("There are more or less than 9 artifact buttons. Doing nothing...");
                return;
            }

            if (!laptopPrefab || !portalLocation || !laptopLocation)
            {
                return;
            }

            laptopInstance = UnityEngine.Object.Instantiate(laptopPrefab, laptopLocation.position, laptopLocation.rotation);
            var portalDialerController = laptopInstance.GetComponent<PortalDialerController>();
            portalDialerController.buttons = new PortalDialerButtonController[9];
            portalDialerController.dialingOrder = new PortalDialerButtonController[9];
            portalDialerController.portalSpawnLocation = portalLocation;
            NetworkServer.Spawn(laptopInstance);

            var portalButtonPrefab = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPaths.Version_1_35_0.RoR2_Base_skymeadow.PortalDialerButton_prefab).WaitForCompletion();
            for (int i = 0; i < artifactButtons.Length; i++)
            {
                var newObject = UnityEngine.Object.Instantiate(portalButtonPrefab, artifactButtons[i].position, artifactButtons[i].rotation);
                var dialerButtonController = newObject.GetComponent<PortalDialerButtonController>();
                portalDialerController.buttons[buttonsPositions[i]] = dialerButtonController;
                portalDialerController.dialingOrder[i] = dialerButtonController;
                NetworkServer.Spawn(newObject);
            }
            SetDirtyBit(NET_ID_DIRTY_BIT);
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            uint num = base.syncVarDirtyBits;
            if (initialState)
            {
                num = NET_ID_DIRTY_BIT;
            }

            if (laptopInstance)
            {
                writer.WritePackedUInt32(num);
                if ((num & NET_ID_DIRTY_BIT) != 0)
                {
                    writer.Write(laptopInstance.GetComponent<NetworkIdentity>().netId);
                }
            }

            return num != 0;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            uint num = reader.ReadPackedUInt32();
            if ((num & NET_ID_DIRTY_BIT) != 0)
            {
                NetworkInstanceId netId = reader.ReadNetworkId();

                var netObject = Util.FindNetworkObject(netId);
                if (netObject)
                {
                    var dialerController = netObject.GetComponent<PortalDialerController>();
                    if (dialerController)
                    {
                        dialerController.portalSpawnLocation = portalLocation;
                    }
                }
            }
        }

        public void OnDrawGizmos()
        {
            if (laptopLocation)
            {
                var position = new Vector3(laptopLocation.position.x, laptopLocation.position.y + 0.4f, laptopLocation.position.z);
                Gizmos.color = Color.gray;
                Gizmos.DrawWireMesh(Resources.GetBuiltinResource<Mesh>("Cube.fbx"), position, laptopLocation.rotation, new Vector3(5.15969753f, 1.89853132f, 1.95242488f));
                GUI.color = Color.gray;
            }
            foreach (var position in artifactButtons)
            {
                if (position)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireMesh(Resources.GetBuiltinResource<Mesh>("Cylinder.fbx"), position.position, position.rotation, new Vector3(1.51289475f, 0.523222327f, 1.54757667f));
                    GUI.color = Color.red;
                }
            }
        }

        void OnValidate()
        {
#if UNITY_EDITOR
            if(artifactButtons.Length != 9)
            {
                LOPLog.Error("Number of buttons is not equal to 9. This WILL result in artifact portal not spawning in game!");
            }
#endif
        }

        public static void CreateAndRegisterLaptop(ContentPack contentPack)
        {
            var entireIsland = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPaths.Version_1_35_0.RoR2_Base_skymeadow.PortalDialerEvent_prefab).WaitForCompletion();
            var laptopTransform = entireIsland.transform.Find("Final Zone/ButtonContainer/PortalDialer");
            if (!laptopTransform)
            {
                LOPLog.Warning($"Couldn't find PortalDialer in {entireIsland}. Not registering laptop to NetworkedObjects catalog...");
                return;
            }

            UnityEngine.Object.DestroyImmediate(laptopTransform.Find("spmSMGrassSmallCluster (3)").gameObject);
            UnityEngine.Object.DestroyImmediate(laptopTransform.Find("spmSMGrassSmallCluster (4)").gameObject);
            UnityEngine.Object.DestroyImmediate(laptopTransform.Find("SM_PowerLine").gameObject);

            laptopPrefab = laptopTransform.gameObject.InstantiateClone("LOP_ArtifactLaptop", true);
            contentPack.networkedObjectPrefabs.Add(new GameObject[] { laptopPrefab });
        }
    }
}

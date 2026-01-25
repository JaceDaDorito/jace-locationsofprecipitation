using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.ExpansionManagement;

using System;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static LOP.Constants.AssetGUIDS;

namespace LOP {
    //Need neb to review this
    public class SetupDLC3AccessNode : MonoBehaviour
    {
        public static GameObject accessNodeMissionController;

        [SerializeField]
        [Tooltip("Allows boss fight override. Solus Amalgamator by default.")]
        private bool overrideBossFight;
        [SerializeField]
        [Tooltip("Overrides the spawn card of the Access Node encounter.")]
        private DirectorCard spawnCard;
        [SerializeField]
        [Tooltip("Ignore Solus Wing defeat.")]
        private bool ignoreSolusWing = false;

        public static Action OnDLC3AccessNodeSetupStart;

        [SystemInitializer]
        private static IEnumerator SystemInit()
        {
            LOPLog.Info("Initializing DLC3 Access Node Assets...");

            var request = LocationsOfPrecipitation.lopAssetBundle.LoadAssetAsync<GameObject>("AccessNodeMissionController");
            while (!request.isDone)
                yield return null;

            accessNodeMissionController = request.asset as GameObject;

            AccessCodesMissionController acMissionControllerComp = accessNodeMissionController.GetComponent<AccessCodesMissionController>();
            SolusFight solusFight = accessNodeMissionController.GetComponent<SolusFight>();
            PortalSpawner ccPortalSpawner = solusFight.conduitCanyonPortalSpawner;
            PortalSpawner cePortalSpawner = solusFight.computationalExchangePortalSpawner;
            PortalSpawner shPortalSpawner = solusFight.solutionalHauntPortalSpawner;

            var dlc3Request = Addressables.LoadAssetAsync<ExpansionDef>(dlc3ExpansionGUID);
            while (!dlc3Request.IsDone)
                yield return null;
            acMissionControllerComp.requiredExpansion = dlc3Request.Result;
            solusFight.requiredExpansion = dlc3Request.Result;
            ccPortalSpawner.requiredExpansion = dlc3Request.Result;
            cePortalSpawner.requiredExpansion = dlc3Request.Result;
            shPortalSpawner.requiredExpansion = dlc3Request.Result;

            var ccPortalCardRequest = Addressables.LoadAssetAsync<InteractableSpawnCard>(Constants.AssetGUIDS.iscHardwareProgPortalGUID);
            while (!ccPortalCardRequest.IsDone)
                yield return null;
            ccPortalSpawner.portalSpawnCard = ccPortalCardRequest.Result;

            var cePortalCardRequest = Addressables.LoadAssetAsync<InteractableSpawnCard>(Constants.AssetGUIDS.iscSolusShopPortalGUID);
            while (!cePortalCardRequest.IsDone)
                yield return null;
            cePortalSpawner.portalSpawnCard = cePortalCardRequest.Result; 

            var shPortalCardRequest = Addressables.LoadAssetAsync<InteractableSpawnCard>(Constants.AssetGUIDS.iscHardwareProgPortalHauntGUID);
            while (!shPortalCardRequest.IsDone)
                yield return null;
            shPortalSpawner.portalSpawnCard = shPortalCardRequest.Result;
        }

        public void Start()
        {
            Action onDLC3AccessNodeSetupStart = OnDLC3AccessNodeSetupStart;
            if (onDLC3AccessNodeSetupStart != null) onDLC3AccessNodeSetupStart();

            //Access Node
            GameObject accessNodeAsset = Addressables.LoadAssetAsync<GameObject>(Constants.AssetGUIDS.accessNodeGUID).WaitForCompletion();
            GameObject accessNode = GameObject.Instantiate(accessNodeAsset, this.transform);
            NetworkServer.Spawn(accessNode);

            GameObject acMissionControllerInstance = GameObject.Instantiate(accessNodeMissionController);
            AccessCodesMissionController acMissionControllerComp = acMissionControllerInstance.GetComponent<AccessCodesMissionController>();
            acMissionControllerComp.ignoreSolusWingDeath = ignoreSolusWing;

            SolusFight solusFight = acMissionControllerInstance.GetComponent<SolusFight>();
            if (overrideBossFight)
            {
                solusFight.ForcedBossFight = spawnCard;
            }

            acMissionControllerComp.nodes = new AccessCodesNodeData[]
            {
                new AccessCodesNodeData()
                {
                    node = accessNode,
                    id = 0
                }
            };

            NetworkServer.Spawn(acMissionControllerInstance);
        }
    }
}

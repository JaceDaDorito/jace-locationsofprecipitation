using System;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Navigation;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RoR2.ExpansionManagement;
using UnityEngine.AddressableAssets;

namespace LOP
{
	public class QuickGameObjectExpansionActivator : MonoBehaviour
	{

		public AssetReferenceT<ExpansionDef> requiredExpansion;
        [HelpBox("If the expansion is not enabled, the activation state for the game objects is the opposite of this value", messageType: HelpBoxAttribute.MessageType.Info)]
        public bool activationStateIfExpansionEnabled;
        public GameObject[] objectsToActivate = Array.Empty<GameObject>();

		private void Awake()
		{
			ActivateOrDeactivateGameObjectsBasedOnExpansionStatus();
		}
		private void ActivateOrDeactivateGameObjectsBasedOnExpansionStatus()
		{
			bool expansionEnabled = GetExpansionEnabled();
			for (int i = 0; i < objectsToActivate.Length; i++)
			{
				ActivateOrDeactivateGameObject(objectsToActivate[i], expansionEnabled);
			}
		}

		private bool GetExpansionEnabled()
		{
			ExpansionDef expansionDef = Addressables.LoadAssetAsync<ExpansionDef>(requiredExpansion).WaitForCompletion();
			return !expansionDef || Run.instance.IsExpansionEnabled(expansionDef);
		}
		private void ActivateOrDeactivateGameObject(GameObject obj, bool isExpansionEnabled)
		{
			if (!obj)
			{
				return;
			}
			if (isExpansionEnabled)
			{
				obj.SetActive(activationStateIfExpansionEnabled);
				return;
			}
			obj.SetActive(!activationStateIfExpansionEnabled);
		}
	}
}
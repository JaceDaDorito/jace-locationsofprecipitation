using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static RoR2.SceneObjectSpawnGroup;

namespace LOP
{
    public class DLC4GongSpawner : MonoBehaviour
    {
        private static string GongAddressableId => RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4_TrialGong.TrialGong_prefab;

        [Min(0f)]
        public int minToSpawn;

        [Min(0f)]
        public int maxToSpawn;

        public Transform[] spawnPoints;

        public bool refreshInEditor;

        private GameObject[] instances;

        private void Start()
        {
            DoSpawns();
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            RedrawPreviews();
#endif
        }

        [ContextMenu("Redraw Previews")]
        public void RedrawPreviews()
        {
            DestroyEditorInstances();

            if (refreshInEditor)
            {
                instances = new GameObject[spawnPoints.Length];
                var asset = Addressables.LoadAssetAsync<GameObject>(GongAddressableId).WaitForCompletion();
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    var point = spawnPoints[i];
                    instances[i] = Instantiate(asset, point.position, point.rotation, point);
                    instances[i].hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.NotEditable;
                    foreach (Transform t in instances[i].GetComponentsInChildren<Transform>())
                    {
                        t.gameObject.hideFlags = instances[i].hideFlags;
                    }
                }
            }
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            DestroyEditorInstances();
#endif
        }

        private void DestroyEditorInstances()
        {
            if (instances == null)
            {
                return;
            }

            for (int i = 0; i < instances.Length; i++)
            {
                if (instances[i])
                {
                    DestroyImmediate(instances[i]);
                }
            }

            instances = Array.Empty<GameObject>();
        }

        private void DoSpawns()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (!Run.instance)
            {
                return;
            }

            if (!Run.instance.IsExpansionEnabled(DLC4Content.DLC4))
            {
                return;
            }

            Xoroshiro128Plus xoroshiro128Plus = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
            if (spawnPoints.Length == 0)
            {
                return;
            }
            int numberToSpawn = xoroshiro128Plus.RangeInt(minToSpawn, maxToSpawn + 1);

            if(numberToSpawn == 0)
            {
                return;
            }

            var prefab = Addressables.LoadAssetAsync<GameObject>(GongAddressableId).WaitForCompletion();
            var shuffeledSpawnPoins = new Transform[spawnPoints.Length];
            spawnPoints.CopyTo(shuffeledSpawnPoins, 0);
            Util.ShuffleArray(shuffeledSpawnPoins, xoroshiro128Plus);

            if(numberToSpawn > shuffeledSpawnPoins.Length)
            {
                numberToSpawn = shuffeledSpawnPoins.Length;
            }

            for(int i = 0; i < numberToSpawn; i++)
            {
                var point = shuffeledSpawnPoins[i];
                var instance = UnityEngine.Object.Instantiate(prefab, point.position, point.rotation, point);
                NetworkServer.Spawn(instance);
            }
        }

    }
}

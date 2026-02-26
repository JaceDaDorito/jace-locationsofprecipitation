using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LOP
{
    public class InstantiateArtifactFormulaDisplay : MonoBehaviour
    {
        public enum Codes
        {
            Circle,
            Diamond,
            Square,
            Triangle
        }

        [Tooltip("Location of artifact code slab. It will respect position and rotation, but not scale.")]
        public Transform position;

        [Tooltip("Codes to show. Codes start in top left corner, 3 per row. You need exactly 9, otherwise slab will not spawn.")]
        public Codes[] code;

        private void Awake()
        {
            if(code.Length != 9)
            {
                Log.Warning("Slab's code lenght is not equal to 9, doing nothing...");
                return;
            }

            var slab = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_artifactworld.ArtifactFormulaDisplay_prefab).WaitForCompletion(), position.position, position.rotation);
            var artifactFormulaDisplay = slab.GetComponent<ArtifactFormulaDisplay>();
            for(int i = 0; i < code.Length; i++)
            {
                artifactFormulaDisplay.artifactCompoundDisplayInfos[i].artifactCompoundDef = GetACD(code[i]);
            }

            ArtifactCompoundDef GetACD(Codes code)
            {
                switch (code)
                {
                    case Codes.Circle:
                        return Addressables.LoadAssetAsync<ArtifactCompoundDef>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_ArtifactCompounds.acdCircle_asset).WaitForCompletion();
                    case Codes.Diamond:
                        return Addressables.LoadAssetAsync<ArtifactCompoundDef>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_ArtifactCompounds.acdDiamond_asset).WaitForCompletion();
                    case Codes.Square:
                        return Addressables.LoadAssetAsync<ArtifactCompoundDef>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_ArtifactCompounds.acdSquare_asset).WaitForCompletion();
                    case Codes.Triangle:
                        return Addressables.LoadAssetAsync<ArtifactCompoundDef>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_ArtifactCompounds.acdTriangle_asset).WaitForCompletion();
                    default:
                        return Addressables.LoadAssetAsync<ArtifactCompoundDef>(RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_ArtifactCompounds.acdEmpty_asset).WaitForCompletion();
                }
            }
        }

        public void OnDrawGizmos()
        {
            if (position)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireMesh(Resources.GetBuiltinResource<Mesh>("Cube.fbx"), position.position, position.rotation, new Vector3(4.02848768f, 6.44617653f, 1.64969826f));
                GUI.color = Color.yellow;
            }
        }

        void OnValidate()
        {
#if UNITY_EDITOR
            if(code.Length != 9)
            {
                LOPLog.Error("Slab's code lenght is not equal to 9. This WILL result in slab not spawning in game!");
            }
#endif
        }
    }
}

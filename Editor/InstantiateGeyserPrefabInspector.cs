using UnityEditor;
using UnityEngine;
using LOP;

namespace LOP.Editor
{
    [CustomEditor(typeof(InstantiateGeyserPrefab))]
    public class InstantiateGeyserPrefabInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();
            InstantiateGeyserPrefab iGP = (InstantiateGeyserPrefab)target;
            if(iGP.geyserType == InstantiateGeyserPrefab.GeyserType.Fan)
            {
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                GUIContent gateToggleContent = new GUIContent();
                gateToggleContent.text = "Gate Toggle On Purchase";
                gateToggleContent.tooltip = "If true, it will enable or disable given gates on purchase.";
                iGP.gateToggleOnPurchase = EditorGUILayout.Toggle(gateToggleContent, iGP.gateToggleOnPurchase);
                EditorGUILayout.EndHorizontal();

                if (iGP.gateToggleOnPurchase)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUIContent gateEnableContent = new GUIContent();
                    gateEnableContent.text = "Gate To Enable When Purchased";
                    gateEnableContent.tooltip = "The gate that gets enabled when purchased.";
                    iGP.gateToEnableWhenPurchased = EditorGUILayout.TextField(gateEnableContent, iGP.gateToEnableWhenPurchased);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUIContent gateDisableContent = new GUIContent();
                    gateDisableContent.text = "Gate To Disable When Purchased";
                    gateDisableContent.tooltip = "The gate that gets disabled when purchased.";
                    iGP.gateToDisableWhenPurchased = EditorGUILayout.TextField(gateDisableContent, iGP.gateToDisableWhenPurchased);
                    EditorGUILayout.EndHorizontal();
                }
            }
            
        }

    }
}
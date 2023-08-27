using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using LOP;
using System.Collections.Generic;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace LOP.Editor
{
    internal static class GameobjectCreators
    {
        [MenuItem("GameObject/Risk Of Rain 2/Geyser", false, 10)]
        static void CreateGeyser(MenuCommand menuCommand)
        {
            var prefab = Constants.AssetGUIDS.QuickLoad<GameObject>(Constants.AssetGUIDS.geyserPrefabGUID);
            var instance = UnityEngine.Object.Instantiate(prefab);
            instance.name = "Geyser";
            GameObjectUtility.SetParentAndAlign(instance, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
            Selection.activeObject = instance;
        }
    }
}

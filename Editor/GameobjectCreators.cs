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
            CreateObject(menuCommand, Constants.AssetGUIDS.geyserPrefabGUID, "Geyser");
        }

        [MenuItem("GameObject/Risk Of Rain 2/Access Node Setup Object", false, 10)]
        static void CreateAccessNodeSetupObject(MenuCommand menuCommand)
        {
            CreateObject(menuCommand, Constants.AssetGUIDS.accessNodeSetupObjectGUID, "AccessNodeHolder");
        }

        [MenuItem("GameObject/Risk Of Rain 2/Wake", false, 10)]
        static void CreateWake(MenuCommand menuCommand)
        {
            CreateObject(menuCommand, Constants.AssetGUIDS.wakePrefabGUID, "Wake");
        }

        static void CreateObject(MenuCommand menuCommand, string GUID, string name)
        {
            var prefab = Constants.AssetGUIDS.QuickLoad<GameObject>(GUID);
            var instance = UnityEngine.Object.Instantiate(prefab);
            instance.name = name;
            GameObjectUtility.SetParentAndAlign(instance, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
            Selection.activeObject = instance;
        }
    }
}

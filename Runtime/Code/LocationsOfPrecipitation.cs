using BepInEx;
using RoR2;
using UnityEngine;
using Path = System.IO.Path;
using BepInEx.Logging;
using System.Security.Permissions;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace LOP
{
    [BepInPlugin(GUID, Name, Version)]
    public class LocationsOfPrecipitation: BaseUnityPlugin
    {
        public const string Author = "JaceDaDorito";

        public const string Name = nameof(LocationsOfPrecipitation);

        public const string Version = "1.2.1";

        public const string GUID = Author + "." + Name;

        public static LocationsOfPrecipitation Instance { get; private set; }
        public static PluginInfo PluginInfo { get; private set; }
        internal static new ManualLogSource Logger { get; set; }
        private static string AssemblyDir { get => (Path.GetDirectoryName(PluginInfo.Location)); }
        public static AssetBundle lopAssetBundle { get; private set; }
        private void Awake()
        {
            Instance = this;
            PluginInfo = Info;

            Logger = base.Logger;
            new LOPLog(Logger);

            lopAssetBundle = AssetBundle.LoadFromFile(Path.Combine(AssemblyDir, "runtimelopassetbundle"));

            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new ContentProvider());
        }
    }
}
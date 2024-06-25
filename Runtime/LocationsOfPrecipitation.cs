using BepInEx;
using RoR2;
using UnityEngine;
using Path = System.IO.Path;
using BepInEx.Logging;

namespace LOP
{
    [BepInPlugin(GUID, Name, Version)]
    public class LocationsOfPrecipitation: BaseUnityPlugin
    {
        public const string Author = "JaceDaDorito";

        public const string Name = nameof(LocationsOfPrecipitation);

        public const string Version = "1.1.1";

        public const string GUID = Author + "." + Name;

        public static LocationsOfPrecipitation Instance { get; private set; }
        public static PluginInfo PluginInfo { get; private set; }
        internal static new ManualLogSource Logger { get; set; }
        private static string AssemblyDir { get => (Path.GetDirectoryName(PluginInfo.Location)); }

        private void Awake()
        {
            Instance = this;
            PluginInfo = Info;

            Logger = base.Logger;
            new LOPLog(Logger);
        }
    }
}
using RoR2;
using RoR2.Audio;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace LOP
{
    public static class LOPUtil
    {
        public static void DestroyImmediateSafe(UnityEngine.Object obj, bool allowDestroyingAssets = false)
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(obj, allowDestroyingAssets);
#else
            GameObject.Destroy(obj);
#endif
        }
    }
}

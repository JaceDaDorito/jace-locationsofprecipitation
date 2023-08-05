using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace LOP
{
    internal class LOPLog
    {
        private static ManualLogSource logger = null;

        internal LOPLog(ManualLogSource logger_)
        {
            logger = logger_;
        }

        internal static void Debug(object data)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(data);
#else
            logger.LogDebug(data);
#endif
        }

        internal static void Error(object data)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(data);
#else
            logger.LogError(data);
#endif
        }

        internal static void Fatal(object data)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(data);
#else
            logger.LogFatal(data);
#endif
        }

        internal static void Info(object data)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(data);
#else
            logger.LogInfo(data);
#endif
        }

        internal static void Message(object data)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(data);
#else
            logger.LogMessage(data);
#endif
        }

        internal static void Warning(object data)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning(data);
#else
            logger.LogWarning(data);
#endif
        }
    }
}

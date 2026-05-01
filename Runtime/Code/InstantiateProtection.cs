using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LOP
{
    /// <summary>
    /// Destroys GameObject if it was created via Instantiate
    /// </summary>
    [ExecuteAlways]
    public class InstantiateProtection : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private bool instantiated;

        private void Awake()
        {
            if (instantiated)
            {
                LOPUtil.DestroyImmediateSafe(gameObject, true);
            }
            instantiated = true;
        }
    }
}

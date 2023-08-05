﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LOP
{
    /// <summary>
    /// Component that injects an AddressableAsset to a component's field
    /// </summary>
    [ExecuteAlways]
    public class AddressableInjector : MonoBehaviour
    {
        [Tooltip("The address used for injecting")]
        public string address;
        /// <summary>
        /// The Loaded Asset
        /// </summary>
        public Object Asset { get => _asset; private set => _asset = value; }
        [NonSerialized] private Object _asset;

        [Tooltip("The component that will be injected")]
        [SerializeField] private Component targetComponent;
        [Tooltip("The member info that'll be injected")]
        [SerializeField] private string targetMemberInfoName;

        private MemberInfo cachedMemberInfo;

        private void OnEnable() => Refresh();

        /// <summary>
        /// Refreshes and re-injects the asset specified in <see cref="address"/>
        /// </summary>
        public void Refresh()
        {
            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrEmpty(address))
            {
#if DEBUG
                string msg = $"Invalid address in {this}, address is null, empty, or white space";
                LOPLog.Warning(msg);
#endif
                return;
            }

            if (!targetComponent)
            {
#if DEBUG
                string msg = $"No Target Component Set in {this}";
                LOPLog.Warning(msg);
#endif
                return;
            }

            if (string.IsNullOrEmpty(targetMemberInfoName) || string.IsNullOrWhiteSpace(targetMemberInfoName))
            {
#if DEBUG
                string msg = $"{this}'s targetMemberInfoName is null, empty or white space";
                LOPLog.Warning(msg);
#endif
                return;
            }

            var memberInfo = GetMemberInfo();
            if (memberInfo == null)
            {
#if DEBUG
                string msg = $"{this} failed finding the MemberInfo to target based on the name \"{targetMemberInfoName}\". Target Component: {targetComponent}";
                LOPLog.Warning(msg); ;
#endif
                return;
            }

            var _asset = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Object>(address).WaitForCompletion();
            if (!_asset)
                return;
            Asset = _asset;
#if UNITY_EDITOR
            Asset = Instantiate(_asset);
#endif
            Asset.hideFlags = HideFlags.DontSaveInEditor | HideFlags.NotEditable | HideFlags.DontSaveInBuild;

            Inject(memberInfo);
        }

        private void Inject(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case PropertyInfo pInfo: InjectPropertyInfo(pInfo); break;
                case FieldInfo fInfo: InjectFieldInfo(fInfo); break;
            }

            void InjectPropertyInfo(PropertyInfo propertyInfo)
            {
                try
                {
                    propertyInfo.SetValue(targetComponent, Asset);
                }
                catch (Exception e)
                {
                    LOPLog.Error(e);
                }

#if UNITY_EDITOR
                LOPLog.Info($"injected {Asset} onto {targetComponent}'s propertyInfo, setting propertyInfo value to null to avoid broken scenes/objects");
                propertyInfo.SetValue(targetComponent, null);
                DestroyImmediate(Asset);
#endif
            }

            void InjectFieldInfo(FieldInfo fieldInfo)
            {
                try
                {
                    fieldInfo.SetValue(targetComponent, Asset);
                }
                catch (Exception e)
                {
                    LOPLog.Error(e);
                }

#if UNITY_EDITOR
                LOPLog.Info($"injected {Asset} onto {targetComponent}'s fieldInfo, setting fieldInfo value to null to avoid broken scenes/objects");
                fieldInfo.SetValue(targetComponent, null);
                DestroyImmediate(Asset);
#endif
            }
        }

        private MemberInfo GetMemberInfo()
        {
            if ((cachedMemberInfo == null || $"({cachedMemberInfo.DeclaringType.Name}) {cachedMemberInfo.Name}" != targetMemberInfoName) && targetComponent)
            {
                cachedMemberInfo = targetComponent.GetType()
                    .GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                    .Where(m =>
                    {
                        string memberTypeName = m.GetType().Name;
                        return memberTypeName == "MonoProperty" || memberTypeName == "MonoField" || memberTypeName == "FieldInfo" || memberTypeName == "PropertyInfo";
                    })
                    .FirstOrDefault(m => $"({m.DeclaringType.Name}) {m.Name}" == targetMemberInfoName);
            }

            return cachedMemberInfo;
        }

#if UNITY_EDITOR
        private void OnDisable() => RemoveReferencesEditor();
        private void RemoveReferencesEditor()
        {
            var memberInfo = GetMemberInfo();

            switch (memberInfo)
            {
                case PropertyInfo pInfo:
                    pInfo.SetValue(targetComponent, null);
                    break;
                case FieldInfo fInfo:
                    fInfo.SetValue(targetComponent, null);
                    break;
            }
        }
#endif
    }
}
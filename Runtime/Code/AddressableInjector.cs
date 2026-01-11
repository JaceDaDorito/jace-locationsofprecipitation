using HG;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace LOP
{
    /// <summary>
    /// Component that injects an AddressableAsset to a component's field
    /// </summary>
    [ExecuteAlways]
    public class AddressableInjector : MonoBehaviour
    {
        [Serializable]
        private struct SerializedFieldOrPropertyInfo
        {
            [SerializeField]
            private readonly string fieldOrPropertyName;
            [SerializeField]
            private readonly SerializableSystemType fieldOrPropertyType;

            private MemberInfo _cachedMemberInfo;
            public bool TryGetMemberInfo(Type declaringType, out MemberInfo result)
            {
                if(_cachedMemberInfo != null)
                {
                    result = _cachedMemberInfo;
                    return true;
                }

                result = null;
                MemberTypes memberTypes = MemberTypes.Field | MemberTypes.Property;
                BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

                MemberInfo[] memberInfos = declaringType.GetMember(fieldOrPropertyName, memberTypes, bindingFlags);
                foreach(var memberInfo in memberInfos)
                {
                    if(memberInfo.Name == fieldOrPropertyName)
                    {
                        Type foundTypeInfo = GetFieldOrPropertyTypeFromMemberInfo(memberInfo);
                        Type serializedType = (Type)fieldOrPropertyType;
                        if(foundTypeInfo.IsSubclassOf(serializedType) || foundTypeInfo == serializedType)
                        {
                            result = memberInfo;
                            _cachedMemberInfo = result;
                            return true;
                        }
                    }
                }

                return false;
            }

            private Type GetFieldOrPropertyTypeFromMemberInfo(MemberInfo memberInfo)
            {
                if(memberInfo is FieldInfo fInfo)
                {
                    return fInfo.FieldType;
                }
                else if(memberInfo is PropertyInfo pInfo)
                {
                    return pInfo.PropertyType;
                }
                return null;
            }
        }

        [Tooltip("The address used for injecting")]
        //While this is a raw string, it'll be presented in the editor as an addressable dropdown.
        public string address;
        /// <summary>
        /// The Loaded Asset
        /// </summary>
        [field: NonSerialized]
        public Object Asset { get; private set; }

        [Tooltip("The component that will be injected")]
        [SerializeField] private Component targetComponent;
        [SerializeField] private SerializedFieldOrPropertyInfo targetMemberInfo;

        #region Obsolete
        [Obsolete("Utilize targetMemberInfo instead")]
        [SerializeField] private string targetMemberInfoName; //This has the targetComponent as the preffix, it isnt that great, so instead the new one uses the type of the field/property as the preffix.
        private MemberInfo cachedMemberInfo;
        #endregion

        private AsyncOperationHandle<Object> _operationHandle;

        //Refresh assigns operation handle, OnDisable must release it if it exists
        private void OnEnable() => Refresh();

        private void OnDisable()
        {
            if(_operationHandle.IsValid())
            {
                Addressables.Release(_operationHandle);
            }

#if UNITY_EDITOR
            RemoveReferencesEditor();
#endif
        }

        /// <summary>
        /// Refreshes and re-injects the asset specified in <see cref="address"/>
        /// </summary>
        public void Refresh()
        {
            if (string.IsNullOrWhiteSpace(address))
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

            if(!TryGetMemberInfo(targetComponent.GetType(), out var memberInfo))
            {
#if DEBUG
                string msg = $"{this} failed finding the MemberInfo within {targetComponent}, this can be because the member info name was invalid.";
                LOPLog.Warning(msg); ;
#endif
                return;
            }

            //Release operation handle if valid, this could happen if the address changes and then the behaviour cycles enabled state. or if someone manually calls Refresh
            if(_operationHandle.IsValid())
            {
                Addressables.Release(_operationHandle);
            }

            _operationHandle = Addressables.LoadAssetAsync<Object>(address);
            var asset = _operationHandle.WaitForCompletion();
            if (!asset)
                return;
            Asset = asset;
#if UNITY_EDITOR
            Asset = Instantiate(asset);
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

        private bool TryGetMemberInfo(Type componentType, out MemberInfo memberInfo)
        {
            if(targetMemberInfo.TryGetMemberInfo(componentType, out memberInfo))
            {
                return true;
            }

            //If the targetMemberInfo did not work, proceed to use the obsolete version.
            if ((cachedMemberInfo == null || $"({cachedMemberInfo.DeclaringType.Name}) {cachedMemberInfo.Name}" != targetMemberInfoName) && targetComponent)
            {
                cachedMemberInfo = targetComponent.GetType()
                    .GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                    .Where(m =>
                    {
                        string memberTypeName = m.GetType().Name;
                        return memberTypeName.Contains("Property") || memberTypeName.Contains("Field");
                    })
                    .FirstOrDefault(m => $"({m.DeclaringType.Name}) {m.Name}" == targetMemberInfoName);
            }

            memberInfo = cachedMemberInfo;
            return memberInfo != null;
        }

#if UNITY_EDITOR

        private void RemoveReferencesEditor()
        {
            if (targetComponent == null)
                return;

            if(!TryGetMemberInfo(targetComponent.GetType(), out var memberInfo))
            {
                return;
            }

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
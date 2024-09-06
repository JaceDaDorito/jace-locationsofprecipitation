﻿using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using LOP;

namespace LOP.Editor
{
    [CustomEditor(typeof(AddressableInjector))]
    public class AddressablesAssetEditor : UnityEditor.Editor
    {
        private int _selectedMemberInfo = 0;
        private string[] _memberInfoNames;
        private MemberInfo[] _memberInfos;

        private SerializedProperty _key;
        private SerializedProperty _targetComponent;
        private SerializedProperty _targetMemberInfoName;

        private void OnEnable()
        {
            _key = serializedObject.FindProperty("address");
            _targetComponent = serializedObject.FindProperty("targetComponent");
            _targetMemberInfoName = serializedObject.FindProperty("targetMemberInfoName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var addressablesAsset = (AddressableInjector)serializedObject.targetObject;

            var newTargetComponent = (Component)EditorGUILayout.ObjectField("Target Component", _targetComponent.objectReferenceValue, typeof(Component), true);
            if (newTargetComponent && (!_targetComponent.objectReferenceValue || newTargetComponent.GetType() != _targetComponent.objectReferenceValue.GetType()))
            {
                _targetComponent.objectReferenceValue = newTargetComponent;

                RefreshMembersOfTargetComponent();
            }

            if (_memberInfoNames != null && _memberInfoNames.Length > 0)
            {
                var newSelectedMemberInfo = EditorGUILayout.Popup("Target Field", _selectedMemberInfo, _memberInfoNames);
                if (newSelectedMemberInfo != _selectedMemberInfo)
                {
                    _selectedMemberInfo = newSelectedMemberInfo;

                    _targetMemberInfoName.stringValue = _memberInfoNames[_selectedMemberInfo];
                }
            }
            else if (!string.IsNullOrWhiteSpace(_targetMemberInfoName.stringValue))
            {
                RefreshMembersOfTargetComponent();

                var indexOf = Array.IndexOf(_memberInfoNames, _targetMemberInfoName.stringValue);
                if (indexOf != -1)
                {
                    _selectedMemberInfo = indexOf;
                }
            }

            _key.stringValue = EditorGUILayout.TextField("Key", _key.stringValue);

            if (serializedObject.ApplyModifiedProperties())
            {
                addressablesAsset.Refresh();
            }
        }

        private void RefreshMembersOfTargetComponent()
        {
            _memberInfos = _targetComponent.
                objectReferenceValue.
                GetType().
                GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).
                OrderBy(m => m.Name).ToArray();
                
            var props = _memberInfos.OfType<PropertyInfo>().Where(p => p.PropertyType.IsSubclassOf(typeof(UnityEngine.Object)) && p.GetSetMethod(true) != null).Cast<MemberInfo>();
            var fields = _memberInfos.OfType<FieldInfo>().Where(f => f.FieldType.IsSubclassOf(typeof(UnityEngine.Object))).Cast<MemberInfo>();
            _memberInfos = props.Concat(fields).ToArray();
            _memberInfoNames = _memberInfos.Select(x => $"({x.DeclaringType.Name}) {x.Name}").ToArray();
        }
    }
}
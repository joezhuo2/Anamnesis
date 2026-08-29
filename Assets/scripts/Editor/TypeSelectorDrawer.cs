using System;
using System.Collections.Generic;
using System.Linq;
using CrystalFlux.Core;
using UnityEditor;
using UnityEngine;

namespace CrystalFlux.EditorTools
{
    [CustomPropertyDrawer(typeof(TypeSelectorAttribute))]
    public class TypeSelectorDrawer : PropertyDrawer
    {
        private const string SelectLabel = "Select Type...";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsNullManagedReference(property))
                return EditorGUIUtility.singleLineHeight;

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsNullManagedReference(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            DrawTypeSelector(position, property, label);
        }

        private static bool IsNullManagedReference(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceValue == null;

        private void DrawTypeSelector(Rect position, SerializedProperty property, GUIContent label)
        {
            var baseType = GetElementType();
            var types = GetImplementations(baseType);
            if (types.Count == 0)
            {
                var hint = baseType != null ? $"no serializable {baseType.Name} implementations" : "unsupported field type";
                EditorGUI.LabelField(position, label, new GUIContent(hint));
                return;
            }

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            var buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);

            EditorGUI.LabelField(labelRect, label);
            if (!EditorGUI.DropdownButton(buttonRect, new GUIContent(SelectLabel), FocusType.Keyboard, EditorStyles.popup))
                return;

            var targetObject = property.serializedObject.targetObject;
            var path = property.propertyPath;
            var menu = new GenericMenu();

            foreach (var type in types)
            {
                var implementation = type;
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(implementation.Name)), false, () =>
                {
                    var element = new SerializedObject(targetObject).FindProperty(path);
                    if (element == null) return;

                    element.managedReferenceValue = Activator.CreateInstance(implementation);
                    element.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.DropDown(buttonRect);
        }

        private Type GetElementType()
        {
            var fieldType = fieldInfo?.FieldType;
            if (fieldType == null) return null;

            if (fieldType.IsArray) return fieldType.GetElementType();
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                return fieldType.GetGenericArguments()[0];

            return fieldType;
        }

        private static List<Type> GetImplementations(Type baseType)
        {
            if (baseType == null) return new List<Type>();

            return TypeCache.GetTypesDerivedFrom(baseType)
                .Where(IsSelectable)
                .OrderBy(t => t.Name)
                .ToList();
        }

        private static bool IsSelectable(Type t)
            => t.IsClass
               && !t.IsAbstract
               && !t.IsGenericTypeDefinition
               && !typeof(UnityEngine.Object).IsAssignableFrom(t)
               && t.IsDefined(typeof(SerializableAttribute), false)
               && t.GetConstructor(Type.EmptyTypes) != null;
    }
}

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

[CustomEditor(typeof(ScriptableObject), true)]
public class ShowInInspectorEditor : Editor
{
    // Dictionary to store last values of fields by their names
    private Dictionary<string, object> _lastValues = new Dictionary<string, object>();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DrawShowInInspectorMembers(target.GetType(), target);
    }

    private void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate; // Subscribe to the update event to check for changes continuously and repaint.
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    private void DrawShowInInspectorMembers(Type type, object instance, string prefix = "")
    {
        if (instance == null || type == typeof(object))
            return;


        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        // Draw fields with [ShowInInspector]
        foreach (var field in type.GetFields(flags))
        {
            if (field.GetCustomAttribute<ShowInInspectorAttribute>() != null)
            {
                DrawValue(prefix + ObjectNames.NicifyVariableName(field.Name), field.GetValue(instance));
            }

            // Recurse into non-primitive nested fields
            if (ShouldRecurse(field.FieldType) && field.IsPublic)
            {
                var nested = field.GetValue(instance);
                if (nested != null)
                {
                    DrawShowInInspectorMembers(field.FieldType, nested, prefix + ObjectNames.NicifyVariableName(field.Name) + ".");
                }
            }
        }

        // Draw properties with [ShowInInspector]
        foreach (var prop in type.GetProperties(flags))
        {
            MethodInfo methodInfo = prop.GetGetMethod(true);
            if (methodInfo == null) continue;

            if (prop.GetCustomAttribute<ShowInInspectorAttribute>() != null)
            {
                object value = null;
                try { value = prop.GetValue(instance); } catch { /* skip inaccessible props */ }

                DrawValue(prefix + ObjectNames.NicifyVariableName(prop.Name), value);
            }

            // Recurse into readable complex properties
            if (ShouldRecurse(prop.PropertyType) && methodInfo.IsPublic)
            {
                object nested = null;
                try { nested = prop.GetValue(instance); } catch { }
                if (nested != null)
                {
                    DrawShowInInspectorMembers(prop.PropertyType, nested, prefix + ObjectNames.NicifyVariableName(prop.Name) + ".");
                }
            }
        }
    }

    private void OnEditorUpdate()
    {
        if (CheckForValueChanges(target.GetType(), target))
        {
            Repaint();
        }
    }

    private bool CheckForValueChanges(Type type, object instance, string prefix = "")
    {
        if (instance == null || type == typeof(object)) return false;

        bool hasChanges = false;
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        // Draw fields with [ShowInInspector]
        foreach (var field in type.GetFields(flags))
        {
            if (field.GetCustomAttribute<ShowInInspectorAttribute>() != null)
            {
                var currentValue = field.GetValue(target);
                if (!_lastValues.ContainsKey(field.Name) || !Equals(currentValue, _lastValues[field.Name]))
                {
                    hasChanges = true;
                    _lastValues[field.Name] = currentValue;  // Update the dictionary with the new value
                }
            }

            // Recurse into non-primitive nested fields
            if (ShouldRecurse(field.FieldType) && field.IsPublic)
            {
                var fieldValue = field.GetValue(instance);
                if (fieldValue != null)
                {
                    hasChanges |= CheckForValueChanges(field.FieldType, fieldValue, prefix + ObjectNames.NicifyVariableName(field.Name) + ".");
                }
            }
        }

        // Draw properties with [ShowInInspector]
        foreach (var prop in type.GetProperties(flags))
        {
            MethodInfo methodInfo = prop.GetGetMethod(true);
            if (methodInfo == null) continue;

            if (prop.GetCustomAttribute<ShowInInspectorAttribute>() != null)
            {
                object value = null;
                try { value = prop.GetValue(instance); } catch { /* skip inaccessible props */ }

                if (!_lastValues.ContainsKey(prop.Name) || !Equals(value, _lastValues[prop.Name]))
                {
                    hasChanges = true;
                    _lastValues[prop.Name] = value;  // Update the dictionary with the new value
                }
            }

            // Recurse into readable complex properties
            if (ShouldRecurse(prop.PropertyType) && methodInfo.IsPublic)
            {
                object propValue = null;
                try { propValue = prop.GetValue(instance); } catch { }
                if (propValue != null)
                {
                    hasChanges |= CheckForValueChanges(prop.PropertyType, propValue, prefix + ObjectNames.NicifyVariableName(prop.Name) + ".");
                }
            }
        }

        return hasChanges;
    }

    private void DrawValue(string label, object value)
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.LabelField(label, value != null ? value.ToString() : "null");
        EditorGUI.EndDisabledGroup();
    }

    private bool ShouldRecurse(Type type)
    {
        return !type.IsPrimitive &&
               !type.IsEnum &&
               !type.IsValueType &&
               type != typeof(string) &&
               !typeof(IEnumerable).IsAssignableFrom(type) &&
               !typeof(UnityEngine.Object).IsAssignableFrom(type) &&
               !typeof(Delegate).IsAssignableFrom(type);
    }
}
#endif
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ChildSelector : MonoBehaviour
{
    [SerializeField, Tooltip("Index of the active child")]
    private int activeChildIndex = 0;
    
    [SerializeField, Tooltip("Name of the currently active child")]
    private string activeChildName = "";
    
    // This will be used to detect if children have changed
    private int childCount = 0;
    
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        UpdateActiveChild();
    }
    
    private void UpdateActiveChild()
    {
        // First check if we have any children
        if (transform.childCount == 0)
        {
            activeChildIndex = -1;
            activeChildName = "No children";
            return;
        }
        
        // Ensure activeChildIndex is within valid range
        activeChildIndex = Mathf.Clamp(activeChildIndex, 0, transform.childCount - 1);
        
        // Deactivate all children first
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        
        // Activate only the selected child
        GameObject activeChild = transform.GetChild(activeChildIndex).gameObject;
        activeChild.SetActive(true);
        activeChildName = activeChild.name;
        
        // Update child count
        childCount = transform.childCount;
    }
    
    //// This ensures our selection persists when the scene loads
    //private void Start()
    //{
    //    UpdateActiveChild();
    //}
    
    //// This helps catch when children are added or removed at runtime
    //private void Update()
    //{
    //    if (childCount != transform.childCount)
    //    {
    //        UpdateActiveChild();
    //    }
    //}
}

#if UNITY_EDITOR
[CustomEditor(typeof(ChildSelector))]
public class ChildSelectorEditor : Editor
{
    private ChildSelector selector;
    private string[] childNames;
    
    private void OnEnable()
    {
        selector = (ChildSelector)target;
        UpdateChildNames();
    }
    
    private void UpdateChildNames()
    {
        Transform transform = selector.transform;
        childNames = new string[transform.childCount];
        
        for (int i = 0; i < transform.childCount; i++)
        {
            childNames[i] = transform.GetChild(i).name;
        }
    }
    
    public override void OnInspectorGUI()
    {
        // Draw default inspector for other properties
        DrawDefaultInspector();
        
        // Return early if no children
        if (selector.transform.childCount == 0)
        {
            EditorGUILayout.HelpBox("This GameObject has no children.", MessageType.Info);
            return;
        }
        
        // Update child names in case they've changed
        UpdateChildNames();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Child Selection", EditorStyles.boldLabel);
        
        // Get the serialized property
        SerializedProperty activeChildIndexProp = serializedObject.FindProperty("activeChildIndex");
        
        // Store the current value to detect changes
        int currentIndex = activeChildIndexProp.intValue;
        
        // Create a dropdown to select children
        int newIndex = EditorGUILayout.Popup("Active Child", currentIndex, childNames);
        
        // Up/Down arrow controls
        EditorGUILayout.BeginHorizontal();
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("◀", GUILayout.Width(30)))
        {
            newIndex = (currentIndex - 1 + childNames.Length) % childNames.Length;
        }
        
        EditorGUILayout.LabelField(currentIndex + 1 + " / " + childNames.Length, GUILayout.Width(50));
        
        if (GUILayout.Button("▶", GUILayout.Width(30)))
        {
            newIndex = (currentIndex + 1) % childNames.Length;
        }
        
        EditorGUILayout.EndHorizontal();
        
        // If the index changed, update the property
        if (newIndex != currentIndex)
        {
            activeChildIndexProp.intValue = newIndex;
            serializedObject.ApplyModifiedProperties();
            
            // Force the GameObject to update
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
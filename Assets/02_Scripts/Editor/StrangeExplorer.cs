using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace StrangeSpace
{
    public class StrangeExplorer : EditorWindow
    {
        private readonly ActionQueue _actionQueue = new ActionQueue();
        
        private Vector2 _listScrollPosition;
        private Vector2 _detailsScrollPosition;
        
        private string _filterText = "";
        
        private IEditorInspector _currentInspector;
        private IEditorInspectorProvider _currentInspectorProvider;

        [MenuItem("Strange/Strange Explorer")]
        public static void ShowWindow()
        {
            var window = GetWindow<StrangeExplorer>();
            window.titleContent = new GUIContent("Strange Explorer");
            window.Show();
        }

        private void OnEnable()
        {
  
        }
        
        private void OnGUI()
        {
            if(EditorDependencies.Inspectors == null || EditorDependencies.Inspectors.Count == 0)
            {
                EditorGUILayout.LabelField("No inspectors found");
                return;
            }
            _actionQueue.ProcessQueuedActions();
            
            EditorGUILayout.BeginHorizontal();

            DrawTaskList();
            DrawPanelDetails();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTaskList()
        {
            var inspectors = EditorDependencies.Inspectors;

            
            EditorGUILayout.BeginVertical(GUILayout.Width(250));

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Available Inspectors", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            string newFilterText = EditorGUILayout.TextField("Filter:", _filterText);
            if (newFilterText != _filterText)
            {
                _filterText = newFilterText;
                // FilterTasks();
            }

            _listScrollPosition = EditorGUILayout.BeginScrollView(_listScrollPosition);

            // Change with sections.
            foreach (var inspector in inspectors)
            {
                var inspectorInfo = inspector.GetInspectorInfo();
                
                EditorGUILayout.BeginVertical();
                if (GUILayout.Button(inspectorInfo.Name))
                {
                    _actionQueue.EnqueueAction(() =>
                    {
                        _currentInspectorProvider = inspector;
                        _currentInspector = inspector.GetInspector();
                    });
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawPanelDetails()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width*2 / 3 - 5));

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Inspector", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            _detailsScrollPosition = EditorGUILayout.BeginScrollView(_detailsScrollPosition);


            if (_currentInspector != null)
            {
                var info = _currentInspectorProvider.GetInspectorInfo();

                EditorGUILayout.LabelField(info.Name, EditorStyles.boldLabel);

                EditorGUILayout.BeginVertical();
                _currentInspector.OnGui();
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField("Select a task to view details", EditorStyles.wordWrappedLabel);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
}
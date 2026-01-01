using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace StrangeSpace
{
    public class TrackerServiceInspector : ZenEditorInspector
    {
        private TrackerService _trackerService;
        private Vector2 _objectsScrollPosition;
        private Vector2 _searchResultsScrollPosition;
        
        private bool _showRegisteredObjects = true;
        private bool _showSearchInterface = true;
        private bool _showStatistics = true;
        private bool _showControls = true;
        
        // Search functionality
        private string _searchPattern = "";
        private List<string> _searchResults = new List<string>();
        private List<GameObject> _searchObjectResults = new List<GameObject>();
        private bool _hasSearched = false;
        
        // Test registration
        private string _testId = "TestObject";
        private GameObject _testObject;
        
        // Filter options
        private bool _showNullObjects = true;
        private bool _showActiveOnly = false;
        private string _nameFilter = "";

        public TrackerServiceInspector(TrackerService trackerService)
        {
            _trackerService = trackerService;
        }

        public override void OnDrawGui()
        {
#if UNITY_EDITOR
            EditorGUILayout.Space(10);
            DrawHeader();
            EditorGUILayout.Space(5);
            
            if (_showStatistics = EditorGUILayout.Foldout(_showStatistics, "Statistics", true))
            {
                DrawStatistics();
            }
            EditorGUILayout.Space(10);
            
            if (_showSearchInterface = EditorGUILayout.Foldout(_showSearchInterface, "Search Interface", true))
            {
                DrawSearchInterface();
            }
            EditorGUILayout.Space(10);
            
            if (_showRegisteredObjects = EditorGUILayout.Foldout(_showRegisteredObjects, "Registered Objects", true))
            {
                DrawRegisteredObjects();
            }
            EditorGUILayout.Space(10);
#endif
        }

#if UNITY_EDITOR
        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.LabelField("Tracker Service Inspector", headerStyle);

            EditorGUILayout.EndVertical();
        }

        private void DrawStatistics()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Total Tracked Objects:", _trackerService.Count.ToString());
            
            // Count active/inactive objects
            var ids = _trackerService.GetIds();
            int activeCount = 0;
            int inactiveCount = 0;
            int nullCount = 0;
            
            foreach (var id in ids)
            {
                var obj = _trackerService.GetObject(id);
                if (obj == null)
                    nullCount++;
                else if (obj.activeInHierarchy)
                    activeCount++;
                else
                    inactiveCount++;
            }
            
            EditorGUILayout.LabelField("Active Objects:", activeCount.ToString());
            EditorGUILayout.LabelField("Inactive Objects:", inactiveCount.ToString());
            if (nullCount > 0)
            {
                Color originalColor = GUI.color;
                GUI.color = Color.red;
                EditorGUILayout.LabelField("Null Objects:", nullCount.ToString());
                GUI.color = originalColor;
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void DrawSearchInterface()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Search Patterns", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Use '*' for wildcard searches (e.g., 'Player*')", EditorStyles.miniLabel);
            
            EditorGUILayout.BeginHorizontal();
            _searchPattern = EditorGUILayout.TextField("Search Pattern:", _searchPattern);
            
            if (GUILayout.Button("Search IDs", GUILayout.Width(80)))
            {
                PerformSearch();
            }
            
            if (GUILayout.Button("Search Objects", GUILayout.Width(100)))
            {
                PerformObjectSearch();
            }
            
            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                ClearSearch();
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (_hasSearched)
            {
                EditorGUILayout.Space(5);
                
                if (_searchResults.Count > 0 || _searchObjectResults.Count > 0)
                {
                    string resultType = _searchResults.Count > 0 ? "IDs" : "Objects";
                    int resultCount = _searchResults.Count > 0 ? _searchResults.Count : _searchObjectResults.Count;
                    
                    EditorGUILayout.LabelField($"Search Results ({resultCount} {resultType}):", EditorStyles.boldLabel);
                    
                    _searchResultsScrollPosition = EditorGUILayout.BeginScrollView(_searchResultsScrollPosition, GUILayout.Height(150));
                    
                    if (_searchResults.Count > 0)
                    {
                        foreach (var id in _searchResults)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(id);
                            
                            var obj = _trackerService.GetObject(id);
                            if (obj != null)
                            {
                                EditorGUILayout.ObjectField(obj, typeof(GameObject), true, GUILayout.Width(150));
                            }
                            else
                            {
                                Color originalColor = GUI.color;
                                GUI.color = Color.red;
                                EditorGUILayout.LabelField("NULL", GUILayout.Width(150));
                                GUI.color = originalColor;
                            }
                            
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        foreach (var obj in _searchObjectResults)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    
                    EditorGUILayout.EndScrollView();
                }
                else
                {
                    EditorGUILayout.LabelField("No results found", EditorStyles.centeredGreyMiniLabel);
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawRegisteredObjects()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Filters
            EditorGUILayout.BeginHorizontal();
            _showNullObjects = EditorGUILayout.Toggle("Show Null Objects", _showNullObjects);
            _showActiveOnly = EditorGUILayout.Toggle("Active Only", _showActiveOnly);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name Filter:", GUILayout.Width(80));
            _nameFilter = EditorGUILayout.TextField(_nameFilter);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            var ids = _trackerService.GetIds();
            var filteredIds = ApplyFilters(ids);
            
            EditorGUILayout.LabelField($"Showing {filteredIds.Count} of {ids.Count} objects");
            
            if (filteredIds.Count > 0)
            {
                _objectsScrollPosition = EditorGUILayout.BeginScrollView(_objectsScrollPosition);
                
                foreach (var id in filteredIds.OrderBy(x => x))
                {
                    DrawObjectEntry(id);
                }
                
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.LabelField("No objects match filter", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawObjectEntry(string id)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            var obj = _trackerService.GetObject(id);
            
            EditorGUILayout.BeginHorizontal();
            
            // ID
            EditorGUILayout.LabelField(id, GUILayout.Width(150));
            
            // Object field
            if (obj != null)
            {
                Color originalColor = GUI.color;
                if (!obj.activeInHierarchy)
                    GUI.color = Color.gray;
                    
                EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                GUI.color = originalColor;
            }
            else
            {
                Color originalColor = GUI.color;
                GUI.color = Color.red;
                EditorGUILayout.LabelField("NULL OBJECT", EditorStyles.boldLabel);
                GUI.color = originalColor;
            }
            
            // Copy button
            if (GUILayout.Button("Copy ID", GUILayout.Width(60)))
            {
                GUIUtility.systemCopyBuffer = id;
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Additional info
            if (obj != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"Active: {obj.activeInHierarchy}");
                if (obj.transform.parent != null)
                    EditorGUILayout.LabelField($"Parent: {obj.transform.parent.name}");
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void PerformSearch()
        {
            _searchResults = _trackerService.GetIds(_searchPattern);
            _searchObjectResults.Clear();
            _hasSearched = true;
        }

        private void PerformObjectSearch()
        {
            _searchObjectResults = _trackerService.GetObjects(_searchPattern);
            _searchResults.Clear();
            _hasSearched = true;
        }

        private void ClearSearch()
        {
            _searchResults.Clear();
            _searchObjectResults.Clear();
            _hasSearched = false;
            _searchPattern = "";
        }

        private List<string> ApplyFilters(List<string> ids)
        {
            var filtered = ids.AsEnumerable();
            
            // Apply name filter
            if (!string.IsNullOrEmpty(_nameFilter))
            {
                filtered = filtered.Where(id => id.Contains(_nameFilter, StringComparison.OrdinalIgnoreCase));
            }
            
            // Apply null/active filters
            filtered = filtered.Where(id =>
            {
                var obj = _trackerService.GetObject(id);
                
                if (obj == null)
                    return _showNullObjects;
                    
                if (_showActiveOnly && !obj.activeInHierarchy)
                    return false;
                    
                return true;
            });
            
            return filtered.ToList();
        }

        private void RemoveNullObjects()
        {
            var ids = _trackerService.GetIds();
            var nullIds = ids.Where(id => _trackerService.GetObject(id) == null).ToList();
            
            foreach (var id in nullIds)
            {
                _trackerService.UnregisterObject(id);
            }
        }

        private void ClearAllObjects()
        {
            var ids = _trackerService.GetIds().ToList();
            foreach (var id in ids)
            {
                _trackerService.UnregisterObject(id);
            }
        }
#endif
    }
}
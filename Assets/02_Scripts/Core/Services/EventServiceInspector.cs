using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace StrangeSpace
{
    public class EventServiceInspector : ZenEditorInspector
    {
        private EventService _eventService;
        private Vector2 _eventsScrollPosition;
        private Vector2 _lastEventsScrollPosition;
        
        private bool _showEvents = true;
        private bool _showLastEvents = true;
        private bool _showEventStats = true;
        private bool _showEventTypes = true;
        
        private EventService.EventType _filterType = EventService.EventType.Global;
        private bool _enableTypeFilter = false;
        private string _messageFilter = "";
        private bool _enableMessageFilter = false;
        
        // Statistics
        private Dictionary<EventService.EventType, int> _eventTypeCounts = new Dictionary<EventService.EventType, int>();

        public EventServiceInspector(EventService eventService)
        {
            _eventService = eventService;
            
            #if UNITY_EDITOR
                UpdateEventStatistics();
            #endif
        }

        public override void OnDrawGui()
        {
#if UNITY_EDITOR
            EditorGUILayout.Space(10);
            DrawHeader();
            EditorGUILayout.Space(5);
            
            DrawEventFilters();
            EditorGUILayout.Space(10);
            
            if (_showEventStats = EditorGUILayout.Foldout(_showEventStats, "Event Statistics", true))
            {
                DrawEventStatistics();
            }
            
            EditorGUILayout.Space(10);
            
            if (_showEvents = EditorGUILayout.Foldout(_showEvents, "All Events", true))
            {
                DrawAllEvents();
            }
            EditorGUILayout.Space(10);
            
            if (_showEventTypes = EditorGUILayout.Foldout(_showEventTypes, "Event Type Distribution", true))
            {
                DrawEventTypeDistribution();
            }
            EditorGUILayout.Space(10);
            
            DrawControls();
#endif
        }

#if UNITY_EDITOR
        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.LabelField("Event Service Inspector", headerStyle);

            EditorGUILayout.EndVertical();
        }

        private void DrawEventFilters()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            
            // Type filter
            EditorGUILayout.BeginHorizontal();
            _enableTypeFilter = EditorGUILayout.Toggle("Filter by Type:", _enableTypeFilter);
            GUI.enabled = _enableTypeFilter;
            _filterType = (EventService.EventType)EditorGUILayout.EnumPopup(_filterType);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            
            // Message filter
            EditorGUILayout.BeginHorizontal();
            _enableMessageFilter = EditorGUILayout.Toggle("Filter by Message:", _enableMessageFilter);
            GUI.enabled = _enableMessageFilter;
            _messageFilter = EditorGUILayout.TextField(_messageFilter);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void DrawEventStatistics()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            UpdateEventStatistics();
            
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Total Events:", _eventService.Events.Count.ToString());
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Events by Type:", EditorStyles.boldLabel);
            
            foreach (var kvp in _eventTypeCounts)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  {kvp.Key}:", kvp.Value.ToString());
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        
        private void DrawAllEvents()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            var filteredEvents = GetFilteredEvents(_eventService.Events);
            filteredEvents.Reverse();
            
            EditorGUILayout.LabelField($"Showing {filteredEvents.Count} of {_eventService.Events.Count} events");
            
            if (filteredEvents.Count > 0)
            {
                _eventsScrollPosition = EditorGUILayout.BeginScrollView(_eventsScrollPosition, GUILayout.Height(300));
                
                foreach (var evt in filteredEvents)
                {
                    DrawEventItem(evt);
                }
                
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.LabelField("No events match filter", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawEventTypeDistribution()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            UpdateEventStatistics();
            
            EditorGUI.indentLevel++;
            
            int totalEvents = _eventService.Events.Count;
            if (totalEvents > 0)
            {
                foreach (var kvp in _eventTypeCounts.OrderByDescending(x => x.Value))
                {
                    float percentage = (float)kvp.Value / totalEvents * 100f;
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{kvp.Key}:", GUILayout.Width(128));
                    EditorGUILayout.LabelField($"{kvp.Value} ({percentage:F1}%)", GUILayout.Width(80));
                    
                    // Simple progress bar
                    Rect rect = GUILayoutUtility.GetRect(0, 16, GUILayout.ExpandWidth(true));
                    EditorGUI.ProgressBar(rect, percentage / 100f, "");
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No events recorded", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void DrawEventItem(EventService.Event evt)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            
            // Event type with color
            Color originalColor = GUI.color;
            GUI.color = GetEventTypeColor(evt.Type);
            EditorGUILayout.LabelField($"[{evt.Type}]", GUILayout.Width(128));
            GUI.color = originalColor;
            
            EditorGUILayout.LabelField(evt.Message, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndHorizontal();
            
            if (evt.Data != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Data:", evt.Data.ToString(), EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawControls()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Clear All Events"))
            {
                RunAction(() => {
                    _eventService.Events.Clear();
                    UpdateEventStatistics();
                });
            }
            
            if (GUILayout.Button("Refresh Statistics"))
            {
                UpdateEventStatistics();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Test event generation
            EditorGUILayout.LabelField("Test Events:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Test Global"))
            {
                RunAction(() => _eventService.RegisteEvent(EventService.EventType.Global, "Test Global Event", "Test Data"));
            }
            
            if (GUILayout.Button("Test UI"))
            {
                RunAction(() => _eventService.RegisteEvent(EventService.EventType.UI, "Test UI Event", null));
            }
            
            if (GUILayout.Button("Test GameData"))
            {
                RunAction(() => _eventService.RegisteEvent(EventService.EventType.GameData, "Test GameData Event", DateTime.Now));
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private List<EventService.Event> GetFilteredEvents(List<EventService.Event> events)
        {
            var filtered = events.AsEnumerable();
            
            if (_enableTypeFilter)
            {
                filtered = filtered.Where(e => e.Type == _filterType);
            }
            
            if (_enableMessageFilter && !string.IsNullOrEmpty(_messageFilter))
            {
                filtered = filtered.Where(e => e.Message.Contains(_messageFilter, StringComparison.OrdinalIgnoreCase));
            }
            
            return filtered.ToList();
        }

        private void UpdateEventStatistics()
        {
            _eventTypeCounts.Clear();
            
            // Initialize with all event types
            foreach (EventService.EventType type in Enum.GetValues(typeof(EventService.EventType)))
            {
                _eventTypeCounts[type] = 0;
            }
            
            // Count events by type
            foreach (var evt in _eventService.Events)
            {
                _eventTypeCounts[evt.Type]++;
            }
        }

        private Color GetEventTypeColor(EventService.EventType type)
        {
            return type switch
            {
                EventService.EventType.Global => Color.white,
                EventService.EventType.GameLifeCycle => Color.green,
                EventService.EventType.GameData => Color.yellow,
                EventService.EventType.UI => Color.cyan,
                _ => Color.gray
            };
        }
#endif
    }
}
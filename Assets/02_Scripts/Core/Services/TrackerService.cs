using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zen.Debug;

namespace StrangeSpace
{
    public class TrackerService : PlainSingleton<TrackerService>, IEditorInspectorProvider
    {
        private Dictionary<string, GameObject> _trackedObjects = new();
        private Dictionary<GameObject, string> _trackedObjectIds = new();

        public int Count => _trackedObjects.Count;
        
        protected override void OnInitialize()
        {
            #if UNITY_EDITOR
                EditorDependencies.AddInspectorProvider(this);
            #endif
        }

        protected override void OnDeinitialize()
        {
            if (_trackedObjectIds.Count > 0)
            {
                ZenLog.Error("[TrackerService] Some objects have not been unregistered before deinitialization.");

                foreach (var trackedObject in _trackedObjects)
                {
                    ZenLog.Error("[TrackerService] Object: " + trackedObject.Key);
                }
            }

            Clear();
        }

        public void Clear()
        {
            _trackedObjects.Clear();
            _trackedObjectIds.Clear();
        }
        public GameObject GetObject(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            var wildCard = false;
            var searchId = id;
            
            if (id.EndsWith("*"))
            {
                wildCard = true;
                searchId = id.Substring(0, id.Length - 1);
            }

            if (wildCard)
            {
                foreach (var kv in _trackedObjects)
                {
                    if (kv.Key.Contains(searchId))
                    {
                        return kv.Value;
                    }
                }
            }
            else
            {
                if (_trackedObjects.TryGetValue(id, out GameObject obj))
                {
                    return obj;
                }
            }
            
            return null;
        }

        public List<GameObject> GetObjects(string pattern)
        {
            var results = new List<GameObject>();
            
            if (string.IsNullOrEmpty(pattern))
            {
                results.AddRange(_trackedObjects.Values);
                return results;
            }

            var wildCard = false;
            var searchPattern = pattern;
            
            if (pattern.EndsWith("*"))
            {
                wildCard = true;
                searchPattern = pattern.Substring(0, pattern.Length - 1);
            }

            if (wildCard)
            {
                foreach (var kv in _trackedObjects)
                {
                    if (kv.Key.Contains(searchPattern))
                    {
                        results.Add(kv.Value);
                    }
                }
            }
            else
            {
                if (_trackedObjects.TryGetValue(pattern, out GameObject obj))
                {
                    results.Add(obj);
                }
            }

            return results;
        }

        public List<string> GetIds(string pattern = null)
        {
            var results = new List<string>();
            
            if (string.IsNullOrEmpty(pattern))
            {
                results.AddRange(_trackedObjects.Keys);
                return results;
            }

            var wildCard = false;
            var searchPattern = pattern;
            
            if (pattern.EndsWith("*"))
            {
                wildCard = true;
                searchPattern = pattern.Substring(0, pattern.Length - 1);
            }

            if (wildCard)
            {
                foreach (var key in _trackedObjects.Keys)
                {
                    if (key.Contains(searchPattern))
                    {
                        results.Add(key);
                    }
                }
            }
            else
            {
                if (_trackedObjects.ContainsKey(pattern))
                {
                    results.Add(pattern);
                }
            }

            return results;
        }

        public bool HasObject(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;
                
            return _trackedObjects.ContainsKey(id);
        }
        
        public void RegisterObject(string id, GameObject obj)
        {
            if (obj == null || string.IsNullOrEmpty(id))
            {
                return;
            }
            
            // Handle case where object is already registered with different ID
            if (_trackedObjectIds.TryGetValue(obj, out string existingId))
            {
                if (existingId != id)
                {
                    // Remove old registration
                    _trackedObjects.Remove(existingId);
                }
            }
            
            // Handle case where ID is already used by different object
            if (_trackedObjects.TryGetValue(id, out GameObject existingObj))
            {
                if (existingObj != obj && existingObj != null)
                {
                    // Remove old object's reverse mapping
                    _trackedObjectIds.Remove(existingObj);
                }
            }
            
            _trackedObjects[id] = obj;
            _trackedObjectIds[obj] = id;
        }

        public void UnregisterObject(GameObject obj)
        {
            if (obj == null)
                return;
                
            if (_trackedObjectIds.TryGetValue(obj, out string id))
            {
                _trackedObjects.Remove(id);
                _trackedObjectIds.Remove(obj);
            }
        }

        public void UnregisterObject(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;
                
            if (_trackedObjects.TryGetValue(id, out GameObject obj))
            {
                _trackedObjects.Remove(id);
                _trackedObjectIds.Remove(obj);
            }
        }
        
        public InspectorInfo GetInspectorInfo()
        {
            return new InspectorInfo
            {
                Name = "Tracker Service",
                Description = "GameObject registry and lookup system",
            };
        }

        public IEditorInspector GetInspector()
        {
#if UNITY_EDITOR
            return new TrackerServiceInspector(this);
#endif
            return null;
        }
    }
}
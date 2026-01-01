using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace StrangeSpace
{
    [Serializable]
    public class AssetReferenceItem
    {
        public string Id;
        public AssetReference Reference;
    }

    public class SceneServiceRoot : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private bool _showLogs = true;
        
        [Header("Data")]
        [SerializeField]
        private List<AssetReferenceItem> _assetReferences = new List<AssetReferenceItem>();
        
        private Dictionary<string, AssetReference> _assetReferencesMap;
        
        // Get / Set
        public bool ShowLogs => _showLogs;
        private void Awake()
        {
            InitializeAssetReferencesMap();
        }
        
        private void InitializeAssetReferencesMap()
        {
            _assetReferencesMap = new Dictionary<string, AssetReference>();
            
            foreach (var item in _assetReferences)
            {
                if (!string.IsNullOrEmpty(item.Id) && item.Reference != null)
                {
                    _assetReferencesMap[item.Id] = item.Reference;
                }
            }
        }
        
        public AssetReference GetAssetReferenceById(string id)
        {
            if (_assetReferencesMap == null)
            {
                InitializeAssetReferencesMap();
            }
            
            if (_assetReferencesMap.TryGetValue(id, out AssetReference reference))
            {
                return reference;
            }
            
            return null;
        }
        
        public bool HasAssetReference(string id)
        {
            if (_assetReferencesMap == null)
            {
                InitializeAssetReferencesMap();
            }
            
            return _assetReferencesMap.ContainsKey(id);
        }
    }
}
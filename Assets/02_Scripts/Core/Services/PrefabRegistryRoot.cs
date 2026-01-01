using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace StrangeSpace
{
    public class PrefabRegistryRoot : MonoBehaviour
    {
        [Serializable]
        public class DirectEntry
        {
            public string Key;
            public GameObject Prefab;
        }
    
        [Serializable]
        public class AddressableEntry
        {
            public string Key;
            public AssetReferenceGameObject Reference;
        }
    
        [SerializeField] private List<DirectEntry> _directPrefabs = new();
        [SerializeField] private List<AddressableEntry> _addressablePrefabs = new();
    
        public IReadOnlyList<DirectEntry> DirectPrefabs => _directPrefabs;
        public IReadOnlyList<AddressableEntry> AddressablePrefabs => _addressablePrefabs;
    }
}
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;


namespace StrangeSpace
{
    public interface IPrefabSource
    {
        UniTask<GameObject> LoadAsync();
        GameObject LoadSync();
        bool SupportsSyncLoad { get; }
        void Release();
    }
    
    public class AddressablePrefabSource : IPrefabSource
    {
        private readonly AssetReferenceGameObject _reference;
        private GameObject _cached;
    
        public bool SupportsSyncLoad => _cached != null;
    
        public AddressablePrefabSource(AssetReferenceGameObject reference)
        {
            _reference = reference;
        }
    
        public GameObject LoadSync()
        {
            if (_cached == null)
                throw new InvalidOperationException($"Addressable prefab not preloaded. Call LoadAsync first.");
            return _cached;
        }
    
        public async UniTask<GameObject> LoadAsync()
        {
            if (_cached == null)
                _cached = await _reference.LoadAssetAsync();
            return _cached;
        }
    
        public void Release()
        {
            if (_cached != null)
            {
                _reference.ReleaseAsset();
                _cached = null;
            }
        }
    }
    
    public class DirectPrefabSource : IPrefabSource
    {
        private readonly GameObject _prefab;
    
        public bool SupportsSyncLoad => true;
    
        public DirectPrefabSource(GameObject prefab)
        {
            _prefab = prefab;
        }
    
        public GameObject LoadSync() => _prefab;
        public UniTask<GameObject> LoadAsync() => UniTask.FromResult(_prefab);
        public void Release() { }
    }
    
    public class PrefabRegistry : PlainSingleton<PrefabRegistry>
    {
        private PrefabRegistryRoot _root;
        private Dictionary<string, IPrefabSource> _sources = new();

        protected override void OnInitialize()
        {
            _root = Root.Get<PrefabRegistryRoot>();

            foreach (var entry in _root.DirectPrefabs)
                _sources[entry.Key] = new DirectPrefabSource(entry.Prefab);

            foreach (var entry in _root.AddressablePrefabs)
                _sources[entry.Key] = new AddressablePrefabSource(entry.Reference);
        }

        protected override void OnDeinitialize()
        {
            foreach (var source in _sources.Values)
                source.Release();

            _sources.Clear();
            _root = null;
        }

        public async UniTask<GameObject> LoadAsync(string key)
        {
            if (!_sources.TryGetValue(key, out var source))
                throw new KeyNotFoundException($"Prefab '{key}' not found in registry");
            return await source.LoadAsync();
        }

        public GameObject LoadSync(string key)
        {
            if (!_sources.TryGetValue(key, out var source))
                throw new KeyNotFoundException($"Prefab '{key}' not found in registry");
            if (!source.SupportsSyncLoad)
                throw new System.InvalidOperationException(
                    $"Prefab '{key}' is addressable and not preloaded. Use LoadAsync first.");
            return source.LoadSync();
        }

        public async UniTask<T> InstantiateAsync<T>(string key, Transform parent = null) where T : Component
        {
            var prefab = await LoadAsync(key);
            var instance = Object.Instantiate(prefab, parent);
            return instance.GetComponent<T>();
        }

        public async UniTask<T> InstantiateAsync<T>(string key, Vector3 position, Quaternion rotation,
            Transform parent = null) where T : Component
        {
            var prefab = await LoadAsync(key);
            var instance = Object.Instantiate(prefab, position, rotation, parent);
            return instance.GetComponent<T>();
        }

        public T InstantiateSync<T>(string key, Transform parent = null) where T : Component
        {
            var prefab = LoadSync(key);
            var instance = Object.Instantiate(prefab, parent);
            return instance.GetComponent<T>();
        }

        public T InstantiateSync<T>(string key, Vector3 position, Quaternion rotation, Transform parent = null)
            where T : Component
        {
            var prefab = LoadSync(key);
            var instance = Object.Instantiate(prefab, position, rotation, parent);
            return instance.GetComponent<T>();
        }

        public bool CanLoadSync(string key)
        {
            return _sources.TryGetValue(key, out var source) && source.SupportsSyncLoad;
        }

        public bool Contains(string key) => _sources.ContainsKey(key);

        public void Release(string key)
        {
            if (_sources.TryGetValue(key, out var source))
                source.Release();
        }
    }
}
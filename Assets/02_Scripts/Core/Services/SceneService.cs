using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zen.Core;
using Zen.Debug;

namespace StrangeSpace
{
    public interface ISceneService
    {
        UniTask<GameObject> LoadPrefabAsync(string addressableKey, Vector3 position = default, Quaternion rotation = default);
        UniTask<GameObject> LoadPrefabAsync(AssetReference assetReference, Vector3 position = default, Quaternion rotation = default);
        
        bool ReleasePrefab(GameObject prefabInstance);
        void ReleaseAllPrefabs();
        
        void Initialize();
        void Deinitialize();
        
        void BindToRoot(SingletonRoot singletonRoot);
    }

    public class SceneService : ISceneService
    {
        private static SceneService _instance;
        private SceneServiceRoot _rootObject;
        private Dictionary<int, AsyncOperationHandle<GameObject>> _activeHandles = new Dictionary<int, AsyncOperationHandle<GameObject>>();

        private bool _showLogs;
        
        // Get / Set
        public SceneServiceRoot RootObject => _rootObject;
        
        public void Initialize()
        {
            
        }

        public void Deinitialize()
        {
            ReleaseAllPrefabs();
        }
        
        public void BindToRoot(SingletonRoot singletonRoot)
        {
            _rootObject = singletonRoot.Get<SceneServiceRoot>();
            
            _showLogs = _rootObject.ShowLogs;
        }
        
        public async UniTask<GameObject> LoadPrefabAsync(string addressableKey, Vector3 position = default, Quaternion rotation = default)
        {
            var watch = new Watch();
            watch.Reset();
            
            try
            {
                AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(
                    addressableKey,
                    position,
                    rotation,
                    _rootObject.transform
                );

                await handle.ToUniTask();

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[SceneService]: Failed to load prefab: {addressableKey}");
                    Addressables.Release(handle);
                    return null;
                }

                GameObject prefabInstance = handle.Result;

                _activeHandles[prefabInstance.GetInstanceID()] = handle;

                if (_showLogs)
                {
                    ZenLog.Info($"[SceneService]: Loaded prefab: {addressableKey} in {watch.Time()} seconds]");
                }
                
                return prefabInstance;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        public async UniTask<GameObject> LoadPrefabAsync(AssetReference assetReference, Vector3 position = default, Quaternion rotation = default)
        {
            var watch = new Watch();
            watch.Reset();
            
            try
            {
                AsyncOperationHandle<GameObject> handle = assetReference.InstantiateAsync(
                    position,
                    rotation,
                    _rootObject.transform
                );

                await handle.ToUniTask();

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"Failed to load prefab from AssetReference");
                    Addressables.Release(handle);
                    return null;
                }

                GameObject prefabInstance = handle.Result;

                _activeHandles[prefabInstance.GetInstanceID()] = handle;

                if (_showLogs)
                {
                    ZenLog.Info($"[SceneService]: Loaded prefab: {assetReference.Asset} in {watch.Time()} seconds]");
                }

                
                return prefabInstance;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        public bool ReleasePrefab(GameObject prefabInstance)
        {
            if (prefabInstance == null)
                return false;

            int instanceId = prefabInstance.GetInstanceID();

            if (_activeHandles.TryGetValue(instanceId, out AsyncOperationHandle<GameObject> handle))
            {
                Addressables.ReleaseInstance(handle);
                _activeHandles.Remove(instanceId);
                return true;
            }

            Debug.LogWarning($"Attempted to release a prefab that wasn't loaded through SceneService: {prefabInstance.name}");
            return false;
        }

        public void ReleaseAllPrefabs()
        {
            foreach (var handle in _activeHandles.Values)
            {
                Addressables.ReleaseInstance(handle);
            }

            _activeHandles.Clear();
        }
    }
}
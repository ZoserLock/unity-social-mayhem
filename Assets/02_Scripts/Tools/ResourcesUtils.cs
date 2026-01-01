using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EAsset
{
    Undefined,
    Asset,
    Prefab
}

// [AKP] Some parts of this class was commented due to missing dependencies.
public static class ResourcesUtils
{
    /// <param name="assetType"> Set to TRUE when searching for interfaces that are implemented by components. </param>
    //public static T FindIdentificableAssetByType<T>(string id, string folder = null, EAsset assetType = EAsset.Undefined, bool verbose = true, params string[] foldersToSearch) where T : class, IIdentifiable
    //{
    //    var assets = FindAssetsByType<T>(assetType, verbose, foldersToSearch);
    //    return assets.Find(x => x.ID == id);
    //}

    /// <param name="forceIsPrefab"> Set to TRUE when searching for interfaces that are implemented by components. </param>
    public static T FindAssetByType<T>(EAsset assetType = EAsset.Undefined, bool verbose = true, params string[] foldersToSearch) where T : class
    {
        var assets = FindAssetsByType<T>(assetType, verbose, foldersToSearch);
        if (assets.Count > 0)
        {
            if (verbose && assets.Count > 1)
            {
                Debug.LogWarning($"FindAssetByType multiple assets found: {typeof(T).Name}, AssetType: {assetType}");
                //Debug.LogWarning($"FindAssetByType multiple assets found: {typeof(T).Name}, AssetType: {assetType}, FolderRoots: {foldersToSearch.AsString(", ")}");
            }
            return assets[0];
        }
        return null;
    }

    /// <param name="forceIsPrefab"> Set to TRUE when searching for interfaces that are implemented by components. </param>
    public static List<T> FindAssetsByType<T>(EAsset assetType = EAsset.Undefined, bool verbose = true, params string[] foldersToSearch) where T : class
    {
        return Internal_FindAssetsByType<T>(assetType, verbose, foldersToSearch);
    }

    private static List<T> Internal_FindAssetsByType<T>(EAsset assetType, bool verbose, params string[] foldersToSearch) where T : class
    {
        if (foldersToSearch == null)
        {
            foldersToSearch = new string[0];
        }

        var values = new List<T>();
#if UNITY_EDITOR
        if (assetType == EAsset.Undefined)
        {
            bool isPrefab = typeof(MonoBehaviour).IsAssignableFrom(typeof(T));
            assetType = isPrefab ? EAsset.Prefab : EAsset.Asset;
        }
        if (assetType == EAsset.Prefab)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", foldersToSearch);
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null)
                {
                    continue;
                }
                T[] components = prefab.GetComponents<T>();
                values.AddRange(components);
            }
        }
        else if (assetType == EAsset.Asset)
        {
            string filter = string.Format("t:{0}", typeof(T).Name);
            string[] guids = AssetDatabase.FindAssets(filter, foldersToSearch);
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
                if (asset != null)
                {
                    values.Add(asset);
                }
            }
        }
        else
        {
            Debug.LogError("AssetType is not being handled: " + assetType);
        }

#endif
        if (verbose && values.Count == 0)
        {
            Debug.LogError($"Asset was not found: {typeof(T).Name}, AssetType: {assetType}");
            //Debug.LogError($"Asset was not found: {typeof(T).Name}, AssetType: {assetType}, Folders: {foldersToSearch.AsString(", ")}"); 
        }
        return values;
    }

    //        public static List<string> FindUniqueAssetsByType<T>() where T : class, IIdentifiable
    //        {
    //            HashSet<string> uniqueValues = new HashSet<string>();
    //#if UNITY_EDITOR
    //            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
    //            for (int i = 0; i < guids.Length; i++)
    //            {
    //                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
    //                T asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
    //                if (asset != null)
    //                {
    //                    uniqueValues.Add(asset.ID);
    //                }
    //            }
    //#endif
    //            return new List<string>(uniqueValues);
    //        }

    //        public static List<string> FindUniqueAssetsByType(Type _type)
    //        {
    //            HashSet<string> uniqueValues = new HashSet<string>();
    //#if UNITY_EDITOR
    //            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", _type));
    //            for (int i = 0; i < guids.Length; i++)
    //            {
    //                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
    //                var asset = AssetDatabase.LoadAssetAtPath(assetPath, _type) as IIdentifiable;
    //                if (asset != null)
    //                {
    //                    uniqueValues.Add(asset.ID);
    //                }
    //            }
    //#endif
    //            return new List<string>(uniqueValues);
    //        }
}
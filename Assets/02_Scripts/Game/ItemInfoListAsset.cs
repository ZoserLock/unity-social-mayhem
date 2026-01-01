using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StrangeSpace
{
    [CreateAssetMenu(fileName = "ItemInfoListAsset", menuName = "Strange Space/ItemInfoListAsset")]
    public class ItemInfoListAsset : ScriptableObject
    {
        [SerializeField]
        public List<ItemInfo> List = new List<ItemInfo>();
        
#if UNITY_EDITOR
        [ContextMenu("Get ItemInfos From Folder")]
        private void GetItemInfosFromFolder()
        {
            List.Clear();
            
            string assetPath = AssetDatabase.GetAssetPath(this);
            string folderPath = Path.GetDirectoryName(assetPath);
            
            string[] guids = AssetDatabase.FindAssets("t:ItemInfo", new[] { folderPath });
            
            foreach (string guid in guids)
            {
                string itemPath = AssetDatabase.GUIDToAssetPath(guid);
                ItemInfo item = AssetDatabase.LoadAssetAtPath<ItemInfo>(itemPath);
                
                if (item != null)
                {
                    List.Add(item);
                }
            }
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Added {List.Count} ItemInfo assets from folder: {folderPath}");
        }
#endif
    }
}

using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;

namespace StrangeSpace
{
    public class FolderOpenerMenu : MonoBehaviour
    {
        [MenuItem("Strange/Open Folders/Persistent Data Path")]
        private static void OpenPersistentDataPath()
        {
            OpenFolder(Application.persistentDataPath);
        }

        [MenuItem("Strange/Open Folders/Data Path")]
        private static void OpenDataPath()
        {
            OpenFolder(Application.dataPath);
        }

        [MenuItem("Strange/Open Folders/Temporary Cache Path")]
        private static void OpenTemporaryCachePath()
        {
            OpenFolder(Application.temporaryCachePath);
        }

        [MenuItem("Strange/Open Folders/Streaming Assets Path")]
        private static void OpenStreamingAssetsPath()
        {
            OpenFolder(Application.streamingAssetsPath);
        }

        private static void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                // Create the folder if it doesn't exist
                Directory.CreateDirectory(folderPath);

                // Open the folder
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    Process.Start("explorer.exe", folderPath.Replace("/", "\\"));
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    Process.Start("open", folderPath);
                }
                else if (Application.platform == RuntimePlatform.LinuxEditor)
                {
                    Process.Start("xdg-open", folderPath);
                }

                UnityEngine.Debug.Log("Opening folder: " + folderPath);
            }
            else
            {
                UnityEngine.Debug.LogError("Folder does not exist: " + folderPath);
            }
        }
        
         [MenuItem("Strange/Save System/Open Save Location")]
        public static void OpenSaveLocation()
        {
            var path = Application.persistentDataPath;
            
            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Save directory does not exist.", "OK");
            }
        }
        
        [MenuItem("Strange/Save System/Delete All Saves")]
        public static void DeleteAllSaves()
        {
            var path = Application.persistentDataPath;
            var confirmDelete = EditorUtility.DisplayDialog(
                "Confirm Delete",
                "Are you sure you want to delete ALL save files?",
                "Yes", "No");
                
            if (confirmDelete)
            {
                var jsonFiles = Directory.GetFiles(path, "*_save.json");
                var binFiles = Directory.GetFiles(path, "*_save.bin");
                var debugBinFiles = Directory.GetFiles(path, "*_save.bin.d.json");
                
                var deletedCount = 0;
                
                foreach (var file in jsonFiles)
                {
                    try
                    {
                        File.Delete(file);
                        deletedCount++;
                    }
                    catch (System.Exception) { }
                }
                
                foreach (var file in binFiles)
                {
                    try
                    {
                        File.Delete(file);
                        deletedCount++;
                    }
                    catch (System.Exception) { }
                }
                
                foreach (var file in debugBinFiles)
                {
                    try
                    {
                        File.Delete(file);
                        deletedCount++;
                    }
                    catch (System.Exception) { }
                }
                
                EditorUtility.DisplayDialog("Success", $"Deleted {deletedCount} save files.", "OK");
            }
        }
        
        [MenuItem("Strange/Save System/Delete Default Save")]
        public static void DeleteDefaultSave()
        {
            var path = Application.persistentDataPath;
            var defaultSlot = "default_slot";
            
            var jsonPath = Path.Combine(path, $"{defaultSlot}_save.json");
            var binPath = Path.Combine(path, $"{defaultSlot}_save.bin");
            var debugBinPath = Path.Combine(path, $"{defaultSlot}_save.bin.d.json");
            
            var confirmDelete = EditorUtility.DisplayDialog(
                "Confirm Delete",
                "Are you sure you want to delete the default save files?",
                "Yes", "No");
                
            if (confirmDelete)
            {
                var deletedCount = 0;
                
                if (File.Exists(jsonPath))
                {
                    try
                    {
                        File.Delete(jsonPath);
                        deletedCount++;
                    }
                    catch (System.Exception) { }
                }
                
                if (File.Exists(binPath))
                {
                    try
                    {
                        File.Delete(binPath);
                        deletedCount++;
                    }
                    catch (System.Exception) { }
                }
                
                if (File.Exists(debugBinPath))
                {
                    try
                    {
                        File.Delete(debugBinPath);
                        deletedCount++;
                    }
                    catch (System.Exception) { }
                }
                
                EditorUtility.DisplayDialog("Success", $"Deleted {deletedCount} default save files.", "OK");
            }
        }
    }
}
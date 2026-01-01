using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using Newtonsoft.Json;
using UnityEditor;
#endif

using UnityEngine;

namespace StrangeSpace
{
    public class SaveManagerInspector : ZenEditorInspector
    {
        private SaveManager _saveManager;
        private SaveManagerSettings _settings;
        private Vector2 _jsonScrollPosition;
        
        private string _currentJsonData;
        
        private bool _showJson = true;
        private bool _showCurrentData = true;
        private bool _showDebugInfo = true;
        private bool _showSaveSlots = true;
        
        // Reflection field info cache
        private System.Reflection.FieldInfo _saveSlotNameField;
        private System.Reflection.FieldInfo _currentDataField;
        private System.Reflection.FieldInfo _dataPathField;
        private System.Reflection.FieldInfo _settingsField;

        public SaveManagerInspector(SaveManager saveManager)
        {
            _saveManager = saveManager;
            
            // Cache reflection fields for better performance
            var type = typeof(SaveManager);
            _saveSlotNameField = type.GetField("_saveSlotName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            _currentDataField = type.GetField("_currentData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            _dataPathField = type.GetField("_dataPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            _settingsField = type.GetField("_settings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (_settingsField != null)
            {
                _settings = _settingsField.GetValue(_saveManager) as SaveManagerSettings;
            }
        }

        public override void OnDrawGui()
        {
#if UNITY_EDITOR
            EditorGUILayout.Space(10);
            DrawHeader();
            EditorGUILayout.Space(5);
            
            DrawCurrentSaveSlot();
            EditorGUILayout.Space(10);
            
            if (_showCurrentData = EditorGUILayout.Foldout(_showCurrentData, "Current Save Data", true))
            {
                DrawCurrentSaveData();
            }
            EditorGUILayout.Space(10);
            
            if (_showJson = EditorGUILayout.Foldout(_showJson, "Current Json", true))
            {
                DrawCurrentSaveJson ();
            }
            EditorGUILayout.Space(10);
            
            
            if (_showSaveSlots = EditorGUILayout.Foldout(_showSaveSlots, "Save Slots", true))
            {
                DrawSaveSlots();
            }
            EditorGUILayout.Space(10);
            
            if (_showDebugInfo = EditorGUILayout.Foldout(_showDebugInfo, "Debug Information", true))
            {
                DrawDebugInfo();
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

            EditorGUILayout.LabelField("Save Manager Inspector", headerStyle);

            EditorGUILayout.EndVertical();
        }

        private void DrawCurrentSaveSlot()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Current Save Slot", EditorStyles.boldLabel);
            
            string currentSlot = GetCurrentSaveSlot();
            
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Active Slot:", currentSlot);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawCurrentSaveData()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            var currentData = _currentDataField?.GetValue(_saveManager) as SaveData;
            
            if (currentData != null)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Version:", currentData.Version.ToString());
                EditorGUILayout.LabelField("Save Count:", currentData.SaveCount.ToString());
                
                if (currentData.GameData != null)
                {
                    EditorGUILayout.LabelField("Game Data:", "Present");
                    
                    // You could expand this to show more details about GameData
                    // based on what properties it contains
                }
                else
                {
                    EditorGUILayout.LabelField("Game Data:", "None");
                }
                
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("No save data loaded", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawSaveSlots()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            string dataPath = _dataPathField?.GetValue(_saveManager) as string;
            
            if (!string.IsNullOrEmpty(dataPath) && Directory.Exists(dataPath))
            {
                var saveFiles = Directory.GetFiles(dataPath, "*.json")
                    .Concat(Directory.GetFiles(dataPath, "*.bin"))
                    .ToList();
                
                if (saveFiles.Count > 0)
                {
                    EditorGUILayout.LabelField($"Found {saveFiles.Count} save files:");
                    
                    EditorGUI.indentLevel++;
                    foreach (var file in saveFiles)
                    {
                        string fileName = Path.GetFileName(file);
                        
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(fileName);
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.LabelField("No save files found", EditorStyles.centeredGreyMiniLabel);
                }
            }
            else
            {
                EditorGUILayout.LabelField("Save directory not available", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawDebugInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            string dataPath = _dataPathField?.GetValue(_saveManager) as string;
            
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Data Path:", dataPath ?? "Not set",GUILayout.MinWidth(500));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open", GUILayout.Width(60)))
            {
                EditorUtility.RevealInFinder(dataPath);
            }
            EditorGUILayout.EndHorizontal();
            
            string currentSlot = GetCurrentSaveSlot();
            if (_settings != null)
            {
                string expectedFile = _settings.Encrypt ? 
                    $"{currentSlot}_save.bin" : 
                    $"{currentSlot}_save.json";
                    
                EditorGUILayout.LabelField("Expected Save File:", expectedFile);
                
                if (!string.IsNullOrEmpty(dataPath))
                {
                    string fullPath = Path.Combine(dataPath, expectedFile);
                    bool exists = File.Exists(fullPath);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("File Exists:", exists ? "Yes" : "No");
                    
                    if (exists)
                    {
                        var fileInfo = new FileInfo(fullPath);
                        EditorGUILayout.LabelField($"Size: {fileInfo.Length} bytes");
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUI.indentLevel--;
            
            EditorGUILayout.EndVertical();
        }
        

        private void DrawCurrentSaveJson()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Current Save JSON", EditorStyles.boldLabel);
            if(GUILayout.Button("Update Data", GUILayout.Width(100)))
            {
                RunAction(() =>
                {
                    var data = _saveManager.CurrentData;

                    _currentJsonData = JsonConvert.SerializeObject(data, JsonSettings.Settings);
                    
                    /*EditorDependencies.ZenTaskManager.RunModalTask(new LambdaTask(async () =>
                    {
                        var gameData = new GameData();

                        //await EditorDependencies.Application.SendEditorCommand("SaveWorld");
                            
                        // TODO: Collect Data
                        await _saveManager.SaveGameDataAsync(gameData);
                        
                        var data = _saveManager.CurrentData;

                        _currentJsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                        
                        /*
                        //await _application.SendCommand("Save");
                        
                        try
                        {
                            var data = _saveManager.CurrentData;

                            _currentJsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                        }
                        catch (Exception e)
                        {
                            _currentJsonData = "Unable to deserialize save data";
                        }
                        
                        return ETaskResult.Success; 
                    }));*/
                });
            }
            EditorGUILayout.EndHorizontal();
            var textAreaStyle = new GUIStyle(EditorStyles.textArea);
            textAreaStyle.wordWrap = true;
            textAreaStyle.stretchHeight = true;

            _jsonScrollPosition = EditorGUILayout.BeginScrollView(_jsonScrollPosition, GUILayout.Height(500));
            GUILayout.TextArea(_currentJsonData, textAreaStyle);
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
        }

        private string GetCurrentSaveSlot()
        {
            string slotName = _saveSlotNameField?.GetValue(_saveManager) as string;
            if (string.IsNullOrEmpty(slotName) && _settings != null)
            {
                return _settings.DefaultSaveSlot;
            }
            return slotName ?? "Unknown";
        }
#endif
    }
}
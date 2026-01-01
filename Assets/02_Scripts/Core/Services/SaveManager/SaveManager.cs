using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Zen.Debug;


namespace StrangeSpace
{

    public interface ISaveManager
    {
        void UseSaveSlot(string name);
        
        void NewGameData();
        
        UniTask<bool> SaveGameDataAsync(GameData data);
        UniTask<GameData> LoadGameDataAsync();
        
        void Initialize();
        void Deinitialize();
        
        void BindToRoot(SingletonRoot singletonRoot);
    }

    [System.Serializable]
    public class SaveManagerSettings
    {
        public bool Encrypt = false;
        public string DefaultSaveSlot = "default_slot";
        
        [Header("Debug")]
        public bool ForceLoadFail = false;
        public bool ForceSaveFail = false;
        public bool ForceRepairFailed = false;
        public bool ForceLoadCorruptData = false;
    }
    
    public class SaveManager : ISaveManager, IEditorInspectorProvider
    {
        public const string CORRUPT_DATA = "\"4{\u20acikofj sañ3kl5254\"";
        
        private const int kVersionNumber = 1;
        private const string kEncryptionKey = "55QK6qosg3Say6rs!95z!!nnQspZzF@$G^pSZh*q";
        
        private const string sFilenameText = "save.json";
        private const string sFilenameBinDebug = "save.bin.d.json";
        private const string sFilenameBin = "save.bin";
        
        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly Rijndael _cryptoAlgorithm = new();
        private readonly IEventService _eventService;

        private SaveManagerRoot _saveManagerRoot;
        private SaveManagerSettings _settings;
        
        private string _saveSlotName;
        private string _dataPath;
        
        private SaveData _currentData;

        public SaveData CurrentData => _currentData;
        
        public SaveManager(IFileSystemProvider fileSystemProvider, IEventService eventService)
        {
            _fileSystemProvider = fileSystemProvider;
            _eventService = eventService;
        }

  
        public void Initialize()
        {
            _dataPath = Application.persistentDataPath;
            
            #if UNITY_EDITOR
                EditorDependencies.AddInspectorProvider(this);
            #endif
        }

        public void Deinitialize()
        {
            
        }

        public void BindToRoot(SingletonRoot singletonRoot)
        {
            _saveManagerRoot = singletonRoot.Get<SaveManagerRoot>();

            _settings = _saveManagerRoot.Settings;
        }

        public void UseSaveSlot(string name)
        {
            _saveSlotName = name;
            
            _eventService.RegisteEvent(EventService.EventType.GameLifeCycle, $"Save Slot Changed to [{GetSlotName()}]");
        }

        public void NewGameData()
        {
            _currentData = new SaveData();
            _currentData.Version = kVersionNumber;
            
            _eventService.RegisteEvent(EventService.EventType.GameLifeCycle, $"[Save Manager] -> Created new game save data for slot [{GetSlotName()}]");
        }
        
        public async UniTask<bool> SaveGameDataAsync(GameData gameData)
        {
            #if UNITY_EDITOR
                if (_saveManagerRoot.Settings.ForceSaveFail)
                {
                    return false;
                }
            #endif
            
            bool saveSuccess = true;
            
            _currentData.SaveCount++;
            _currentData.GameData = gameData;

            if (_settings.Encrypt)
            {
                string jsonData = JsonConvert.SerializeObject(_currentData, JsonSettings.Settings);
                byte[] encryptedBytes = _cryptoAlgorithm.Encrypt(jsonData, kEncryptionKey);
                
                var success = await _fileSystemProvider.WriteAllBytesAsync(GetBinFilename(), encryptedBytes);
                saveSuccess &= success;
                
#if UNITY_EDITOR
                success = await _fileSystemProvider.WriteAllTextAsync(GetBinDebugFilename(), jsonData);
                saveSuccess &= success;
#endif
            }
            else
            {
                string jsonData = JsonConvert.SerializeObject(_currentData, JsonSettings.Settings);
                
                var success = await  _fileSystemProvider.WriteAllTextAsync(GetTextFilename(), jsonData);
                saveSuccess &= success;
                
                _eventService.RegisteEvent(EventService.EventType.GameData, "[SaveGameData Json]", jsonData);
            }
            
            _eventService.RegisteEvent(EventService.EventType.GameLifeCycle, $"[Save Manager] -> Saved game data for slot [{GetSlotName()}]");

            return saveSuccess;
        }

        public async UniTask<GameData> LoadGameDataAsync()
        {
            await UniTask.WaitForEndOfFrame();

            #if UNITY_EDITOR
            if (_saveManagerRoot.Settings.ForceLoadFail)
            {
                return null;
            }
            #endif
            
            string jsonData = null;
            
            if (_settings.Encrypt)
            {
                var bytes = await _fileSystemProvider.ReadAllBytesAsync(GetBinFilename());
                    
                if(bytes != null)
                {
                    jsonData = _cryptoAlgorithm.Decrypt(bytes, kEncryptionKey);
                    
                    if (jsonData == null)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                jsonData = await _fileSystemProvider.ReadAllTextAsync(GetTextFilename());
                
                if (jsonData == null)
                {
                    return null;
                }
            }
            
            #if UNITY_EDITOR
            if (_saveManagerRoot.Settings.ForceLoadCorruptData)
            {
                jsonData = CORRUPT_DATA;
            }
            #endif

            try
            {
                _currentData = JsonConvert.DeserializeObject<SaveData>(jsonData, JsonSettings.Settings);
                _currentData.LoadCount++;
            }
            catch (Exception e)
            {
                return null;
            }
    
            
            int version = _currentData.Version;

            bool repairSuccessfull = true;
            if (version != kVersionNumber)
            {
                repairSuccessfull = RepairSave();
            }

            if (!repairSuccessfull)
            {
                Debug.LogError("[Save Manager] -> Load Failed: System was not able to repair the save!");
                
                _eventService.RegisteEvent(EventService.EventType.GameLifeCycle, $"[Save Manager] -> Load Failed. Unable to repair save for slot [{GetSlotName()}]");
                
                // TODO: Do something? replace save? copy old save and create new? abort load?

                return null;
            }
            
            _eventService.RegisteEvent(EventService.EventType.GameLifeCycle, $"[Save Manager] -> Loaded game data for slot [{GetSlotName()}]");
            
            return _currentData.GameData;
        }
        
        private bool RepairSave()
        {
            #if UNITY_EDITOR
            if (_saveManagerRoot.Settings.ForceRepairFailed)
            {
                return false;
            }
            #endif
            
            if (_currentData.Version == 1)
            {
                // Make changes to version 2
                _currentData.Version = 1;
            }

            if (_currentData.Version == 2)
            {
                // make changes to version 3 and do it this wait until the current version in restored.
                _currentData.Version = 2;
            }

            if (_currentData.Version != kVersionNumber)
            {
                Debug.LogError("Save Repair Failed! for version: " + _currentData.Version);
                return false;
            }

            return true;
        }
        
        private string GetSlotName()
        {
            if (!string.IsNullOrEmpty(_saveSlotName))
            {
                return _saveSlotName;
            }
            
            return _settings.DefaultSaveSlot;
        }
        
        private string GetBinFilename()
        {
            return Path.Combine(_dataPath, $"{GetSlotName()}_{sFilenameBin}");;
        }
        
        private string GetBinDebugFilename()
        {
            return Path.Combine(_dataPath, $"{GetSlotName()}_{sFilenameBinDebug}");
        }
        
        private string GetTextFilename()
        {
            return Path.Combine(_dataPath, $"{GetSlotName()}_{sFilenameText}");
        }

        public InspectorInfo GetInspectorInfo()
        {
            return new InspectorInfo
            {
                Name = "Save Manager",
                Description = "Saves and Load the game",
            };
        }

        public IEditorInspector GetInspector()
        {
            return new SaveManagerInspector(this);
        }
    }
}
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zen.Core;
using Zen.Debug;

namespace StrangeSpace
{
    public class GameplayStateData
    {
        public bool IsNewGame { get; set; }
        public GameData GameData { get; set; }
    }
    
    
    public class GameplayState : GameState, IEditorInspectorProvider
    {
        // Dependencies
        private readonly ZenApplication _zenApplication;
        private readonly IZenTaskManager _taskManager;
        private readonly IGameTaskManager _gameTaskManager;
        private readonly ISceneService _sceneService;
        private readonly ITimeManager _timeManager;
        private readonly ISaveManager _saveManager;
        
        private readonly GameDatabase _gameDatabase;
        private readonly UIManager _uiManager;
        private readonly UIOverlay _uiOverlay;
        
        // State Objects
        
        private Camera _applicationCamera;

        private GameplayStateData _enterData;
        
        private GameObject _sceneRoot;

        private GameCameraController _gameCamera;
        
        private GameDirector _gameDirector;



        public GameplayState(ZenApplication zenApplication, 
                             IZenTaskManager taskManager,
                             ISceneService sceneService,
                             IGameTaskManager gameTaskManager,
                             ITimeManager timeManager,
                             ISaveManager saveManager,
                             GameDatabase gameDatabase,
                             UIManager uiManager,
                             UIOverlay uiOverlay)
        {
            _zenApplication = zenApplication;
            _taskManager = taskManager;
            _sceneService = sceneService;
            _gameTaskManager = gameTaskManager;
            _timeManager = timeManager;
            _saveManager = saveManager;
            _gameDatabase = gameDatabase;

            _uiManager = uiManager;
            _uiOverlay = uiOverlay;
        }

        protected override void OnRegister()
        {
            #if UNITY_EDITOR
                EditorDependencies.AddInspectorProvider(this);
            #endif
        }

        protected override async UniTask OnEnter(object data)
        {
       
            ZenLog.Info(LogCategory.System, "[GamePlayState]: OnEnter");
            
            _enterData = data as GameplayStateData;

            if (_enterData == null)
            {
                ZenLog.Error(LogCategory.System, "[GamePlayState]: OnEnter: Enter GamePlayStateData is null");
                return;
            }

            _applicationCamera = _zenApplication.ApplicationCamera;
                
            // Clear currently running game tasks to start fresh.
            _gameTaskManager.Clear();
            
            // Load Unity Objects
            Watch.Tik();
            ZenLog.Info(LogCategory.System, "[GamePlayState]: Loading GameplayRoot");
            _sceneRoot = await _sceneService.LoadPrefabAsync("Prefabs/GameplayRoot");
            ZenLog.Info(LogCategory.System, $"[GamePlayState]: GameplayRoot Loaded In: {Watch.Tok()} s");
            
            // Disable support camera since this state have a camera.
            _applicationCamera.gameObject.SetActive(false);
            
            var directorRoot = _sceneRoot.GetComponent<GameDirectorRoot>();

            // Setup Director
            _gameDirector = new GameDirector(_gameTaskManager, 
                                             _timeManager, 
                                             _gameDatabase, 
                                             directorRoot);

            _gameCamera = _gameDirector.GameCamera;
            
            // Register input event
            SetupInput();
            
            // Register scheduled Actions
            AutoSchedule(TimeSpan.FromSeconds(5), SaveWorldScheduled);
            
            if(_enterData.IsNewGame)
            {
                await CreateNewGame();
            }
            else
            {
                if (_enterData.GameData == null)
                {
                    ZenLog.Error("[GamePlayState]: Gameplay marked as load game but game data is null");
                }
                
                await LoadGameFromData(_enterData.GameData);
            }
        }
        
        protected override UniTask OnLeave()
        {
            ClearInput();
            
            if (_sceneRoot != null)
            {
                _sceneService.ReleasePrefab(_sceneRoot);
                _sceneRoot = null;
            }
            
            ZenLog.Info(LogCategory.System, "[GamePlayState] -> OnLeave");
            
            return UniTask.CompletedTask;
        }
        
        private void SetupInput()
        {
            InputManager.Instance.OnPinchZoomEvent += OnPinchZoom;
        }

        private void ClearInput()
        {
            InputManager.Instance.OnPinchZoomEvent -= OnPinchZoom;
        }

        private async UniTask CreateNewGame()
        {
            _gameDirector.InitializeAsNew();
            _timeManager.InitializeAsNew();
            // Create new gameplay

            // Load Scene GamePlay
            // Hold the scene reference

            // GameObject.InstantiateAsync();
        }
        
        private async Task LoadGameFromData(GameData data)
        {
            // Initialize Game
            _gameDirector.InitializeWithData(data.GameDirector);
            _timeManager.InitializeWithData(data.TimeManager);
         
            // TODO: Data and other stuff.
        }

        private void CleanupGameplayState()
        {
            // Unload Scene GamePlay
        }
        
        protected override void OnUpdate(float deltaTime)
        {
            // Update GamePlay
            _gameTaskManager.Tick();
            _gameDirector.Tick(deltaTime);
        }
        
        private void OnPinchZoom(float zoom)
        {
            _gameCamera.ApplyZoom(zoom);
            ZenLog.Info(LogCategory.Gameplay, $"[GamePlayState] -> Normalized Zoom {_gameCamera.NormalizedCurrentDepth}");
        }

        private GameData GetSaveData()
        {
            var data = new GameData();
            
            data.TimeManager = _timeManager.GetSaveData();
            data.GameDirector = _gameDirector.GetSaveData();

            // TODO: here we could deep copy via serialization.
            // TODO: Like -> To Json From Json.
            
            return data;
        }

        private void SaveWorldScheduled()
        {
            ZenLog.Info(LogCategory.Gameplay, $"[GamePlayState] -> Saving Game World");
            
            var gameData = GetSaveData();
            
            _taskManager.RunModalTask(new SaveGameTask(_saveManager, gameData));
        }

        public InspectorInfo GetInspectorInfo()
        {
            return new InspectorInfo()
            {
                Name = "Gameplay State",
                Priority = 100,
            };
        }

        public IEditorInspector GetInspector()
        {
            return new GamePlayStateInspector(this);
        }
    }
}
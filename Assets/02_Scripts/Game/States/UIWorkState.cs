using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zen.Core;
using Zen.Debug;

namespace StrangeSpace
{
    public class UIWorkState : GameState
    {
        // Dependencies
        private readonly ZenApplication _zenApplication;
        private readonly IGameTaskManager _gameTaskManager;
        private readonly ISceneService _sceneService;
        
        private readonly UIManager _uiManager;
        private readonly UIOverlay _uiOverlay;

        // State Objects
        private Camera _applicationCamera;
        
        private GameplayStateData _enterData;
        
        private GameObject _sceneRoot;

        private UIWorkRoot _root;
  

        public UIWorkState(ZenApplication zenApplication, 
                           ISceneService sceneService,
                           IGameTaskManager gameTaskManager,
                           UIManager uiManager,
                           UIOverlay uiOverlay)
        {
            _zenApplication = zenApplication;
            _gameTaskManager = gameTaskManager;
            _sceneService = sceneService;

            _uiManager = uiManager;
            _uiOverlay = uiOverlay;
        }
        
        protected override void OnRegister()
        {

        }

        protected override async UniTask OnEnter(object data)
        {
            _applicationCamera = _zenApplication.ApplicationCamera;
            
            // Cleanup
            _gameTaskManager.Clear();
            
            // Load Unity Objects
            Watch.Tik();
            ZenLog.Info(LogCategory.System, "[UIWorkState]: Loading UIWorkRoot");
            _sceneRoot = await _sceneService.LoadPrefabAsync("Prefabs/UIWorkRoot");
            ZenLog.Info(LogCategory.System, $"[UIWorkState]: UIWorkRoot Loaded In: {Watch.Tok()} s");
            
            // Disable support camera since this state have a camera.
            _applicationCamera.gameObject.SetActive(false);
            
            _root = _sceneRoot.GetComponent<UIWorkRoot>();

            var cameraData = _root.Camera.Camera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Clear();
            cameraData.cameraStack.Add(_uiManager.CanvasCamera);
            cameraData.cameraStack.Add(_uiOverlay.Camera);
            
            var panel = _uiManager.AttachPopup<UIListPanel>();
            panel.Show();
            
            // Register input event
            SetupInput();
            
            // Register scheduled Actions
            // AutoSchedule(TimeSpan.FromSeconds(5), SaveWorldScheduled);
        }
        
        protected override UniTask OnLeave()
        {
            ClearInput();
            
            if (_sceneRoot != null)
            {
                _sceneService.ReleasePrefab(_sceneRoot);
                _sceneRoot = null;
            }

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
        
        protected override void OnUpdate(float deltaTime)
        {
            // Update GamePlay
            _gameTaskManager.Tick();
        }
        
        private void OnPinchZoom(float zoom)
        {

            ZenLog.Info(LogCategory.Gameplay, $"[GamePlayState] -> Not implemented yet");
        }
    }
}
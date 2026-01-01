using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zen.Debug;
using Zenject;

namespace StrangeSpace
{
    public class ZenApplication
    {
        private ApplicationRoot     _applicationRoot;
        private Camera              _applicationCamera;
        private GameStateMachine    _stateMachine;
        
        // Dependencies
        private IFileSystemProvider _fileSystemProvider;
        private IEventService       _eventService;
        private IZenTaskManager     _taskManager;
        private InputManager        _inputManager;
        private ISceneService       _sceneService;
        private ITimeManager        _timeManager;
        private ISaveManager        _saveManager;
        
        // Game Dependencies
        private IGameTaskManager    _gameTaskManager;
        
        private TrackerService      _trackerService;
        private PrefabRegistry      _prefabRegistry;
        private ItemDatabase        _itemDatabase;
        private GameDatabase        _gameDatabase;
        
        // Singletons as dependencies
        private UIManager _uiManager;
        private UIOverlay _uiOverlay;
        
        private bool _initialized;
        
        // If this application is already deinitializing
        private bool _deinitializing;
        
        // DI Container
        // TODO: use one
        private readonly DiContainer _coreContainer = new DiContainer();
        
        // Get / Set
        public ApplicationRoot ApplicationRoot => _applicationRoot;
        
        public IFileSystemProvider FileSystemProvider => _fileSystemProvider;
        public IEventService EventService => _eventService;
        public IZenTaskManager TaskManager => _taskManager;
        public ITimeManager TimeManager => _timeManager;
        public IGameTaskManager GameTaskManager => _gameTaskManager;
        public ISceneService SceneService => _sceneService;
        public ISaveManager SaveManager => _saveManager;
        public UIManager UIManager => _uiManager;
        public UIOverlay UIOverlay => _uiOverlay;
        
        public ItemDatabase ItemDatabase => _itemDatabase;
        public GameDatabase GameDatabase => _gameDatabase;
        
        public Camera ApplicationCamera => _applicationCamera;

        public ApplicationSettings ApplicationSettings => _applicationRoot.ApplicationSettings;
        public GameSettings GameSettings => _applicationRoot.GameSettings;
        
        // Add state machine.
        public void Initialize(ApplicationRoot applicationRoot)
        {
            if (_initialized)
            {
                ZenLog.Error(LogCategory.System, "[ZenApplication]: Application already initialized");
                return;
            }

            ClearEditorToolDependencies();
            
            _applicationRoot = applicationRoot;
            _applicationCamera  = _applicationRoot.ApplicationCamera;
            
            var singletonRoot = _applicationRoot.SingletonRoot;
            
            // Create Dependencies Container
            RegisterDependencies();
            
            BindRootSystems(singletonRoot);
            
            InitializeSystems();
            
            // Create Main State Machine
            InitializeStateMachine();
            
            _stateMachine.MoveToState<MainMenuState>();
            
            // Initialize Dependencies of Editor Tools. Better if called last.
            InitializeEditorToolDependencies();
            
            _initialized = true;
        }

   
        private void BindRootSystems(SingletonRoot singletonRoot)
        {
            // Bind Pure systems
            _sceneService  .BindToRoot(singletonRoot);
            _saveManager   .BindToRoot(singletonRoot);
            
            // Bind Singletons
            InputManager   .BindToRoot(singletonRoot);
            ItemDatabase   .BindToRoot(singletonRoot);
            GameDatabase   .BindToRoot(singletonRoot);
            TrackerService .BindToRoot(singletonRoot);
            PrefabRegistry .BindToRoot(singletonRoot);
            
            UIManager      .BindToRoot(singletonRoot);
            UIOverlay      .BindToRoot(singletonRoot);
        }
        
        private void InitializeSystems()
        {
            // Set dependencies for not pure systems
            _uiManager.SetDependencies(_eventService, _trackerService);
            _uiOverlay.SetDependencies(_eventService, _trackerService);
            
            // Initialize Pure Services
            _saveManager.Initialize();
            _sceneService.Initialize();
            
            // Initialize Singleton 
            InputManager.Initialize();
            ItemDatabase.Initialize();
            GameDatabase.Initialize();
            TrackerService.Initialize();
            PrefabRegistry.Initialize();
            
            UIManager.Initialize();
            UIOverlay.Initialize();
        }
        
        private void DeinitializeSystems()
        {
            // Deinitialize Singleton 
            UIOverlay.Deinitialize();
            UIManager.Deinitialize();
           
            PrefabRegistry.Deinitialize();
            TrackerService.Deinitialize();
            GameDatabase.Deinitialize();
            ItemDatabase.Deinitialize();
            InputManager.Deinitialize();
            
            // Initialize Pure Services
            _sceneService.Deinitialize();
            _saveManager.Deinitialize();
        }

        public void Deinitialize()
        {
            if (!_initialized)
            {
                ZenLog.Error(LogCategory.System, "[ZenApplication]: Application not initialized");
                return;
            }

            // Idempotency
            if (_deinitializing)
                return;
            
            _deinitializing = true;

            
            DeinitializeEditorToolDependencies();
            
            DeinitializeStateMachine();
            
            DeinitializeSystems();
            
            
            _initialized = false;
            
        }

      
        private void ClearEditorToolDependencies()
        {
            #if UNITY_EDITOR
                EditorDependencies.Clear();
            #endif
        }

        // Initialize the access points of editor tools.
        private void InitializeEditorToolDependencies()
        {
            #if UNITY_EDITOR

                EditorDependencies.Application     = this;
                EditorDependencies.ZenTaskManager  = _taskManager;
                EditorDependencies.GameTaskManager = _gameTaskManager;
                EditorDependencies.TimeManager     = _timeManager;
                
               // EditorDependencies.AddInspectorProvider(_stateMachine);
                //EditorDependencies.AddInspectorProvider(_eventService as EventService);
                
            #endif
        }
        
        private void DeinitializeEditorToolDependencies()
        {
            #if UNITY_EDITOR
                EditorDependencies.Clear();
            #endif
        }

        private void InitializeStateMachine()
        {
            _stateMachine = new GameStateMachine(this);

            var gameplayState = new GameplayState(this, 
                                                              _taskManager, 
                                                              _sceneService, 
                                                              _gameTaskManager, 
                                                              _timeManager,
                                                              _saveManager, 
                                                              _gameDatabase, 
                                                              _uiManager, 
                                                              _uiOverlay);

            var mainMenuState = new MainMenuState(_eventService, 
                                                  _saveManager);
            
            var uiWorkState = new UIWorkState(this,
                                                          _sceneService,
                                                          _gameTaskManager,
                                                          _uiManager,
                                                          _uiOverlay);
            
            // Create Game States
            _stateMachine.RegisterState<GameplayState>(gameplayState);
            _stateMachine.RegisterState<MainMenuState>(mainMenuState);
            _stateMachine.RegisterState<UIWorkState>(uiWorkState);
        }

        private void DeinitializeStateMachine()
        {
            _stateMachine.UnregisterState<UIWorkState>();
            _stateMachine.UnregisterState<MainMenuState>();
            _stateMachine.UnregisterState<GameplayState>();
            
            _stateMachine = null;
        }

        private void RegisterDependencies()
        { 
            // Create and get Plain Singleton Instances
            _inputManager   = InputManager.Create();
            _itemDatabase   = ItemDatabase.Create();
            _gameDatabase   = GameDatabase.Create();
            _trackerService = TrackerService.Create();
            _prefabRegistry = PrefabRegistry.Create();
            
            _uiManager      = UIManager.Create();
            _uiOverlay      = UIOverlay.Create();
            
            // Bind Core Pure Dependencies
            _coreContainer.Bind<IFileSystemProvider>().To<FileSystemProvider>().AsSingle().NonLazy();
            _coreContainer.Bind<IEventService>()      .To<EventService>()      .AsSingle().NonLazy();
            _coreContainer.Bind<IZenTaskManager>()    .To<ZenTaskManager>()    .AsSingle().NonLazy();
            _coreContainer.Bind<ITimeManager>()       .To<TimeManager>()       .AsSingle().NonLazy();
            _coreContainer.Bind<ISceneService>()      .To<SceneService>()      .AsSingle().NonLazy();
            _coreContainer.Bind<ISaveManager>()       .To<SaveManager>()       .AsSingle().NonLazy();
            _coreContainer.Bind<IGameTaskManager>()   .To<GameTaskManager>()   .AsSingle().NonLazy();
            
            // Bind Singletons Dependencies outside DI Containers.
            _coreContainer.Bind<UIManager>()          .To<UIManager>().FromInstance(_uiManager);
            _coreContainer.Bind<UIOverlay>()          .To<UIOverlay>().FromInstance(_uiOverlay);
            
            // Bind Game Dependencies
            _coreContainer.Bind<ItemDatabase>()       .To<ItemDatabase>().FromInstance(_itemDatabase);
            _coreContainer.Bind<GameDatabase>()       .To<GameDatabase>().FromInstance(_gameDatabase);
            _coreContainer.Bind<TrackerService>()     .To<TrackerService>().FromInstance(_trackerService);
            _coreContainer.Bind<PrefabRegistry>()     .To<PrefabRegistry>().FromInstance(_prefabRegistry);
            
            // Resolve Core Dependencies
            _fileSystemProvider = _coreContainer.Resolve<IFileSystemProvider>();
            _eventService       = _coreContainer.Resolve<IEventService>();
            _taskManager        = _coreContainer.Resolve<IZenTaskManager>();
            _timeManager        = _coreContainer.Resolve<ITimeManager>();
            _sceneService       = _coreContainer.Resolve<ISceneService>();
            _gameTaskManager    = _coreContainer.Resolve<IGameTaskManager>();
            _saveManager        = _coreContainer.Resolve<ISaveManager>();
            
        }
        
        public void Update(float deltaTime)
        {
           // TODO: Allow updates in different frequency
           _taskManager.Update();
           _stateMachine.Update(deltaTime);
        }
    }
}